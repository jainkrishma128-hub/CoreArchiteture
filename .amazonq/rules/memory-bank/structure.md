# CommonArchitecture - Project Structure

## Solution Organization

### Root Structure
```
CommonArchitecture/
├── src/                          # Main source code
├── SeedRunner/                   # Database seeding utility
├── .github/workflows/            # CI/CD pipelines
├── *.sql                        # Database scripts
├── *.md                         # Documentation files
└── *.ps1                        # PowerShell automation scripts
```

## Core Projects (src/)

### 1. CommonArchitecture.Core (Domain Layer)
**Purpose**: Contains business entities, domain logic, and contracts
```
Core/
├── Entities/                     # Domain entities (User, Product, Category, etc.)
├── Interfaces/                   # Repository and service contracts
├── DTOs/                        # Data transfer objects
└── Enums/                       # Domain enumerations
```
**Dependencies**: None (pure domain layer)

### 2. CommonArchitecture.Application (Application Layer)
**Purpose**: Orchestrates use cases and business workflows
```
Application/
├── Services/                     # Application services
├── DTOs/                        # Application-specific DTOs
└── Behaviors/                   # Cross-cutting concerns (validation, logging)
```
**Dependencies**: Core

### 3. CommonArchitecture.Infrastructure (Infrastructure Layer)
**Purpose**: Implements data access and external service integrations
```
Infrastructure/
├── Persistence/                  # EF Core DbContext and configurations
├── Repositories/                 # Repository implementations
├── Services/                    # Infrastructure services (logging, etc.)
├── Migrations/                  # EF Core database migrations
└── UnitOfWork/                  # Transaction management
```
**Dependencies**: Core, Application

### 4. CommonArchitecture.API (API Presentation Layer)
**Purpose**: REST API endpoints and HTTP concerns
```
API/
├── Controllers/                  # REST API controllers
├── Middlewares/                 # HTTP middleware components
├── Services/                    # API-specific services
├── Helpers/                     # Utility classes
├── Properties/                  # Launch settings
└── wwwroot/                     # Static API assets
```
**Dependencies**: Core, Application, Infrastructure

### 5. CommonArchitecture.Web (Web Presentation Layer)
**Purpose**: Razor Pages web application
```
Web/
├── Areas/                       # Feature areas (Admin, etc.)
├── Controllers/                 # MVC controllers
├── Views/                       # Razor views and layouts
├── ViewComponents/              # Reusable view components
├── Models/                      # View models
├── Services/                    # Web-specific services
├── Filters/                     # Action filters
├── Handlers/                    # Custom handlers
├── Middlewares/                 # Web middleware
├── Validators/                  # Input validation
└── wwwroot/                     # Static web assets (CSS, JS, images)
```
**Dependencies**: Core, Application, Infrastructure

### 6. Tests
**Purpose**: Automated testing suites
```
tests/
├── CommonArchitecture.UnitTests/        # Unit tests
└── CommonArchitecture.IntegrationTests/ # Integration tests
```

## Architectural Patterns

### Clean Architecture Layers
1. **Domain (Core)**: Business entities and rules
2. **Application**: Use cases and orchestration
3. **Infrastructure**: Data access and external services
4. **Presentation**: API and Web interfaces

### Key Design Patterns
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction boundary management
- **Dependency Injection**: Loose coupling and testability
- **CQRS**: Command Query Responsibility Segregation
- **Mediator Pattern**: Decoupled request handling

### Data Flow
```
Client Request → Presentation Layer → Application Layer → Domain Layer
                      ↓                    ↓              ↓
                 HTTP Response ← Infrastructure Layer ← Repository
```

## Component Relationships

### Dependency Direction
- **Presentation** → Application → Core
- **Infrastructure** → Application → Core
- **Tests** → All layers (for testing)

### Key Interfaces
- `IRepository<T>`: Generic repository contract
- `IUnitOfWork`: Transaction management
- `ILoggingService`: Centralized logging
- `IApplicationDbContext`: Database context contract

## Configuration Structure
- **appsettings.json**: Base configuration
- **appsettings.Development.json**: Development overrides
- **appsettings.Testing.json**: Test environment settings
- **User Secrets**: Sensitive development data (JWT keys)

## Documentation Organization
Located in `src/Documents/`:
- Architecture overview
- Database migration guides
- Module addition guides
- Authentication guides
- UI library documentation