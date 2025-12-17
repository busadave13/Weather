# Active Context: Weather

## Current Session Focus
Installed Sequential Thinking MCP Server for structured problem-solving.

## Recent Changes

### Session: 2025-12-16 (Latest - Sequential Thinking MCP Server)
1. **Installed Sequential Thinking MCP Server**
   - Configured npx-based Sequential Thinking MCP Server in cline_mcp_settings.json
   - Server name: `github.com/modelcontextprotocol/servers/tree/main/src/sequentialthinking`
   - Package: `@modelcontextprotocol/server-sequential-thinking`
   - Created directory: `C:\Users\daveh\OneDrive\Documents\Cline\MCP\sequentialthinking`
   - Demonstrated with task dependency problem (socks → shoes → laces)

2. **Available Tool**
   - `sequentialthinking` - Step-by-step thinking for complex problem-solving
     - Break down complex problems
     - Revise previous thoughts
     - Branch into alternative reasoning paths
     - Dynamic thought count adjustment
     - Hypothesis generation and verification

### Session: 2025-12-16 (GitHub MCP Server)
1. **Installed GitHub MCP Server**
   - Configured Docker-based GitHub MCP Server in cline_mcp_settings.json
   - Server name: `github.com/github/github-mcp-server`
   - Uses Docker image `ghcr.io/github/github-mcp-server`
   - Authenticated with GitHub Personal Access Token
   - Verified working with `get_me` tool - returned user profile for busadave13

2. **Available GitHub Tools**
   - Repository management (create, fork, branches, files, commits)
   - Issues and Pull Requests (create, update, list, search)
   - Code search and security alerts
   - GitHub Actions workflows
   - And many more (see Connected MCP Servers section)

### Session: 2025-12-16 (Alerts API)
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
