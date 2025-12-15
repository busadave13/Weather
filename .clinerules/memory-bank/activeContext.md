# Active Context: Weather

## Current Session Focus
MockeryHandler implementation and simplification for service-specific mocking.

## Recent Changes

### Session: 2025-12-14 (Latest)
1. **Created MockeryHandler with service-specific mocking**
   - Added `ServiceName` property to identify which downstream service the handler is for
   - Implemented `X-Mockery-Mocks` header parsing with comma-delimited mock list
   - Matches mocks by first segment (e.g., "wind/prod/success" matches "WindSensor")
   - Random selection when multiple mocks match the same service
   - Routes matched requests to Mockery service with `X-Mock-ID` header

2. **Created MockeryHandlerFactory**
   - Factory pattern for creating service-specific handlers
   - Registered as singleton in DI container

3. **Simplified MockeryHandler**
   - Removed `X-Mock-ID` header parsing from incoming requests
   - Now only uses `X-Mockery-Mocks` header for mock selection
   - `X-Mock-ID` constant kept for calling Mockery service

4. **Added comprehensive unit tests**
   - 13 tests covering constructor, header parsing, random selection, edge cases

### Previous Session: 2024-12-14
- Created PR and checkout workflows
- Initialized memory-bank

## Active Decisions
- **MockeryHandler uses only `X-Mockery-Mocks` header** - no `X-Mock-ID` parsing from incoming requests
- **Service matching uses `Contains` with case-insensitive comparison**
- **Random selection** when multiple mocks match the same ServiceName
- Handler disabled by default via `MockeryHandlerOptions.Enabled`

## Current State
- MockeryHandler fully implemented and tested
- Factory pattern in place for service-specific handlers
- Three sensor clients registered: TemperatureSensor, WindSensor, PrecipitationSensor

## Architecture

```
Incoming Request with X-Mockery-Mocks header
    ↓
MockeryHandler (service-specific, e.g., "WindSensor")
    ↓
Parse header → Find matching mock by first segment
    ↓
Match found? → Call Mockery with X-Mock-ID header
No match?   → Call real downstream service
```

## Usage Example
```bash
curl -H "X-Mockery-Mocks: windsensor/success, temperaturesensor/success, precipitationsensor/success" \
     http://localhost:5081/api/weather
```

## Important Patterns
- Each HttpClient gets its own MockeryHandler via factory
- Service names are registered in Program.cs (e.g., "TemperatureSensor", "WindSensor")
- Mock identifiers use format: `<service-prefix>/<path>` (e.g., "wind/prod/success")
