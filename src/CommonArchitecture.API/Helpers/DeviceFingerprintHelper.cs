using System.Security.Cryptography;
using System.Text;

namespace CommonArchitecture.API.Helpers;

public static class DeviceFingerprintHelper
{
    public static string GenerateFingerprint(string? userAgent, string? ipAddress)
    {
        var combined = $"{userAgent ?? "unknown"}|{ipAddress ?? "unknown"}";
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static string? GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        // Check for real IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}

