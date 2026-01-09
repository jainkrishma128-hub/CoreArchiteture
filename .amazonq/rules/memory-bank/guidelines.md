# CommonArchitecture - Development Guidelines

## Code Quality Standards

### Naming Conventions
- **Classes**: PascalCase (e.g., `ApplicationDbContext`, `TokenStorageService`)
- **Methods**: PascalCase (e.g., `GetAccessTokenAsync`, `SaveTokensAsync`)
- **Properties**: PascalCase (e.g., `AccessToken`, `RefreshToken`)
- **Fields**: camelCase with underscore prefix (e.g., `_httpContextAccessor`, `_memoryCache`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `ACCESS_TOKEN_COOKIE`, `FIXED_OTP`)
- **Parameters**: camelCase (e.g., `accessToken`, `refreshToken`)
- **Local Variables**: camelCase (e.g., `httpContext`, `deviceFingerprint`)

### File Organization
- **One class per file** with matching filename
- **Namespace matches folder structure** (e.g., `CommonArchitecture.Web.Services`)
- **Using statements** at top, organized by system then third-party then project references
- **Consistent indentation** using 4 spaces

### Documentation Standards
- **XML comments** for public APIs and complex methods
- **Inline comments** for business logic explanations
- **Logging statements** for important operations and error conditions
- **Method names** should be self-documenting

## Architectural Patterns

### Dependency Injection
- **Constructor injection** is the primary pattern
- **Interface-based dependencies** for all services
- **Scoped lifetime** for most application services
- **Transient lifetime** for lightweight handlers
- **Singleton lifetime** for configuration and caching services

```csharp
// Standard DI registration pattern
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>();
```

### Repository Pattern
- **Generic repository interfaces** with common CRUD operations
- **Specific repository implementations** for complex queries
- **Async methods** for all database operations
- **Unit of Work** pattern for transaction management

### Service Layer Pattern
- **Application services** orchestrate business operations
- **Infrastructure services** handle external concerns
- **Web services** manage UI-specific logic
- **Clear separation** between layers

## Entity Framework Patterns

### DbContext Configuration
- **Fluent API configuration** in `OnModelCreating`
- **Explicit property configuration** with constraints
- **Index definitions** for performance optimization
- **Default values** using SQL functions (e.g., `GETUTCDATE()`)

```csharp
// Standard entity configuration pattern
entity.HasKey(p => p.Id);
entity.Property(p => p.Name).IsRequired().HasMaxLength(256);
entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
entity.HasIndex(p => p.Name).IsUnique();
```

### Relationship Configuration
- **Explicit foreign key definitions**
- **Cascade delete** for dependent entities
- **Restrict delete** for reference entities
- **Composite indexes** for query optimization

## Authentication & Security Patterns

### JWT Token Management
- **Refresh token rotation** for enhanced security
- **Device fingerprinting** for session tracking
- **Token reuse detection** to prevent replay attacks
- **Comprehensive logging** for security events

### Rate Limiting
- **EnableRateLimiting attribute** on sensitive endpoints
- **AuthPolicy** configuration for authentication endpoints
- **Consistent error responses** for rate limit violations

### Cookie Security
- **HttpOnly cookies** for token storage
- **Secure flag** based on HTTPS context
- **SameSite Strict** for CSRF protection
- **Proper expiration** handling

## HTTP Client Patterns

### Service Registration
- **Named HttpClient** registration with base address
- **Message handlers** for cross-cutting concerns (auth, logging)
- **Default headers** for content type and accept headers
- **Timeout configuration** for reliability

```csharp
// Standard HttpClient registration
builder.Services.AddHttpClient<IProductApiService, ProductApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<RefreshTokenHandler>();
```

### Error Handling
- **Structured error responses** using ProblemDetails
- **HTTP status code mapping** for different error types
- **Comprehensive logging** with context information
- **User-friendly error messages** in UI

## Frontend JavaScript Patterns

### Module Organization
- **Self-contained modules** with clear responsibilities
- **State management** using local variables
- **Event delegation** for dynamic content
- **Debounced search** for performance

### AJAX Patterns
- **Consistent error handling** with status code checks
- **Loading indicators** for user feedback
- **CSRF token** inclusion in requests
- **Proper content type** headers

```javascript
// Standard AJAX pattern
$.ajax({
    url: url,
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify(formData),
    headers: { 
        'RequestVerificationToken': token,
        'X-CSRF-TOKEN': token
    },
    success: function(response) { /* handle success */ },
    error: function(xhr) { /* handle error */ }
});
```

### UI Patterns
- **Bootstrap classes** for consistent styling
- **Modal dialogs** for CRUD operations
- **Toast notifications** for user feedback
- **Pagination** with proper navigation
- **Sorting indicators** for table columns

## Logging Patterns

### Structured Logging
- **LogInformation** for normal operations
- **LogWarning** for recoverable issues
- **LogError** for exceptions and failures
- **Contextual information** in log messages

```csharp
// Standard logging pattern
_logger.LogInformation("User {UserId} logged in from IP: {IpAddress}", 
    user.Id, ipAddress);
_logger.LogWarning("Invalid OTP attempt for mobile: {Mobile}", request.Mobile);
```

### Security Logging
- **Authentication events** with user and IP context
- **Authorization failures** with attempted actions
- **Token operations** with device fingerprints
- **Sensitive data exclusion** from logs

## Configuration Patterns

### Settings Management
- **appsettings.json** for base configuration
- **Environment-specific** overrides
- **User Secrets** for development credentials
- **Environment variables** for production secrets

### Service Configuration
- **Options pattern** for complex configuration
- **Configuration binding** to strongly-typed classes
- **Validation** of configuration values
- **Default values** for optional settings

## Testing Patterns

### Unit Testing
- **Arrange-Act-Assert** pattern
- **Mock dependencies** using interfaces
- **Test method naming** describes scenario
- **Single responsibility** per test

### Integration Testing
- **In-memory database** for data layer tests
- **Test fixtures** for shared setup
- **Clean state** between tests
- **Real HTTP clients** for API tests

## Performance Patterns

### Database Optimization
- **AsNoTracking** for read-only queries
- **Proper indexing** for frequently queried columns
- **Pagination** for large result sets
- **Bulk operations** for data modifications

### Caching Strategies
- **Memory cache** for frequently accessed data
- **Distributed cache** for multi-instance scenarios
- **Cache invalidation** on data changes
- **Appropriate expiration** policies

### Async Programming
- **Async/await** for I/O operations
- **ConfigureAwait(false)** in library code
- **Task.FromResult** for synchronous implementations
- **Proper exception handling** in async methods