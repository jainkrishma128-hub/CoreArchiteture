using CommonArchitecture.Application.DTOs;
using CommonArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Text.Json;

namespace CommonArchitecture.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController : Controller
{
    private readonly IAuthApiService _authApiService;
    private readonly IToastNotification _toastNotification;

    public AuthController(IAuthApiService authApiService, IToastNotification toastNotification)
    {
        _authApiService = authApiService;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // If already logged in, redirect to dashboard
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto request)
    {
        var result = await _authApiService.SendOtpAsync(request);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authApiService.LoginAsync(request);

        if (result.Success && result.User != null)
        {
            // Store user information in session
            HttpContext.Session.SetString("UserId", result.User.Id.ToString());
            HttpContext.Session.SetString("UserName", result.User.Name);
            HttpContext.Session.SetString("UserEmail", result.User.Email);
            HttpContext.Session.SetString("UserMobile", result.User.Mobile);
            HttpContext.Session.SetString("UserRoleId", result.User.RoleId.ToString());
            HttpContext.Session.SetString("UserRoleName", result.User.RoleName ?? "Unknown");
            
            if (!string.IsNullOrEmpty(result.User.ProfileImagePath))
            {
                HttpContext.Session.SetString("UserProfileImage", result.User.ProfileImagePath);
            }

            // Serialize user object for easy access
            var userJson = JsonSerializer.Serialize(result.User);
            HttpContext.Session.SetString("User", userJson);
        }

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authApiService.LogoutAsync();
        
        // Clear session
        HttpContext.Session.Clear();
        
        _toastNotification.AddSuccessToastMessage("Logged out successfully!");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
