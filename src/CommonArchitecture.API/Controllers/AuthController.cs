using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Application.Services;
using CommonArchitecture.API.Helpers;
using CommonArchitecture.Core.Entities;
using CommonArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace CommonArchitecture.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private const string FIXED_OTP = "1234";

    public AuthController(
        IAuthRepository authRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authRepository = authRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("send-otp")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<SendOtpResponseDto>> SendOtp([FromBody] SendOtpRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Mobile))
        {
            return BadRequest(new SendOtpResponseDto
            {
                Success = false,
                Message = "Mobile number is required"
            });
        }

        // Validate mobile number format (10 digits)
        if (request.Mobile.Length != 10 || !request.Mobile.All(char.IsDigit))
        {
            return BadRequest(new SendOtpResponseDto
            {
                Success = false,
                Message = "Please enter a valid 10-digit mobile number"
            });
        }

        // Check if user exists with this mobile number
        var user = await _authRepository.GetUserByMobileAsync(request.Mobile);
        if (user == null)
        {
            _logger.LogWarning("OTP request for non-existent mobile: {Mobile}", request.Mobile);
            return NotFound(new SendOtpResponseDto
            {
                Success = false,
                Message = "User not found with this mobile number"
            });
        }

        var ipAddress = DeviceFingerprintHelper.GetClientIpAddress(HttpContext);
        _logger.LogInformation("OTP sent to user {UserId} (Mobile: {Mobile}) from IP: {IpAddress}", 
            user.Id, request.Mobile, ipAddress);

        // In production, send OTP via SMS service
        // For now, return fixed OTP in development
        return Ok(new SendOtpResponseDto
        {
            Success = true,
            Message = "OTP sent successfully",
            Otp = FIXED_OTP // Remove this in production
        });
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Mobile) || string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(new LoginResponseDto
            {
                Success = false,
                Message = "Mobile number and OTP are required"
            });
        }

        var ipAddress = DeviceFingerprintHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        // Validate OTP
        if (request.Otp != FIXED_OTP)
        {
            _logger.LogWarning("Invalid OTP attempt for mobile: {Mobile} from IP: {IpAddress}", 
                request.Mobile, ipAddress);
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                Message = "Invalid OTP"
            });
        }

        // Get user by mobile
        var user = await _authRepository.GetUserByMobileAsync(request.Mobile);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent mobile: {Mobile} from IP: {IpAddress}", 
                request.Mobile, ipAddress);
            return NotFound(new LoginResponseDto
            {
                Success = false,
                Message = "User not found"
            });
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        // Generate device fingerprint
        var deviceFingerprint = DeviceFingerprintHelper.GenerateFingerprint(userAgent, ipAddress);

        // Save refresh token to database with device/IP info
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        _logger.LogInformation("User {UserId} (Mobile: {Mobile}) logged in successfully from IP: {IpAddress}, Device: {DeviceFingerprint}", 
            user.Id, request.Mobile, ipAddress, deviceFingerprint);

        // Map to UserDto
        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Mobile = user.Mobile,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty,
            ProfileImagePath = user.ProfileImagePath
        };

        return Ok(new LoginResponseDto
        {
            Success = true,
            Message = "Login successful",
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes)
        });
    }

    [HttpPost("refresh-token")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new RefreshTokenResponseDto
            {
                Success = false,
                Message = "Refresh token is required"
            });
        }

        var ipAddress = DeviceFingerprintHelper.GetClientIpAddress(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        // Get refresh token from database
        var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token attempt from IP: {IpAddress}", ipAddress);
            return Unauthorized(new RefreshTokenResponseDto
            {
                Success = false,
                Message = "Invalid or expired refresh token"
            });
        }

        // Token reuse detection - check if this token was already used
        if (await _refreshTokenRepository.IsTokenReusedAsync(request.RefreshToken, refreshTokenEntity.UserId))
        {
            _logger.LogWarning("Token reuse detected for user {UserId} from IP: {IpAddress}. Revoking all tokens.", 
                refreshTokenEntity.UserId, ipAddress);
            
            // Security: Revoke all user tokens if reuse detected
            await _refreshTokenRepository.RevokeAllUserTokensAsync(refreshTokenEntity.UserId);
            
            return Unauthorized(new RefreshTokenResponseDto
            {
                Success = false,
                Message = "Token reuse detected. Please login again."
            });
        }

        // Get user
        var user = refreshTokenEntity.User;
        if (user == null)
        {
            _logger.LogError("User not found for refresh token");
            return Unauthorized(new RefreshTokenResponseDto
            {
                Success = false,
                Message = "User not found"
            });
        }

        // Verify device fingerprint matches (optional but recommended)
        var deviceFingerprint = DeviceFingerprintHelper.GenerateFingerprint(userAgent, ipAddress);
        if (!string.IsNullOrEmpty(refreshTokenEntity.DeviceFingerprint) && 
            refreshTokenEntity.DeviceFingerprint != deviceFingerprint)
        {
            _logger.LogWarning("Device fingerprint mismatch for user {UserId}. Expected: {Expected}, Got: {Actual}", 
                user.Id, refreshTokenEntity.DeviceFingerprint, deviceFingerprint);
            // Log but don't block - IPs can change (mobile networks, VPNs)
        }

        // Revoke old refresh token and store it as PreviousToken for reuse detection
        var oldToken = refreshTokenEntity.Token;
        await _refreshTokenRepository.RevokeTokenAsync(request.RefreshToken);

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
        var accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");

        // Save new refresh token with previous token reference
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            PreviousToken = oldToken, // Store old token for reuse detection
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        _logger.LogInformation("Token refreshed for user {UserId} from IP: {IpAddress}", user.Id, ipAddress);

        return Ok(new RefreshTokenResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes)
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? request = null)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var ipAddress = DeviceFingerprintHelper.GetClientIpAddress(HttpContext);

        // Revoke refresh token if provided
        if (request != null && !string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await _refreshTokenRepository.RevokeTokenAsync(request.RefreshToken);
            _logger.LogInformation("Refresh token revoked during logout from IP: {IpAddress}", ipAddress);
        }

        // Get user ID from claims
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            // Revoke all user tokens
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
            _logger.LogInformation("User {UserId} logged out. All tokens revoked. IP: {IpAddress}", userId, ipAddress);
        }

        return Ok(new { success = true, message = "Logout successful" });
    }
}
