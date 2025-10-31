using AutoMapper;
using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Enums;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Repositories;
using MedTime.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MedTime.Services
{
    public class PaymentService
    {
        private readonly PaymenthistoryRepo _paymentRepo;
        private readonly PremiumplanRepo _planRepo;
        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;
        private readonly PayOSSettings _payosSettings;
        private readonly HttpClient _httpClient;

        public PaymentService(
            PaymenthistoryRepo paymentRepo,
            PremiumplanRepo planRepo,
            UserRepo userRepo,
            IMapper mapper,
            IOptions<PayOSSettings> payosSettings,
            IHttpClientFactory httpClientFactory)
        {
            _paymentRepo = paymentRepo;
            _planRepo = planRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _payosSettings = payosSettings.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        #region Get Plans
        public async Task<List<PremiumplanDto>> GetActivePlansAsync()
        {
            var plans = await _planRepo.GetActivePlansAsync();
            var planDtos = _mapper.Map<List<PremiumplanDto>>(plans);

            // Calculate final price with discount
            foreach (var planDto in planDtos)
            {
                if (planDto.Discountpercent.HasValue && planDto.Discountpercent.Value > 0)
                {
                    planDto.Finalprice = planDto.Price * (1 - planDto.Discountpercent.Value / 100);
                }
                else
                {
                    planDto.Finalprice = planDto.Price;
                }
            }

            return planDtos;
        }
        #endregion

        #region Create Payment Link
        public async Task<CreatePaymentResponse?> CreatePaymentLinkAsync(int userId, CreatePaymentRequest request)
        {
            Console.WriteLine($"[Payment] Starting payment creation for userId={userId}, planId={request.PlanId}");
            
            // Get plan
            var plan = await _planRepo.GetPlanByIdAsync(request.PlanId);
            if (plan == null)
            {
                Console.WriteLine($"[Payment] Plan not found: planId={request.PlanId}");
                throw new Exception("Plan not found");
            }

            Console.WriteLine($"[Payment] Plan found: {plan.Planname}, Price={plan.Price}");

            // Calculate final price
            var finalPrice = plan.Price;
            if (plan.Discountpercent.HasValue && plan.Discountpercent.Value > 0)
            {
                finalPrice = plan.Price * (1 - plan.Discountpercent.Value / 100);
            }

            Console.WriteLine($"[Payment] Final price calculated: {finalPrice}");

            // Generate unique order ID
            var orderId = GenerateOrderId();
            Console.WriteLine($"[Payment] Generated orderId: {orderId}");

            // Call PayOS API to create payment link
            Console.WriteLine($"[Payment] Calling PayOS API...");
            Console.WriteLine($"[Payment] PayOS Config - ClientId: {_payosSettings.ClientId}");
            Console.WriteLine($"[Payment] PayOS Config - BaseUrl: {_payosSettings.BaseUrl}");
            Console.WriteLine($"[Payment] PayOS Config - ApiKey: {(_payosSettings.ApiKey?.Length > 0 ? "***" + _payosSettings.ApiKey.Substring(_payosSettings.ApiKey.Length - 4) : "NULL")}");
            
            var payosResponse = await CreatePayOSPaymentLink(
                orderId,
                finalPrice,
                plan.Planname,
                request.ReturnUrl ?? _payosSettings.ReturnUrl,
                request.CancelUrl ?? _payosSettings.CancelUrl
            );
            
            if (payosResponse == null)
            {
                Console.WriteLine($"[Payment] ❌ PayOS API returned null - Payment creation failed");
                throw new Exception("Failed to create payment link from PayOS");
            }

            Console.WriteLine($"[Payment] ✅ PayOS API success - CheckoutUrl: {payosResponse.CheckoutUrl}");

            // Create payment history record
            var payment = new Paymenthistory
            {
                Userid = userId,
                Planid = plan.Planid,
                Orderid = orderId,
                Amount = finalPrice,
                Status = PaymentStatusEnum.PENDING,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                Updatedat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            await _paymentRepo.CreateAsync(payment);
            Console.WriteLine($"[Payment] Payment record created in database");

            return new CreatePaymentResponse
            {
                OrderId = orderId,
                CheckoutUrl = payosResponse.CheckoutUrl,
                QrCode = payosResponse.QrCode,
                Amount = finalPrice,
                PlanName = plan.Planname
            };
        }

        private async Task<PayOSCreatePaymentLinkResponse?> CreatePayOSPaymentLink(
            string orderId,
            decimal amount,
            string description,
            string returnUrl,
            string cancelUrl)
        {
            try
            {
                // Validate URLs - nếu FE truyền "string" thì dùng default từ config
                var validReturnUrl = (string.IsNullOrEmpty(returnUrl) || returnUrl == "string") 
                    ? _payosSettings.ReturnUrl 
                    : returnUrl;
                
                var validCancelUrl = (string.IsNullOrEmpty(cancelUrl) || cancelUrl == "string") 
                    ? _payosSettings.CancelUrl 
                    : cancelUrl;

                Console.WriteLine($"[PayOS] Using returnUrl: {validReturnUrl}");
                Console.WriteLine($"[PayOS] Using cancelUrl: {validCancelUrl}");

                // Generate signature
                var orderCodeLong = long.Parse(orderId);
                var signature = GeneratePaymentSignature(
                    orderCodeLong, 
                    (int)amount, 
                    description, 
                    validCancelUrl, 
                    validReturnUrl
                );

                var requestBody = new
                {
                    orderCode = orderCodeLong,
                    amount = (int)amount,
                    description = description,
                    returnUrl = validReturnUrl,
                    cancelUrl = validCancelUrl,
                    signature = signature,
                    items = new[]
                    {
                        new
                        {
                            name = description,
                            quantity = 1,
                            price = (int)amount
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[PayOS] Request Body: {jsonContent}");

                var url = $"{_payosSettings.BaseUrl}/v2/payment-requests";
                Console.WriteLine($"[PayOS] Calling URL: {url}");
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("x-client-id", _payosSettings.ClientId);
                httpRequest.Headers.Add("x-api-key", _payosSettings.ApiKey);
                httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                Console.WriteLine($"[PayOS] Sending request...");
                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[PayOS] Response Status: {response.StatusCode}");
                Console.WriteLine($"[PayOS] Response Body: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[PayOS] ❌ HTTP Error {response.StatusCode}: {responseContent}");
                    return null;
                }

                var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                Console.WriteLine($"[PayOS] Parsed Response - Code: {payosResponse?.Code}, Desc: {payosResponse?.Desc}");

                if (payosResponse?.Code == "00" && payosResponse.Data != null)
                {
                    Console.WriteLine($"[PayOS] ✅ Success - CheckoutUrl: {payosResponse.Data.CheckoutUrl}");
                    return new PayOSCreatePaymentLinkResponse
                    {
                        CheckoutUrl = payosResponse.Data.CheckoutUrl,
                        QrCode = payosResponse.Data.QrCode,
                        PaymentLinkId = payosResponse.Data.PaymentLinkId
                    };
                }

                Console.WriteLine($"[PayOS] ❌ Invalid response code or missing data");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PayOS] ❌ Exception: {ex.Message}");
                Console.WriteLine($"[PayOS] StackTrace: {ex.StackTrace}");
                return null;
            }
        }
        #endregion

        #region Check Payment Status
        public async Task<PaymentStatusResponse?> GetPaymentStatusAsync(string orderId)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null)
                return null;

            // Also check with PayOS API for latest status
            var payosStatus = await CheckPayOSPaymentStatus(orderId);
            
            if (payosStatus != null && payosStatus.Status == "PAID" && payment.Status != PaymentStatusEnum.PAID)
            {
                // Update if PayOS says paid but our DB doesn't
                await ProcessSuccessfulPayment(orderId, payosStatus.TransactionId);
                payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            }

            return new PaymentStatusResponse
            {
                OrderId = payment!.Orderid,
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                PaidAt = payment.Paidat,
                TransactionId = payment.Transactionid
            };
        }

        private async Task<PayOSPaymentStatusResponse?> CheckPayOSPaymentStatus(string orderId)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_payosSettings.BaseUrl}/v2/payment-requests/{orderId}");
                httpRequest.Headers.Add("x-client-id", _payosSettings.ClientId);
                httpRequest.Headers.Add("x-api-key", _payosSettings.ApiKey);

                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return null;

                var payosResponse = JsonSerializer.Deserialize<PayOSApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payosResponse?.Code == "00" && payosResponse.Data != null)
                {
                    return new PayOSPaymentStatusResponse
                    {
                        Status = payosResponse.Data.Status,
                        TransactionId = payosResponse.Data.TransactionId
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking PayOS payment status: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Webhook Handler
        public async Task<bool> HandlePayOSWebhookAsync(string signature, object webhookData)
        {
            try
            {
                // Verify webhook signature
                var dataString = JsonSerializer.Serialize(webhookData);
                var isValid = VerifyWebhookSignature(signature, dataString);

                if (!isValid)
                {
                    Console.WriteLine("Invalid webhook signature");
                    return false;
                }

                // Parse webhook data
                var data = JsonSerializer.Deserialize<PayOSWebhookData>(dataString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null)
                    return false;

                // Process based on status
                if (data.Code == "00") // Success
                {
                    await ProcessSuccessfulPayment(data.OrderCode, data.Reference);
                    return true;
                }
                else if (data.Code == "01") // Cancelled
                {
                    await _paymentRepo.UpdatePaymentStatusAsync(
                        data.OrderCode,
                        PaymentStatusEnum.CANCELLED,
                        payosResponse: dataString
                    );
                    return true;
                }
                else // Failed
                {
                    await _paymentRepo.UpdatePaymentStatusAsync(
                        data.OrderCode,
                        PaymentStatusEnum.FAILED,
                        payosResponse: dataString
                    );
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling webhook: {ex.Message}");
                return false;
            }
        }

        private async Task ProcessSuccessfulPayment(string orderId, string? transactionId)
        {
            var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
            if (payment == null || payment.Status == PaymentStatusEnum.PAID)
                return;

            // Update payment status
            await _paymentRepo.UpdatePaymentStatusAsync(
                orderId,
                PaymentStatusEnum.PAID,
                transactionId
            );

            // Update user premium status
            var premiumStart = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var premiumEnd = premiumStart.AddDays(payment.Plan.Durationdays);

            await _userRepo.UpdatePremiumStatusAsync(
                payment.Userid,
                true,
                premiumStart,
                premiumEnd
            );
        }
        #endregion

        #region Payment History
        public async Task<List<PaymenthistoryDto>> GetPaymentHistoryAsync(int userId)
        {
            var payments = await _paymentRepo.GetUserPaymentHistoryAsync(userId);
            return _mapper.Map<List<PaymenthistoryDto>>(payments);
        }
        #endregion

        #region Cancel Subscription
        public async Task<bool> CancelSubscriptionAsync(int userId)
        {
            // Set premium to false and clear dates
            return await _userRepo.UpdatePremiumStatusAsync(userId, false, null, null);
        }
        #endregion

        #region Helper Methods
        private string GenerateOrderId()
        {
            // Generate unique order ID (max 9 digits for PayOS)
            // Use last 9 digits of Unix timestamp in seconds + random 3 digits
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var random = new Random().Next(100, 999);
            var orderId = long.Parse($"{timestamp % 1000000}{random}");
            return orderId.ToString();
        }

        private string GeneratePaymentSignature(long orderCode, decimal amount, string description, string cancelUrl, string returnUrl)
        {
            // Tạo data string theo format PayOS (sắp xếp theo alphabet)
            var dataString = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            
            Console.WriteLine($"[PayOS] Signature Data String: {dataString}");
            
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_payosSettings.ChecksumKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
                var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                Console.WriteLine($"[PayOS] Generated Signature: {signature}");
                return signature;
            }
        }

        private string GenerateSignature(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_payosSettings.ChecksumKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private bool VerifyWebhookSignature(string signature, string data)
        {
            var expectedSignature = GenerateSignature(data);
            return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Internal Response Classes
        private class PayOSApiResponse
        {
            public string Code { get; set; } = null!;
            public string Desc { get; set; } = null!;
            public PayOSData? Data { get; set; }
        }

        private class PayOSData
        {
            public string CheckoutUrl { get; set; } = null!;
            public string QrCode { get; set; } = null!;
            public string PaymentLinkId { get; set; } = null!;
            public string Status { get; set; } = null!;
            public string? TransactionId { get; set; }
        }

        private class PayOSCreatePaymentLinkResponse
        {
            public string CheckoutUrl { get; set; } = null!;
            public string QrCode { get; set; } = null!;
            public string PaymentLinkId { get; set; } = null!;
        }

        private class PayOSPaymentStatusResponse
        {
            public string Status { get; set; } = null!;
            public string? TransactionId { get; set; }
        }

        private class PayOSWebhookData
        {
            public string Code { get; set; } = null!;
            public string Desc { get; set; } = null!;
            public string OrderCode { get; set; } = null!;
            public string? Reference { get; set; }  // Transaction ID
            public decimal Amount { get; set; }
            public string Status { get; set; } = null!;
        }
        #endregion
    }
}
