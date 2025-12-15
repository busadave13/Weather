# Active Context: Weather

## Current Session Focus
Health check counter refactoring and CI/CD documentation updates.

## Recent Changes

### Session: 2025-12-14 (Latest)
1. **Refactored Request Counter to Health Checks**
   - Removed `RequestCounterMiddleware` - no longer needed
   - `ReadyHealthCheck` now increments counter on each call via `IncrementAndGet()`
   - `LiveHealthCheck` now increments counter on each call via `IncrementAndGet()`
   - Counter increments each time health check endpoints are called
   - Updated unit tests to verify counter increment behavior

2. **Updated CI/CD Documentation**
   - Updated `.github/PUBLISH.md` to reflect actual Weather project configuration
   - Changed from Docker Hub to GitHub Container Registry (ghcr.io)
   - Updated username from `davhar` to `busadave13`
   - Added GitVersion documentation for semantic versioning
   - Added troubleshooting and best practices sections

3. **Added Helm Charts and GitHub Workflow**
   - `charts/weather/` - Kubernetes Helm chart
   - `.github/workflows/publish-docker-helm.yml` - CI/CD pipeline
   - Publishes Docker images and Helm charts to ghcr.io

### Previous Session: 2025-12-14
1. **Implemented Load Shedding Middleware**
   - Created `LoadSheddingOptions` configuration class
   - Created `LoadSheddingMiddleware` with sliding window RPS tracking
   - Created `LoadSheddingExtensions` for DI registration
   - Added configuration section in `appsettings.json`
   - Added 17 comprehensive unit tests

2. **Implemented Health Checks**
   - `LiveHealthCheck` - Liveness probe with grace period
   - `ReadyHealthCheck` - Readiness probe based on request count threshold
   - `StartupHealthCheck` - Startup probe
   - `RequestCounter` - Thread-safe counter using Interlocked

### Previous Session: 2024-12-14
- Created MockeryHandler with service-specific mocking
- Created MockeryHandlerFactory for creating handlers per HttpClient
- Created PR and checkout workflows
- Initialized memory-bank

## Active Decisions
- **Health checks increment counter directly** - No middleware needed
- **Load Shedding disabled by default** - Must set `Enabled: true` to activate
- **CI/CD uses GitHub Container Registry** - ghcr.io/busadave13/weather

## Current State
- 74 unit tests passing
- Health checks increment counter on each call
- Load shedding middleware fully implemented
- MockeryHandler fully implemented
- CI/CD pipeline ready for PR merge

## Architecture

### Health Check Counter Flow
```
/health/ready or /health/live called
    ↓
Health check class increments counter
    ↓
Counter value compared against threshold
    ↓
Return Healthy or Unhealthy based on threshold
```

### Load Shedding Flow
```
Incoming Request → LoadSheddingMiddleware
    ↓
Track request in sliding window
    ↓
Calculate current RPS
    ↓
RPS <= Threshold? → Continue to Controller
    ↓
RPS > Threshold → Apply failure percentage logic
```

## Configuration Example
```json
{
  "HealthCheck": {
    "RequestCountThreshold": 1000,
    "LiveGracePeriodRequests": 100
  },
  "LoadShedding": {
    "Enabled": true,
    "RpsThreshold": 100,
    "FailurePercentage": 25,
    "FailureStatusCode": 503
  }
}
```

## CI/CD Artifacts
```bash
# Docker Image
docker pull ghcr.io/busadave13/weather:latest

# Helm Chart
helm install weather oci://ghcr.io/busadave13/helm/weather --version 1.0.0
```

## Important Patterns
- **Health Check Counter**: Counter incremented directly in health check classes
- **Load Shedding**: Middleware pattern with sliding window counter for RPS tracking
- **MockeryHandler**: DelegatingHandler pattern for HTTP interception
- **Factory Pattern**: MockeryHandlerFactory for service-specific handlers
