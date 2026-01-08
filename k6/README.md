# k6 Load Testing for Weather API

This folder contains a k6 load testing script for the Weather API service.

## Prerequisites

Install k6:

```bash
# macOS (homebrew)
brew install k6

# Windows (chocolatey)
choco install k6

# Docker
docker pull grafana/k6
```

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `BASE_URL` | `http://localhost:5081` | Target API URL |
| `RPS` | `10` | Target requests per second |
| `DURATION` | `1m` | Test duration (e.g., `30s`, `5m`, `1h`) |

## Running Tests

### Basic Usage

```bash
# Default: 10 RPS for 1 minute against localhost:5081
k6 run load-test.js
```

### Custom Configuration

```bash
# 50 RPS for 5 minutes
RPS=50 DURATION=5m k6 run load-test.js

# Against a different URL
BASE_URL=https://api.example.com k6 run load-test.js

# Full customization
BASE_URL=https://api.example.com RPS=100 DURATION=10m k6 run load-test.js
```

### Windows (PowerShell)

```powershell
$env:BASE_URL = "http://localhost:5081"
$env:RPS = "50"
$env:DURATION = "5m"
k6 run load-test.js
```

### Using Docker

```bash
docker run --rm -i \
  -e BASE_URL=http://host.docker.internal:5081 \
  -e RPS=50 \
  -e DURATION=5m \
  -v ${PWD}/k6:/scripts \
  grafana/k6 run /scripts/load-test.js
```

## Thresholds

The test includes automatic pass/fail thresholds:

| Metric | Threshold | Description |
|--------|-----------|-------------|
| `http_req_duration` | p(95) < 500ms | 95% of requests under 500ms |
| `http_req_failed` | rate < 0.1 | Less than 10% error rate |
| `weather_request_duration` | p(95) < 400ms | Weather endpoint under 400ms |

## Output Options

```bash
# JSON output
k6 run --out json=results.json load-test.js

# InfluxDB (for Grafana dashboards)
k6 run --out influxdb=http://localhost:8086/k6 load-test.js
