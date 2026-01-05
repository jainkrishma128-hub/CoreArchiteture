namespace CommonArchitecture.Core.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedAt { get; set; }
    public string? DeviceFingerprint { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? PreviousToken { get; set; } // For reuse detection
}

