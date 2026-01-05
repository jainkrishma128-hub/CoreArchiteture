namespace CommonArchitecture.Core.Interfaces;

public interface INotificationService
{
    Task SendGoodMorningMessageAsync(string userName, string email, string mobile);
}

