namespace CommonArchitecture.Core.Entities;

public class ErrorLog
{
 public int Id { get; set; }
 public string Message { get; set; } = string.Empty;
 public string? StackTrace { get; set; }
 public string Path { get; set; } = string.Empty;
 public string Method { get; set; } = string.Empty;
 public string? QueryString { get; set; }
 public string? UserId { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
