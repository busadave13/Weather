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
3. **Records** - Immutable data structures
4. **Span<T>/Memory<T>** - High-performance scenarios

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
Controllers → Services → Repositories → Data Store
     ↓            ↓
   DTOs       Models
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
