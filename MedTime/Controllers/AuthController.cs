using MedTime.Helpers;
using MedTime.Models.DTOs;
using MedTime.Models.Entities;
using MedTime.Models.Requests;
using MedTime.Models.Responses;
using MedTime.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedTime.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<UserDto>.ErrorResponse(
                    errors,
                    "Validation failed",
                    400
                );
                return BadRequest(errorResponse);
            }

            try
            {
                var user = await _authService.RegisterAsync(request);
                if (user == null)
                {
                    var errorResponse = ApiResponse<UserDto>.ErrorResponse(
                        "Username already exists",
                        "Registration failed",
                        400
                    );
                    return BadRequest(errorResponse);
                }

                var successResponse = ApiResponse<UserDto>.SuccessResponse(
                    user,
                    "User registered successfully. Please login to continue.",
                    201
                );
                return StatusCode(201, successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<UserDto>.ErrorResponse(
                    ex.Message,
                    "An error occurred during registration",
                    500
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                var errorResponse = ApiResponse<AuthResponse>.ErrorResponse(
                    "Invalid username or password",
                    "Login failed",
                    401
                );
                return Unauthorized(errorResponse);
            }

            var successResponse = ApiResponse<AuthResponse>.SuccessResponse(
                response,
                "Login successful",
                200
            );
            return Ok(successResponse);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (response == null)
            {
                var errorResponse = ApiResponse<AuthResponse>.ErrorResponse(
                    "Invalid or expired refresh token",
                    "Token refresh failed",
                    401
                );
                return Unauthorized(errorResponse);
            }

            var successResponse = ApiResponse<AuthResponse>.SuccessResponse(
                response,
                "Token refreshed successfully",
                200
            );
            return Ok(successResponse);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var result = await _authService.LogoutAsync(userId);
            
            if (!result)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    "Logout failed",
                    "An error occurred during logout",
                    400
                );
                return BadRequest(errorResponse);
            }

            var successResponse = ApiResponse<object>.SuccessResponse(
                new { userId },
                "Successfully logged out",
                200
            );
            return Ok(successResponse);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    errors,
                    "Validation failed",
                    400
                );
                return BadRequest(errorResponse);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var result = await _authService.ChangePasswordAsync(userId, request);

                if (!result)
                {
                    var errorResponse = ApiResponse<object>.ErrorResponse(
                        "Current password is incorrect",
                        "Change password failed",
                        400
                    );
                    return BadRequest(errorResponse);
                }

                var successResponse = ApiResponse<object>.SuccessResponse(
                    new { userId },
                    "Password changed successfully. Please login again with your new password.",
                    200
                );
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.ErrorResponse(
                    ex.Message,
                    "An error occurred while changing password",
                    500
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
}
