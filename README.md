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
