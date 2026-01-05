# Quick Start Guide - Security Improvements

## 🚀 Setup Steps

### 1. Set JWT Secret Key (Required First Step)

**For Development (User Secrets):**
```bash
cd src/CommonArchitecture.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm!"
```

**For Production (Environment Variables):**
```bash
# Windows PowerShell
$env:Jwt__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm!"

# Linux/macOS
export Jwt__SecretKey="YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm!"
```

### 2. Run Database Migration

```bash
cd src/CommonArchitecture.Infrastructure
dotnet ef migrations add AddRefreshTokenSecurityFields --startup-project ../CommonArchitecture.API
dotnet ef database update --startup-project ../CommonArchitecture.API
```

### 3. Build and Run

```bash
# Build
dotnet build

# Run API
cd src/CommonArchitecture.API
dotnet run

# Run Web (in another terminal)
cd src/CommonArchitecture.Web
dotnet run
```

## ✅ What's Been Implemented

1. ✅ **JWT Secret Key** - Moved to User Secrets/Environment Variables
2. ✅ **Token Cleanup Service** - Background service cleans expired tokens hourly
3. ✅ **HTTP-Only Cookies** - Secure token storage (XSS protection)
4. ✅ **Device/IP Fingerprinting** - Tracks device and IP for security
5. ✅ **Rate Limiting** - 5 requests/minute on auth endpoints
6. ✅ **Comprehensive Logging** - All auth operations logged
7. ✅ **Token Reuse Detection** - Prevents token replay attacks

## 📝 Configuration Files

- **API**: `src/CommonArchitecture.API/appsettings.json` - JWT config (no secret key)
- **Web**: `src/CommonArchitecture.Web/appsettings.json` - API settings

## 🔍 Testing

1. **Login**: POST to `/api/auth/login` with mobile and OTP
2. **Refresh Token**: POST to `/api/auth/refresh-token` with refresh token
3. **Logout**: POST to `/api/auth/logout` (requires authentication)
4. **Rate Limiting**: Try 6 requests in 1 minute (6th should return 429)

## 📚 Documentation

- **Full Details**: See `SECURITY_IMPROVEMENTS.md`
- **Original Implementation**: See `JWT_IMPLEMENTATION_SUMMARY.md`

