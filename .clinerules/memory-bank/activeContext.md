# Active Context: Weather

## Current Session Focus
Load Shedding middleware implementation for rate-based request rejection.

## Recent Changes

### Session: 2025-12-14 (Latest)
1. **Implemented Load Shedding Middleware**
   - Created `LoadSheddingOptions` configuration class
   - Created `LoadSheddingMiddleware` with sliding window RPS tracking
   - Created `LoadSheddingExtensions` for DI registration
   - Added configuration section in `appsettings.json`
   - Added 17 comprehensive unit tests

2. **Load Shedding Features**
   - Configurable RPS threshold to trigger shedding
   - Configurable failure percentage (0-100%)
   - Configurable HTTP status code for rejected requests
   - Configurable sliding window duration
   - Applies to all `/api/weather` endpoints (case-insensitive)
   - Structured logging for monitoring

### Previous Session: 2025-12-14
1. **Created MockeryHandler with service-specific mocking**
   - Added `ServiceName` property to identify which downstream service the handler is for
   - Implemented `X-Mockery-Mocks` header parsing with comma-delimited mock list
   - Matches mocks by first segment (e.g., "wind/prod/success" matches "WindSensor")
   - Random selection when multiple mocks match the same service
   - Routes matched requests to Mockery service with `X-Mockery-Mock` header

2. **Created MockeryHandlerFactory**
   - Factory pattern for creating service-specific handlers
   - Registered as singleton in DI container

3. **Simplified MockeryHandler**
   - Removed `X-Mock-ID` header parsing from incoming requests
   - Now only uses `X-Mockery-Mocks` header for mock selection
   - `X-Mockery-Mock` header used when calling Mockery service

4. **Added comprehensive unit tests**
   - 13 tests covering constructor, header parsing, random selection, edge cases

### Previous Session: 2024-12-14
- Created PR and checkout workflows
- Initialized memory-bank

## Active Decisions
- **Load Shedding disabled by default** - Must set `Enabled: true` to activate
- **MockeryHandler uses only `X-Mockery-Mocks` header** - no `X-Mock-ID` parsing from incoming requests
- **Service matching uses `Contains` with case-insensitive comparison**
- **Random selection** when multiple mocks match the same ServiceName

## Current State
- Load shedding middleware fully implemented and tested (17 tests)
- MockeryHandler fully implemented and tested
- Factory pattern in place for service-specific handlers
- Three sensor clients registered: TemperatureSensor, WindSensor, PrecipitationSensor

## Architecture

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
RPS > Threshold
    ↓
Random(0-100) >= FailurePercentage? → Continue to Controller
    ↓
Random(0-100) < FailurePercentage → Return FailureStatusCode (503)
```

### MockeryHandler Flow
```
Incoming Request with X-Mockery-Mocks header
    ↓
MockeryHandler (service-specific, e.g., "WindSensor")
    ↓
Parse header → Find matching mock by first segment
    ↓
Match found? → Call Mockery with X-Mockery-Mock header
No match?   → Call real downstream service
```

## Configuration Example
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

## Usage Example
```bash
curl -H "X-Mockery-Mocks: windsensor/success, temperaturesensor/success, precipitationsensor/success" \
     http://localhost:5081/api/weather
```

## Important Patterns
- **Load Shedding**: Middleware pattern with sliding window counter for RPS tracking
- Each HttpClient gets its own MockeryHandler via factory
- Service names are registered in Program.cs (e.g., "TemperatureSensor", "WindSensor")
- Mock identifiers use format: `<service-prefix>/<path>` (e.g., "wind/prod/success")
