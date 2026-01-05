using Hangfire.Dashboard;

namespace CommonArchitecture.API;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In development, allow all access
        // In production, you should implement proper authorization here
        // Example: Check if user is authenticated and has Admin role
        var httpContext = context.GetHttpContext();
        
        // For now, allow in development, deny in production (should use proper auth)
        // You can implement JWT token validation here if needed
        return true; // TODO: Implement proper authorization in production
    }
}

