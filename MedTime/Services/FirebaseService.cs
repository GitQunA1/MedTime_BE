using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace MedTime.Services
{
    /// <summary>
    /// Service để gửi push notification qua Firebase Cloud Messaging (FCM)
    /// </summary>
    public class FirebaseService
    {
        private readonly FirebaseApp _firebaseApp;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;

            if (FirebaseApp.DefaultInstance == null)
            {
                // Đọc từ environment variable (production)
                var credentialJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");

                if (string.IsNullOrEmpty(credentialJson))
                {
                    // Fallback: đọc từ file (local development)
                    var credential = GoogleCredential.FromFile("medtime-e523a-firebase-adminsdk-fbsvc-8d7d70da2d.json");
                    _firebaseApp = FirebaseApp.Create(new AppOptions { Credential = credential });
                }
                else
                {
                    // Production: parse JSON từ env
                    var credential = GoogleCredential.FromJson(credentialJson);
                    _firebaseApp = FirebaseApp.Create(new AppOptions { Credential = credential });
                }

                _logger.LogInformation("Firebase initialized successfully");
            }
            else
            {
                _firebaseApp = FirebaseApp.DefaultInstance;
            }
        }

        /// <summary>
        /// Gửi notification đến 1 device token
        /// </summary>
        public async Task<string> SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Token = deviceToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>(),
                    // Android specific options
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            Sound = "default",
                            ChannelId = "medicine_reminder"
                        }
                    },
                    // iOS specific options
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent notification to {deviceToken}. MessageId: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to {deviceToken}");
                throw;
            }
        }

        /// <summary>
        /// Gửi notification đến nhiều device tokens (max 500 tokens/batch)
        /// </summary>
        public async Task<BatchResponse> SendMulticastNotificationAsync(
            List<string> deviceTokens, 
            string title, 
            string body, 
            Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new MulticastMessage
                {
                    Tokens = deviceTokens,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>(),
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            Sound = "default",
                            ChannelId = "medicine_reminder"
                        }
                    },
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Sound = "default",
                            Badge = 1
                        }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
                _logger.LogInformation($"Successfully sent {response.SuccessCount} notifications out of {deviceTokens.Count}");
                
                if (response.FailureCount > 0)
                {
                    _logger.LogWarning($"Failed to send {response.FailureCount} notifications");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send multicast notification");
                throw;
            }
        }

        /// <summary>
        /// Gửi notification theo topic (subscribe/unsubscribe trên mobile)
        /// </summary>
        public async Task<string> SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>()
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent notification to topic {topic}. MessageId: {response}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification to topic {topic}");
                throw;
            }
        }

        /// <summary>
        /// Subscribe device token vào topic
        /// </summary>
        public async Task<TopicManagementResponse> SubscribeToTopicAsync(List<string> deviceTokens, string topic)
        {
            try
            {
                var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(deviceTokens, topic);
                _logger.LogInformation($"Subscribed {response.SuccessCount} tokens to topic {topic}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to subscribe to topic {topic}");
                throw;
            }
        }

        /// <summary>
        /// Unsubscribe device token khỏi topic
        /// </summary>
        public async Task<TopicManagementResponse> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic)
        {
            try
            {
                var response = await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(deviceTokens, topic);
                _logger.LogInformation($"Unsubscribed {response.SuccessCount} tokens from topic {topic}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unsubscribe from topic {topic}");
                throw;
            }
        }
    }
}
