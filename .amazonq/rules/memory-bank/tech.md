# CommonArchitecture - Technology Stack

## Core Framework
- **.NET 9.0**: Latest LTS version with modern C# features
- **ASP.NET Core**: Web framework for both API and Web applications
- **C# 12**: Latest language features with nullable reference types enabled
- **Implicit Usings**: Simplified namespace management

## Data & Persistence
- **Entity Framework Core 9.0**: Modern ORM with SQL Server provider
- **SQL Server**: Primary database engine
- **EF Core Migrations**: Database schema versioning and deployment
- **System.Linq.Dynamic.Core 1.7.1**: Dynamic LINQ queries for flexible filtering

## Authentication & Security
- **JWT Bearer Authentication**: Token-based API security
- **System.IdentityModel.Tokens.Jwt 8.2.1**: JWT token handling
- **HTTP-Only Cookies**: Secure web session management
- **User Secrets**: Development credential management
- **Rate Limiting**: Built-in request throttling

## Background Processing
- **Hangfire 1.8.17**: Background job processing and scheduling
- **Hangfire.SqlServer**: SQL Server storage for Hangfire jobs
- **Hangfire.AspNetCore**: ASP.NET Core integration

## Web Development
- **Razor Pages**: Server-side rendered web UI
- **MVC Controllers**: Traditional controller-based endpoints
- **FluentValidation.AspNetCore 11.3.1**: Robust input validation
- **NToastNotify 8.0.0**: User notification system

## API Development
- **OpenAPI/Swagger**: API documentation and testing
- **Scalar.AspNetCore 2.11.1**: Modern API documentation UI
- **RESTful Controllers**: Standard HTTP API endpoints
- **Problem Details**: Standardized error responses

## Development Tools
- **User Secrets**: Secure development configuration
- **EF Core Design Tools**: Migration and scaffolding support
- **Hot Reload**: Development productivity features

## Build & Deployment
- **SDK-style Projects**: Modern .csproj format
- **PowerShell Scripts**: Automation for common tasks
- **GitHub Actions**: CI/CD pipeline support (configured)

## Development Commands

### Database Operations
```bash
# Create migration
dotnet ef migrations add <MigrationName> --startup-project ../CommonArchitecture.API

# Update database
dotnet ef database update --startup-project ../CommonArchitecture.API

# Drop database
dotnet ef database drop --startup-project ../CommonArchitecture.API
```

### Build & Run
```bash
# Build solution
dotnet build

# Run API (default: https://localhost:7001)
cd src/CommonArchitecture.API
dotnet run

# Run Web (default: https://localhost:7002)
cd src/CommonArchitecture.Web
dotnet run

# Run both applications
./run-both.ps1
```

### Testing
```bash
# Run unit tests
dotnet test src/tests/CommonArchitecture.UnitTests

# Run integration tests
dotnet test src/tests/CommonArchitecture.IntegrationTests

# Run all tests
dotnet test
```

### User Secrets Management
```bash
# Initialize user secrets
dotnet user-secrets init

# Set JWT secret key
dotnet user-secrets set "Jwt:SecretKey" "YourSecretKey"

# List all secrets
dotnet user-secrets list
```

### Database Seeding
```bash
# Run seed data utility
cd SeedRunner
dotnet run

# Or use PowerShell script
./run-seed.cs
```

## Configuration Management
- **appsettings.json**: Base application settings
- **appsettings.Development.json**: Development overrides
- **appsettings.Testing.json**: Test environment configuration
- **User Secrets**: Sensitive development data (JWT keys, connection strings)
- **Environment Variables**: Production configuration

## Package Management
- **NuGet**: Package dependency management
- **Central Package Management**: Consistent versioning across projects
- **Transitive Dependencies**: Automatic dependency resolution

## IDE Support
- **Visual Studio 2022**: Full IDE support with IntelliSense
- **Visual Studio Code**: Lightweight development with C# extension
- **JetBrains Rider**: Cross-platform .NET IDE
- **Hot Reload**: Real-time code changes during development