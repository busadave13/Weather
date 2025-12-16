# Weather API

A .NET 9.0 ASP.NET Core Web API that aggregates weather data from multiple sensor services.

## Overview

This project demonstrates a clean architecture pattern for a weather service that:
- Aggregates temperature, wind, and precipitation data from separate sensor services
- Maps internal sensor response models to public API models
- Uses dependency injection for testability
- Implements parallel calls for efficient data retrieval

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Weather API                          │
├─────────────────────────────────────────────────────────┤
│  Controllers                                            │
│  └── WeatherController                                  │
│      ├── GET /api/weather (combined data)               │
│      ├── GET /api/weather/temperature                   │
│      ├── GET /api/weather/wind                          │
│      └── GET /api/weather/precipitation                 │
├─────────────────────────────────────────────────────────┤
│  Business Logic Layer                                   │
│  └── WeatherBusinessLogic                               │
│      ├── Maps sensor responses to API models            │
│      └── Orchestrates parallel sensor calls             │
├─────────────────────────────────────────────────────────┤
│  Clients Layer                                          │
│  ├── TemperatureSensorClient                            │
│  ├── WindSensorClient                                   │
│  └── PrecipitationSensorClient                          │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

```
Weather/
├── src/
│   └── Weather/
│       ├── BusinessLogic/           # Business logic with mapping
│       ├── Clients/                 # HTTP clients for sensor services
│       │   └── Models/              # Sensor response models
│       ├── Controllers/             # API controllers
│       ├── Models/                  # Public API models
│       ├── Program.cs               # Application entry point with DI
│       └── appsettings.json         # Configuration
├── tests/
│   └── Weather.Tests/
│       └── BusinessLogic/           # Unit tests for business logic
└── Weather.sln
```

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /api/weather` | Returns combined weather data from all sensors |
| `GET /api/weather/temperature` | Returns current temperature data |
| `GET /api/weather/wind` | Returns current wind data |
| `GET /api/weather/precipitation` | Returns current precipitation data |

## Models

### Public API Models

- **WeatherData**: Combined weather data with temperature, wind, and precipitation
- **TemperatureData**: Temperature value, unit, and feels-like temperature
- **WindData**: Wind speed, unit, direction (cardinal), and gusts
- **PrecipitationData**: Precipitation amount, unit, type, and humidity

### Sensor Response Models

Internal models that map the raw JSON responses from sensor services:
- **SensorTemperatureResponse**
- **SensorWindResponse**  
- **SensorPrecipitationResponse**

## Configuration

### Sensor Services

Configure sensor service URLs in `appsettings.json`:

```json
{
  "SensorServices": {
    "Temperature": {
      "BaseUrl": "http://localhost:5001"
    },
    "Wind": {
      "BaseUrl": "http://localhost:5002"
    },
    "Precipitation": {
      "BaseUrl": "http://localhost:5003"
    }
  }
}
```

### Load Shedding

Configure load shedding to reject a percentage of requests when RPS exceeds a threshold:

```json
{
  "LoadShedding": {
    "Enabled": true,
    "RpsThreshold": 100,
    "FailurePercentage": 25,
    "FailureStatusCode": 503,
    "WindowDurationSeconds": 1
  }
}
```

| Setting | Description |
|---------|-------------|
| `Enabled` | Enable/disable load shedding |
| `RpsThreshold` | RPS threshold to trigger shedding |
| `FailurePercentage` | Percentage of requests to reject when over threshold |
| `FailureStatusCode` | HTTP status code for rejected requests |
| `WindowDurationSeconds` | Sliding window duration for RPS calculation |

### Mockery Integration

Enable mock responses via the `X-Mockery-Mocks` header:

```bash
curl -H "X-Mockery-Mocks: windsensor/success, temperaturesensor/success, precipitationsensor/success" \
     http://localhost:5081/api/weather
```

## Building and Running

### Prerequisites

- .NET 9.0 SDK

### Build

```bash
dotnet build Weather.sln
```

### Run Tests

```bash
dotnet test Weather.sln
```

### Run Application

```bash
dotnet run --project src/Weather/Weather.csproj
```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

## Health Checks

Kubernetes-compatible health check endpoints:

| Endpoint | Purpose | Description |
|----------|---------|-------------|
| `/health/live` | Liveness probe | Indicates if the app is running |
| `/health/ready` | Readiness probe | Indicates if the app is ready for traffic |
| `/health/startup` | Startup probe | Indicates if the app has started |

### Configuration

```json
{
  "HealthCheck": {
    "RequestCountThreshold": 1000,
    "LiveGracePeriodRequests": 100
  }
}
```

| Setting | Description |
|---------|-------------|
| `RequestCountThreshold` | Ready becomes Unhealthy after this many requests (0 = disabled) |
| `LiveGracePeriodRequests` | Additional requests before Live becomes Unhealthy |

The health checks count each probe call. When `RequestCountThreshold` is exceeded:
- `/health/ready` returns Unhealthy (stops new traffic)
- `/health/live` returns Unhealthy after grace period (triggers pod restart)

## Load Testing

Load tests using [k6](https://k6.io/) are available in the `k6/` directory:

```powershell
# Run standard load test
k6 run k6/load-test.js

# Run stress test (find breaking points)
k6 run k6/stress-test.js

# Run spike test (sudden traffic spikes)
k6 run k6/spike-test.js

# Set custom target URL
$env:K6_BASE_URL = "http://localhost:5081"
k6 run k6/load-test.js
```

Sample output:
```
     ✓ status is 200
     ✓ response time < 500ms

     checks.........................: 100.00% ✓ 1234  ✗ 0
     http_req_duration..............: avg=45ms    min=12ms    max=234ms
     http_req_failed................: 0.00%   ✓ 0     ✗ 1234
     http_reqs......................: 1234    12.34/s
     vus............................: 20      min=1   max=20
```

See [k6/README.md](k6/README.md) for full documentation.

## CI/CD Pipeline

Automated publishing via GitHub Actions when PRs are merged to `main`.

### Docker Image

```bash
# Pull latest
docker pull ghcr.io/busadave13/weather:latest

# Pull specific version
docker pull ghcr.io/busadave13/weather:1.0.0

# Run locally
docker run -p 8080:8080 ghcr.io/busadave13/weather:latest
```

### Helm Chart

```bash
# Install from GitHub Container Registry
helm install weather oci://ghcr.io/busadave13/helm/weather --version 1.0.0

# Install with custom values
helm install weather oci://ghcr.io/busadave13/helm/weather \
  --version 1.0.0 \
  --namespace dev \
  --create-namespace
```

See [.github/PUBLISH.md](.github/PUBLISH.md) for full CI/CD documentation.

## Cline Workflows

This project includes Cline workflow automation:

### Create a New Branch

Use `/checkout.md` to create a new feature branch:
- Checks for uncommitted changes
- Fetches latest from remote
- Creates branch with proper naming convention

### Create a Pull Request

Use `/pullrequest.md` to create a PR:
- Runs unit tests before PR creation
- Auto-generates title and description
- Creates PR via GitHub MCP

## Technology Stack

- **Language**: C# 9.0+
- **Framework**: ASP.NET Core 9.0
- **Testing**: xUnit, Moq, FluentAssertions
- **Documentation**: Swagger/OpenAPI

## License

MIT License - see [LICENSE](LICENSE) file for details.
