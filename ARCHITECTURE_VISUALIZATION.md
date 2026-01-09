# Architecture Visualization & Structure

## Current Modular Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Web Application                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           Program.cs (Main Entry Point)             │   │
│  │                                                     │   │
│  │  var modules = new IModule[] {                      │   │
│  │    new PersistenceModule(),                         │   │
│  │    new ApplicationServicesModule(),                 │   │
│  │    new CachingModule()                              │   │
│  │  };                                                 │   │
│  │  builder.Services.AddModules(config, modules);      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
        ┌─────────────────────────────────────┐
        │   Modular Dependency Injection      │
        │                                     │
        │  ┌──────────────────────────────┐  │
        │  │   ModuleExtensions           │  │
        │  │  .AddModules(modules)        │  │
        │  └──────────────────────────────┘  │
        └─────────────────────────────────────┘
                          │
         ┌────────────────┼────────────────┐
         ▼                ▼                ▼
    ┌─────────┐     ┌──────────┐     ┌─────────┐
    │Persistence│  │Application│  │Caching  │
    │ Module   │  │ Services  │  │ Module  │
    │         │  │ Module    │  │        │
    │ • DbCtx │  │         │  │ • Memory│
    │ • Repos │  │ • Logger │  │ • Cache │
    │ • UoW  │  │ • Notify │  │ • Helper│
    └─────────┘  └──────────┘  └─────────┘
         │            │            │
         └────────────┼────────────┘
                      ▼
          ┌───────────────────────┐
          │  Service Collection   │
          │  (IServiceProvider)   │
          └───────────────────────┘
                      │
          ┌──────────┼──────────┐
          ▼          ▼          ▼
     Controllers  Services  Middlewares
```

---

## Layer Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                      │
│  ┌──────────────────┐        ┌──────────────────┐          │
│  │  API Controllers │        │  Web Controllers │          │
│  │  (REST Endpoints)│        │  (MVC Views)     │          │
│  └──────────────────┘        └──────────────────┘          │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ depends on
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Business Logic Services                            │   │
│  │  • ProductService      • UserService                │   │
│  │  • RoleService         • DashboardService           │   │
│  │  • MenuService         • RoleMenuService            │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ depends on
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Core/Domain Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────┐   │
│  │   Entities   │  │  Interfaces  │  │  Enums         │   │
│  │              │  │              │  │                │   │
│  │ • User       │  │ • IRepository│  │ • Status       │   │
│  │ • Product   │  │ • IService   │  │ • UserRole     │   │
│  │ • Category  │  │ • IUnitOfWork│  │                │   │
│  │ • Role      │  │              │  │                │   │
│  └──────────────┘  └──────────────┘  └────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ depends on
                              ▼
┌─────────────────────────────────────────────────────────────┐
│               Infrastructure Layer                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Persistence & Data Access                          │  │
│  │  • DbContext  • Repositories  • Unit of Work        │  │
│  │  • Migrations • Database Seeders                    │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Cross-Cutting Concerns                             │  │
│  │  • Logging  • Notifications  • Caching              │  │
│  └──────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Modular Registration                               │  │
│  │  • Modules  • Extensions  • Configuration           │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Module Registration Flow

```
Program.cs (Startup)
    │
    ├─ Configuration setup
    │  └─ appsettings.json
    │  └─ User Secrets (dev)
    │  └─ Environment Variables (prod)
    │
    ├─ Create Modules
    │  ├─ PersistenceModule
    │  ├─ ApplicationServicesModule
    │  └─ CachingModule
    │
    ├─ Call AddModules()
    │  └─ Calls .AddModules(config, modules)
    │     └─ In ModuleExtensions
    │        └─ Iterates modules
    │           └─ Calls RegisterServices() on each
    │
    ├─ Each Module Registers:
    │  ├─ Validates Configuration
    │  ├─ Creates Service Descriptors
    │  └─ Adds to ServiceCollection
    │
    └─ Build ServiceProvider
       └─ Dependency injection ready
          └─ Application starts!
```

---

## Dependency Injection Container (Simplified)

```
IServiceProvider
│
├── Database Layer
│   ├── IDbContextFactory<ApplicationDbContext>  [Singleton]
│   ├── ApplicationDbContext                     [Scoped]
│   ├── IUnitOfWork                             [Scoped]
│   │
│   └── Repositories                            [Scoped]
│       ├── IUserRepository → UserRepository
│       ├── IProductRepository → ProductRepository
│       ├── ICategoryRepository → CategoryRepository
│       └── ...
│
├── Application Services                        [Scoped]
│   ├── ILoggingService → LoggingService
│   ├── INotificationService → NotificationService
│   ├── IProductService → ProductService
│   └── ...
│
├── Caching                                     [Singleton]
│   ├── IMemoryCache
│   └── CacheHelper
│
└── Supporting Services                        [Scoped]
    ├── IJwtService → JwtService
    ├── IAuthRepository → AuthRepository
    └── ...
```

---

## File Structure After Refactoring

```
src/
├── CommonArchitecture.Core/
│   ├── Modules/
│   │   └── IModule.cs                 ← Module interface
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   ├── IService.cs
│   │   └── ...
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   └── ...
│   └── DTOs/
│
├── CommonArchitecture.Infrastructure/
│   ├── Extensions/
│   │   └── ModuleExtensions.cs        ← DI extension methods
│   ├── Modules/
│   │   ├── PersistenceModule.cs       ← Database & repos
│   │   ├── ApplicationServicesModule.cs ← App services
│   │   └── CachingModule.cs           ← Caching
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── ProductRepository.cs
│   │   └── ...
│   ├── Services/
│   │   ├── LoggingService.cs
│   │   └── NotificationService.cs
│   └── UnitOfWork/
│
├── CommonArchitecture.API/
│   ├── Program.cs                     ← Clean DI setup
│   ├── Controllers/
│   └── Middlewares/
│
└── CommonArchitecture.Web/
    ├── Program.cs                     ← Clean DI setup
    ├── Controllers/
    └── Views/
```

---

## Data Flow Example: User Authentication

```
Request: POST /api/auth/login
                │
                ▼
        ┌──────────────────┐
        │ AuthController   │
        └──────────────────┘
                │ Injects IAuthRepository
                ▼
        ┌──────────────────┐
        │ AuthRepository   │ ← Registered by PersistenceModule
        └──────────────────┘
                │ Uses ApplicationDbContext (Scoped)
                ▼
        ┌──────────────────┐
        │ ApplicationDbContext
        └──────────────────┘
                │ Created from IDbContextFactory (Singleton)
                ▼
        ┌──────────────────┐
        │ SQL Server       │
        └──────────────────┘
                │
                ▼
        ┌──────────────────┐
        │ AuthRepository   │ Returns User
        └──────────────────┘
                │
                ▼
        ┌──────────────────┐
        │ AuthController   │
        │ - Generate JWT   │ ← Injects IJwtService
        │ - Log event      │ ← Injects ILoggingService
        └──────────────────┘
                │
                ▼
        ┌──────────────────┐
        │ HTTP 200 + Token │
        └──────────────────┘
```

---

## Adding a New Feature (Email Service)

```
Step 1: Create Interface (Core Layer)
└─ src/CommonArchitecture.Core/Interfaces/IEmailService.cs

Step 2: Create Implementation (Infrastructure Layer)
└─ src/CommonArchitecture.Infrastructure/Services/EmailService.cs

Step 3: Create Module (Infrastructure Layer)
└─ src/CommonArchitecture.Infrastructure/Modules/EmailModule.cs
   ├─ Validate Email config
   └─ Register IEmailService → EmailService

Step 4: Update Program.cs
└─ Add new EmailModule to modules array
   var modules = new IModule[] {
       new PersistenceModule(),
       new EmailModule(),  // ← NEW
       ...
   };

Step 5: Use in Controllers
└─ Inject IEmailService in controller
   public EmailController(IEmailService emailService) { }

Step 6: Done! 🎉
└─ No complex DI changes needed
└─ Modular, testable, scalable
```

---

## Security Model

```
Secrets Management
    │
    ├─ Development
    │  └─ User Secrets
    │     └─ dotnet user-secrets set "Key" "value"
    │     └─ Stored in secure local file
    │
    ├─ Staging
    │  └─ Environment Variables
    │     └─ Set on server/container
    │     └─ Never in code
    │
    └─ Production
       └─ Environment Variables + Key Vault
          └─ Azure Key Vault / AWS Secrets Manager
          └─ Highest security level

Configuration Validation (Module Registration)
    │
    ├─ Each module validates required settings
    ├─ Throws InvalidOperationException if missing
    ├─ Fails fast at startup (not runtime)
    └─ Clear error messages

Dependency Injection Lifetimes
    │
    ├─ Singleton (database factory)
    ├─ Scoped (DbContext, services per request)
    └─ Transient (temporary objects)
       └─ Prevents lifetime scope violations
```

---

## Performance Optimization

```
Database Connection
    │
    ├─ Pooled Factory (Singleton)
    │  └─ Reuses connections efficiently
    │  └─ Better performance at scale
    │
    └─ Scoped DbContext
       └─ One context per request
       └─ Proper disposal via IDisposable

Memory Caching
    │
    ├─ CacheHelper (Singleton)
    │  └─ Thread-safe cache instance
    │  └─ Shared across all requests
    │
    └─ CacheInvalidator (Scoped)
       └─ Request-specific cache updates
       └─ Proper lifecycle management

Lazy Initialization
    │
    ├─ Services created on first use
    └─ Only needed services instantiated
       └─ Reduced startup time
       └─ Lower memory footprint
```

---

This modular architecture provides:

✅ **Scalability** - Add modules without touching Program.cs  
✅ **Maintainability** - Clear separation of concerns  
✅ **Security** - Configuration validation & secrets management  
✅ **Testability** - Mock modules for testing  
✅ **Performance** - Efficient resource usage  
✅ **Extensibility** - Easy to add new features  

Your architecture is production-ready! 🚀
