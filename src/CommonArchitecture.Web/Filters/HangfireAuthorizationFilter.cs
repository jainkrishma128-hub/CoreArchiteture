using Hangfire.Dashboard;

namespace CommonArchitecture.Web.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Check if user is authenticated via session
        var userId = httpContext.Session.GetString("UserId");
        var userRoleId = httpContext.Session.GetString("UserRoleId");
        
        // Only Admin (RoleId = 1) can access Hangfire dashboard
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRoleId))
        {
            return false;
        }
        
        if (int.TryParse(userRoleId, out int roleId) && roleId == 1) // Admin role
        {
            return true;
        }
        
        return false;
    }
}

