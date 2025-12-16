# Active Context: Weather

## Current Session Focus
Created GET /api/alerts endpoint with controller-based load shedding (replaced middleware).

## Recent Changes

### Session: 2025-12-16 (Latest - Alerts API)
1. **Created GET /api/alerts Endpoint**
   - New `AlertsController` with load shedding built into controller
   - Returns 204 No Content on success
   - Returns 503 Service Unavailable when load shedding triggers
   - Sets `X-Load-Shedding: true` and `Retry-After: 1` headers

2. **Refactored Load Shedding to Service Pattern**
   - Created `ILoadSheddingService` interface
   - Created `LoadSheddingService` implementation (singleton)
   - Removed `LoadSheddingMiddleware` (deleted)
   - Removed `LoadSheddingExtensions` (deleted)
   - Load shedding now only applies to /api/alerts endpoint

3. **Tests Updated**
   - Deleted `LoadSheddingMiddlewareTests.cs`
   - Added `LoadSheddingServiceTests.cs` (10 tests)
   - Added `AlertsControllerTests.cs` (11 tests)
   - 90 unit tests passing (up from 83)

### Session: 2025-12-16 (Earlier)
1. **Added Test Configuration API**
   - Created `POST /api/config` endpoint for runtime test configuration
   - Can force health checks to fail individually (startup, ready, live)
   - Can add delays to health check responses

2. **Added k6 Load Testing**
   - Created `k6/` folder with load testing scripts
   - Removed fortio (replaced with k6)

3. **Added LoadShedding to Helm Chart**
   - Added `config.loadShedding.*` values to `values.yaml`

### Session: 2025-12-14
- Fixed MockeryHandler Header Issue
- Fixed Helm Chart Mockery URL

## Active Decisions
- **Load shedding in controller** - Not middleware, applies only to /api/alerts
- **Health checks increment counter directly** - No middleware needed
- **k6 for load testing** - Replaced fortio with k6

## Current State
- 90 unit tests passing
- GET /api/alerts with controller-based load shedding
- Load shedding configurable via appsettings.json or Helm values

## API Endpoints
- `GET /api/weather` - Combined weather data
- `GET /api/weather/temperature` - Temperature data
- `GET /api/weather/wind` - Wind data
- `GET /api/weather/precipitation` - Precipitation data
- `GET /api/alerts` - Alerts with load shedding protection
- `GET /api/config` - Get test configuration
- `POST /api/config` - Set test configuration
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe
- `GET /health/startup` - Startup probe

## Important Patterns
- **Load Shedding Service**: Singleton with sliding window RPS tracking
- **Controller-based Protection**: Load shedding logic in controller, not middleware
- **Test Configuration State**: Singleton for thread-safe runtime configuration
- **MockeryHandler**: DelegatingHandler pattern for HTTP interception
