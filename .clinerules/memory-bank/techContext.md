# Technical Context: Weather

## Technology Stack

### Core Runtime
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 9.0+ | Runtime platform |
| C# | 9.0+ | Primary language |
| ASP.NET Core | 9.0+ | Web framework |

### Development Tools
| Tool | Purpose |
|------|---------|
| dotnet CLI | Build, run, test |
| Docker | Containerization |
| VS Code | IDE with Cline extension |
| Git | Version control |

### Testing Stack
| Library | Purpose |
|---------|---------|
| xUnit | Test framework |
| Moq | Mocking library |
| FluentAssertions | Assertion library |

### Infrastructure
| Tool | Purpose |
|------|---------|
| GitHub Actions | CI/CD pipelines |
| Docker | Container runtime |
| Terraform | Infrastructure as code |

## Development Environment Setup

### Prerequisites
```bash
# .NET SDK
dotnet --version  # Should be 9.0+

# Docker
docker --version

# Git
git --version
```

### Common Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run

# Format code
dotnet format

# Docker build
docker build -t weather-service .
```

## Dependencies
- Dependencies managed via NuGet
- Package references in `.csproj` files
- No direct dependency on external services (mockery-based)

## Configuration
- **Development:** `appsettings.Development.json`
- **Production:** Environment variables
- **Secrets:** Never in source code

## Constraints
1. All code must pass `dotnet format` checks
2. Nullable reference types must be enabled
3. Warnings treated as errors
4. XML documentation required for public APIs

## Integration Points
- GitHub repository for source control
- GitHub Actions for CI/CD
- Docker Hub or container registry for images
