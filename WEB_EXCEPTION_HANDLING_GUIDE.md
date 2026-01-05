# Web Layer Global Exception Handling & Logging

## 📋 Overview

I have implemented **comprehensive global exception handling** in your Web layer. Now, any error that occurs—whether it's a crash, a missing page (404), or an access denied error (403)—will be handled gracefully.

## ✅ Key Features Implemented

### 1. Global Exception Middleware 🛡️
**File:** `WebExceptionLoggingMiddleware.cs`
- **Catches ALL exceptions** in the application.
- **Logs exceptions to the database** (via `ErrorLogs` table).
- **Gracefully redirects** users to user-friendly error pages:
  - `UnauthorizedAccessException` → **Access Denied Page**
  - `KeyNotFoundException` → **Not Found Page**
  - Other Exceptions → **Generic Error Page**

### 2. Status Code Handling (404, 403, etc.) 🚥
**File:** `Program.cs`
- Added `app.UseStatusCodePagesWithReExecute("/Error/{0}");`
- This catches non-exception errors (like typing a wrong URL) and shows the custom Not Found page instead of a generic browser error.

### 3. Centralized Error Controller 🎮
**File:** `ErrorController.cs`
- Manages all error routes:
  - `/Error` (500)
  - `/Error/NotFound` (404)
  - `/Error/AccessDenied` (403)
  - `/Error/{statusCode}` (Universal handler)

### 4. Beautiful Error Views 🎨
Created modern, user-friendly Razor views with icons and helpful navigation:
- **`Views/Error/Index.cshtml`**: Generic error page (Red theme).
- **`Views/Error/NotFound.cshtml`**: 404 page (Yellow theme, Search icon).
- **`Views/Error/AccessDenied.cshtml`**: 403 page (Red theme, Lock icon).

### 5. Consistent Authorization 🔐
**File:** `AuthorizationFilters.cs`
- Updated authorization filters to redirect unauthorized users to the global **Access Denied** page instead of the old Admin-specific page.

---

## 🏗️ How it Works

### Scenario 1: A page crashes (Exception)
1. Exception occurs (e.g., Database connection fails).
2. `WebExceptionLoggingMiddleware` catches it.
3. Log is saved to `ErrorLogs` table with **UserId** and **Stack Trace**.
4. User is redirected to `/Error/Index`.
5. User sees a friendly "Oops! Something went wrong" page with a Request ID.

### Scenario 2: User types wrong URL (404)
1. No matching route found.
2. `StatusCodePagesWithReExecute` intercepts the 404 status.
3. Internally redirects to `/Error/404`.
4. `ErrorController` executes and returns `Views/Error/NotFound.cshtml`.
5. User sees "Page Not Found" with a home button.

### Scenario 3: User tries to access Admin area without rights (403)
1. `AuthorizeRoleAttribute` detects insufficient permissions.
2. Redirects to `/Error/AccessDenied`.
3. User sees "Access Denied" page with a Lock icon.

---

## 📝 Files Modified/Created

| File | Type | Description |
|------|------|-------------|
| `WebExceptionLoggingMiddleware.cs` | Middleware | Enhanced to redirect to error pages. |
| `Program.cs` | Config | Added status code pages and updated exception handler. |
| `ErrorController.cs` | Controller | **NEW** Controller for error pages. |
| `ErrorViewModel.cs` | Model | Added `ErrorMessage` and `StatusCode`. |
| `Views/Error/Index.cshtml` | View | **NEW** Generic error view. |
| `Views/Error/NotFound.cshtml` | View | **NEW** 404 view. |
| `Views/Error/AccessDenied.cshtml`| View | **NEW** 403 view. |
| `AuthorizationFilters.cs` | Filter | Updated redirect path. |

---

## 🚀 How to Test

1. **Test Generic Error**: Throw an exception in any controller action.
   - You can use the `/Home/Privacy` action to temporarily throw `new Exception("Test")`.
   - Result: Should see the Red Error Page and find a log in `ErrorLogs` database table.

2. **Test 404**: Type a random URL (e.g., `/RandomPage123`).
   - Result: Should see the Yellow "Page Not Found" view.

3. **Test Access Denied**: Log in as a regular user and try to access `/Admin/Dashboard`.
   - Result: Should see the "Access Denied" view.
