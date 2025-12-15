# Fortio Load Testing

This directory contains [Fortio](https://github.com/fortio/fortio) load testing scripts to test the Weather API endpoints and validate the load shedding middleware.

## Why Fortio?

Fortio is a load testing tool originally developed for Istio. It excels at:
- **Constant QPS mode** - Maintains exact queries per second regardless of response latency
- **Precise latency histograms** - Shows detailed percentile breakdowns
- **Simple CLI** - No scripting required
- **Docker-first** - Easy to run via Docker
- **Pretty output** - Scripts parse results for easy-to-read summaries

## Prerequisites

- **Docker** (recommended) - No local installation needed
- OR **Fortio** installed locally ([installation guide](https://github.com/fortio/fortio#installation))
- The Weather API must be running locally (default: `http://localhost:5081`)

## Quick Start

```powershell
# Run at 50 QPS (below load shedding threshold)
.\run-test.ps1 -Qps 50 -Duration 5s

# Run at 150 QPS (triggers load shedding)
.\run-test.ps1 -Qps 150 -Duration 10s
```

## Sample Output

```
============================================
     Fortio Load Test - Weather API
============================================
Target URL:    http://localhost:5081/api/weather
QPS:           150 requests/second
Duration:      10s
Connections:   8
============================================

Running load test...

============================================
           LOAD TEST RESULTS
============================================

  Total Requests:    1496
  Actual QPS:        149.5

  HTTP Status Codes:
    ✓ 200 OK:        1463 (97.8%)
    ⚠ 503 Shed:      33 (2.2%)

  Latency (ms):
    Avg:    29.43
    P50:    26.42
    P90:    40.6
    P99:    115.2
    Max:    149.38

  ============================================
  LOAD SHEDDING:  2.2% of requests rejected
  ============================================
```

## Scripts

| Script | Platform | Description |
|--------|----------|-------------|
| `run-test.ps1` | Windows (PowerShell) | Wrapper script with named parameters |
| `run-test.sh` | Linux/macOS (Bash) | Wrapper script with positional parameters |

## Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| QPS | 100 | Queries (requests) per second |
| Duration | 30s | Test duration (e.g., 30s, 1m, 5m) |
| URL | `http://localhost:5081/api/weather` | Target endpoint URL |
| MockeryMocks | `windsensor/success, temperaturesensor/success, precipitationsensor/success` | X-Mockery-Mocks header value |
| Connections | 8 | Number of concurrent connections |
| UseDocker | true | Run via Docker (default) |

## Usage

### PowerShell (Windows)

```powershell
# Basic test with defaults (100 QPS for 30 seconds via Docker)
.\run-test.ps1

# Custom QPS and duration
.\run-test.ps1 -Qps 50 -Duration 10s

# Test load shedding (exceed threshold)
.\run-test.ps1 -Qps 200 -Duration 1m

# More concurrent connections
.\run-test.ps1 -Qps 150 -Connections 16

# Use locally installed Fortio
.\run-test.ps1 -Qps 100 -UseDocker:$false

# Custom mockery mocks
.\run-test.ps1 -Qps 100 -MockeryMocks "windsensor/error, temperaturesensor/timeout"
```

### Bash (Linux/macOS)

```bash
# Make script executable (first time only)
chmod +x run-test.sh

# Basic test with defaults (100 QPS for 30 seconds via Docker)
./run-test.sh

# Custom QPS and duration
./run-test.sh 50 10s

# Test load shedding
./run-test.sh 200 1m

# Use locally installed Fortio
./run-test.sh 100 30s http://localhost:5081/api/weather "" 8 --local
```

### Direct Docker Usage

```bash
# Run Fortio directly via Docker (raw output)
docker run --rm fortio/fortio load \
    -qps 150 \
    -t 30s \
    -c 8 \
    -payload "" \
    -H "X-Mockery-Mocks: windsensor/success, temperaturesensor/success, precipitationsensor/success" \
    http://host.docker.internal:5081/api/weather
```

## Testing Load Shedding

The Weather API has a configurable load shedding middleware. To test it:

1. **Configure load shedding** in `src/Weather/appsettings.json`:
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

2. **Start the Weather API**:
   ```bash
   cd src/Weather
   dotnet run
   ```

3. **Run load test below threshold** (all requests succeed):
   ```powershell
   .\run-test.ps1 -Qps 50 -Duration 5s
   ```

4. **Run load test above threshold** (triggers load shedding):
   ```powershell
   .\run-test.ps1 -Qps 150 -Duration 10s
   ```

## Load Shedding Scenarios

| Scenario | QPS Setting | Expected Result |
|----------|-------------|-----------------|
| Below threshold | 50 | Code 200: 100% |
| At threshold | 100 | Code 200: ~100% |
| Above threshold | 150 | Some 503 responses based on FailurePercentage |
| High load | 300 | Higher percentage of 503 responses |

## Troubleshooting

### Docker not found
Install Docker from https://www.docker.com/get-started

### Connection refused
1. Verify the Weather API is running: `curl http://localhost:5081/api/weather`
2. Check the correct port is configured in `launchSettings.json`

### Host unreachable from Docker
The scripts automatically replace `localhost` with `host.docker.internal` for Docker compatibility.

### QPS not achieved
- Increase `-c` (connections) if single connection can't sustain the target QPS
- Check if the server can handle the load
- Network latency may limit achievable QPS

### 500 errors without mockery
The Weather API requires sensor services or mockery mocks. Ensure the `X-Mockery-Mocks` header is set (default in scripts).
