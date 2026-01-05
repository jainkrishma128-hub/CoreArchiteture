using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommonArchitecture.Web.Filters;

/// <summary>
/// Authorization filter to ensure user is authenticated
/// </summary>
public class AuthorizeUserAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetString("UserId");
        
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
        }
        
        base.OnActionExecuting(context);
    }
}

/// <summary>
/// Authorization filter for role-based access control
/// Roles: 1 = Admin (Full Access), 2 = Product Manager (Products Only)
/// </summary>
public class AuthorizeRoleAttribute : ActionFilterAttribute
{
    private readonly int[] _allowedRoles;

    public AuthorizeRoleAttribute(params int[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetString("UserId");
        var userRoleId = context.HttpContext.Session.GetString("UserRoleId");
        
        // Check if user is authenticated
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
            return;
        }
        
        // Check if user has required role
        if (!string.IsNullOrEmpty(userRoleId) && int.TryParse(userRoleId, out int roleId))
        {
            if (!_allowedRoles.Contains(roleId))
            {
                // Redirect to global access denied page
                context.Result = new RedirectToActionResult("AccessDenied", "Error", new { area = "" });
                return;
            }
        }
        else
        {
            context.Result = new RedirectToActionResult("Login", "Auth", new { area = "Admin" });
            return;
        }
        
        base.OnActionExecuting(context);
    }
}
