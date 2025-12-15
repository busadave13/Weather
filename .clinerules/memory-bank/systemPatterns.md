# System Patterns: Weather

## Architecture Overview
```
┌─────────────────────────────────────────────────────────┐
│                    Weather Service                       │
├─────────────────────────────────────────────────────────┤
│  API Layer (ASP.NET Core)                               │
│  ├── Controllers                                        │
│  ├── Middleware                                         │
│  └── DTOs                                               │
├─────────────────────────────────────────────────────────┤
│  Service Layer                                          │
│  ├── Business Logic                                     │
│  └── Interfaces                                         │
├─────────────────────────────────────────────────────────┤
│  Client Layer                                           │
│  ├── Sensor Clients (Temperature, Wind, Precipitation) │
│  ├── MockeryHandler (DelegatingHandler)                │
│  └── MockeryHandlerFactory                             │
├─────────────────────────────────────────────────────────┤
│  Data Layer                                             │
│  ├── Repositories                                       │
│  └── Models                                             │
└─────────────────────────────────────────────────────────┘
```

## Key Technical Decisions

### Code Style
- **Indentation:** 4 spaces (no tabs)
- **Naming:** PascalCase for methods/classes, camelCase for variables
- **Nullable Reference Types:** Enabled
- **Warnings:** Treated as errors

### Patterns Used
1. **Repository Pattern** - Data access abstraction
2. **Dependency Injection** - ASP.NET Core built-in DI
3. **DelegatingHandler Pattern** - MockeryHandler for HTTP interception
4. **Factory Pattern** - MockeryHandlerFactory for service-specific handlers
5. **Records** - Immutable data structures
6. **Span<T>/Memory<T>** - High-performance scenarios

### MockeryHandler Pattern
The MockeryHandler is a DelegatingHandler that intercepts outgoing HTTP requests and routes them to a mock service based on the `X-Mockery-Mocks` header.

```
┌────────────────────────────────────────────────────────────────┐
│ Incoming Request                                               │
│ Headers: X-Mockery-Mocks: wind/success, temp/error, precip/ok │
└────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────┐
│ MockeryHandler (ServiceName: "WindSensor")                     │
│ 1. Parse X-Mockery-Mocks header                               │
│ 2. Find mock matching "wind" → "wind/success"                 │
│ 3. Call Mockery service with X-Mockery-Mock: wind/success     │
│ 4. Return mocked response                                      │
└────────────────────────────────────────────────────────────────┘
```

**Key Components:**
- `MockeryHandler` - Per-service delegating handler
- `MockeryHandlerFactory` - Creates handlers with specific ServiceName
- `MockeryHandlerOptions` - Configuration (Enabled, BaseUrl)

**Header Format:**
- `X-Mockery-Mocks`: Comma-delimited list of mock identifiers
- Each identifier: `<service-prefix>/<path>` (e.g., "wind/prod/success")
- Matching: First segment compared against ServiceName (case-insensitive)
- Multiple matches: Random selection

### Testing Strategy
- **Unit Tests:** xUnit framework
- **Mocking:** Moq library
- **Assertions:** FluentAssertions for readable tests
- **Coverage:** Target meaningful coverage, not percentage

### Security
- No hardcoded secrets in source code
- Environment variables for configuration
- Secrets management via secure stores

## Component Relationships
```
Controllers → BusinessLogic → SensorClients → MockeryHandler → Mockery/RealService
     ↓             ↓               ↓
   DTOs         Models        ClientModels
```

## HttpClient Registration Pattern
```csharp
// Each sensor client gets its own MockeryHandler instance
builder.Services.AddHttpClient<IWindSensorClient, WindSensorClient>(...)
    .AddHttpMessageHandler(sp => 
        sp.GetRequiredService<IMockeryHandlerFactory>().Create("WindSensor"));
```

## Development Workflow
1. `/checkout.md` - Create feature branch from main
2. Develop and commit locally
3. `/pullrequest.md` - Run tests, create PR

## Branch Strategy
- `main` - Production-ready code
- `users/davhar/feature/<name>` - New features
- `users/davhar/fix/<name>` - Bug fixes
- `users/davhar/bugfix/<name>` - Bug fixes (alternative)
- `users/davhar/chore/<name>` - Maintenance tasks
