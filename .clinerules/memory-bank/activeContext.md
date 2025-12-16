# Active Context: Weather

## Current Session Focus
Test configuration API, k6 load testing, and Helm chart LoadShedding integration.

## Recent Changes

### Session: 2025-12-16 (Latest)
1. **Added Test Configuration API**
   - Created `POST /api/config` endpoint for runtime test configuration
   - Can force health checks to fail individually (startup, ready, live)
   - Can add delays to health check responses
   - `ITestConfigurationState` singleton for thread-safe state
   - 83 unit tests passing

2. **Added k6 Load Testing**
   - Created `k6/` folder with load testing scripts
   - `load-test.js` - Standard load test (ramp to 20 VUs)
   - `stress-test.js` - Stress test (ramp to 200 VUs)
   - `spike-test.js` - Spike test for load shedding validation
   - Removed fortio (replaced with k6)

3. **Added LoadShedding to Helm Chart**
   - Added `config.loadShedding.*` values to `values.yaml`
   - Added environment variables to `deployment.yaml`
   - Can now configure load shedding via Helm values or environment variables
   - Local: Use `appsettings.json`
   - Kubernetes: Use `helm upgrade --set config.loadShedding.enabled=false`

### Session: 2025-12-14
1. **Fixed MockeryHandler Header Issue**
   - Changed `TryAddWithoutValidation()` to `Headers.Add()` for reliable header addition
   - All unit tests passing

2. **Fixed Helm Chart Mockery URL**
   - Updated URL: `http://mockery.mockery.svc.cluster.local`

### Previous Sessions
- Implemented Load Shedding Middleware
- Implemented Health Checks (Live, Ready, Startup)
- Created MockeryHandler with service-specific mocking
- CI/CD pipeline for Docker and Helm chart publishing

## Active Decisions
- **Health checks increment counter directly** - No middleware needed
- **Load Shedding configurable via Helm** - Environment variables override appsettings.json
- **CI/CD uses GitHub Container Registry** - ghcr.io/busadave13/weather
- **k6 for load testing** - Replaced fortio with k6

## Current State
- 83 unit tests passing
- Test configuration API allows runtime control of health checks
- Load shedding configurable via appsettings.json or Helm values
- k6 load testing scripts available

## Configuration Example
```json
{
  "LoadShedding": {
    "Enabled": true,
    "RpsThreshold": 100,
    "FailurePercentage": 25,
    "FailureStatusCode": 503,
    "WindowSizeSeconds": 1
  }
}
```

## Helm Configuration
```yaml
config:
  loadShedding:
    enabled: true
    rpsThreshold: 100
    failurePercentage: 25
    failureStatusCode: 503
    windowSizeSeconds: 1
```

## Test Configuration API
```bash
# Force ready health check to fail
curl -X POST http://localhost:5081/api/config \
  -H "Content-Type: application/json" \
  -d '{"forceReadyFail": true}'

# Add delay to health checks
curl -X POST http://localhost:5081/api/config \
  -H "Content-Type: application/json" \
  -d '{"readyDelayMs": 5000}'

# Get current configuration
curl http://localhost:5081/api/config
```

## k6 Load Testing
```powershell
# Run load tests
k6 run k6/load-test.js
k6 run k6/stress-test.js
k6 run k6/spike-test.js
```

## Important Patterns
- **Test Configuration State**: Singleton for thread-safe runtime configuration
- **Load Shedding**: Middleware with sliding window RPS tracking
- **MockeryHandler**: DelegatingHandler pattern for HTTP interception
- **Factory Pattern**: MockeryHandlerFactory for service-specific handlers
