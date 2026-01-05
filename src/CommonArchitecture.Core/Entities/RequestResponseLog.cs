namespace CommonArchitecture.Core.Entities;

public class RequestResponseLog
{
 public int Id { get; set; }
 public string Method { get; set; } = string.Empty;
 public string Path { get; set; } = string.Empty;
 public string? QueryString { get; set; }
 public string? RequestBody { get; set; }
 public int ResponseStatusCode { get; set; }
 public string? ResponseBody { get; set; }
 public long DurationMs { get; set; }
 public string? IpAddress { get; set; }
 public string? UserAgent { get; set; }
 public string? UserId { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
