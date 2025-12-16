# k6 Load Testing for Weather API

This folder contains k6 load testing scripts for the Weather API service.

## Prerequisites

Install k6:

```bash
# Windows (chocolatey)
choco install k6

# macOS (homebrew)
brew install k6

# Docker
docker pull grafana/k6
```

## Available Tests

| Script | Purpose | Description |
|--------|---------|-------------|
| `load-test.js` | Standard Load Test | Gradual ramp-up, sustained load, graceful ramp-down |
| `stress-test.js` | Stress Test | Push beyond normal capacity to find breaking points |
| `spike-test.js` | Spike Test | Sudden traffic spikes to test load shedding |

## Running Tests

### Local Development

```bash
# Set the target URL (default: http://localhost:5081)
export K6_BASE_URL=http://localhost:5081

# Run load test
k6 run load-test.js

# Run stress test
k6 run stress-test.js

# Run spike test
k6 run spike-test.js
```

### Windows (PowerShell)

```powershell
# Set the target URL
$env:K6_BASE_URL = "http://localhost:5081"

# Run load test
k6 run load-test.js
```

### Using Docker

```bash
# Run from project root
docker run --rm -i \
  -e K6_BASE_URL=http://host.docker.internal:5081 \
  -v ${PWD}/k6:/scripts \
  grafana/k6 run /scripts/load-test.js
```

### Kubernetes Testing

```bash
# Port forward the weather service
kubectl port-forward svc/weather 8080:80 -n weather &

# Run the test
K6_BASE_URL=http://localhost:8080 k6 run load-test.js
```

## Test Scenarios

### Load Test (`load-test.js`)

Standard load test with gradual traffic patterns:

```
Stage 1: Ramp up to 10 VUs over 30 seconds
Stage 2: Stay at 10 VUs for 1 minute
Stage 3: Ramp up to 20 VUs over 30 seconds
Stage 4: Stay at 20 VUs for 1 minute
Stage 5: Ramp down to 0 VUs over 30 seconds
```

### Stress Test (`stress-test.js`)

Aggressive test to find system limits:

```
Stage 1: Ramp up to 50 VUs over 1 minute
Stage 2: Stay at 50 VUs for 2 minutes
Stage 3: Ramp up to 100 VUs over 1 minute
Stage 4: Stay at 100 VUs for 2 minutes
Stage 5: Ramp up to 200 VUs over 1 minute
Stage 6: Stay at 200 VUs for 2 minutes
Stage 7: Ramp down to 0 VUs over 1 minute
```

### Spike Test (`spike-test.js`)

Sudden traffic spikes to test load shedding:

```
Stage 1: Start with 5 VUs for 30 seconds
Stage 2: Spike to 100 VUs instantly
Stage 3: Stay at 100 VUs for 1 minute
Stage 4: Drop to 5 VUs
Stage 5: Stay at 5 VUs for 30 seconds
```

## Thresholds

All tests include thresholds that determine pass/fail:

| Metric | Threshold | Description |
|--------|-----------|-------------|
| `http_req_duration` | p(95) < 500ms | 95% of requests under 500ms |
| `http_req_failed` | rate < 0.1 | Less than 10% error rate |
| `http_req_duration{type:health}` | p(99) < 100ms | Health checks under 100ms |

## Output Options

### JSON Output

```bash
k6 run --out json=results.json load-test.js
```

### InfluxDB Output (for Grafana dashboards)

```bash
k6 run --out influxdb=http://localhost:8086/k6 load-test.js
```

### Cloud Output (k6 Cloud)

```bash
k6 cloud load-test.js
```

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `K6_BASE_URL` | `http://localhost:5081` | Base URL of the Weather API |
| `K6_VUS` | varies by test | Override virtual users count |
| `K6_DURATION` | varies by test | Override test duration |

## Testing Load Shedding

To test the load shedding middleware:

1. Enable load shedding in the Weather API:
   ```bash
   curl -X POST http://localhost:5081/api/config \
     -H "Content-Type: application/json" \
     -d '{"loadSheddingEnabled": true}'
   ```

2. Run the spike test:
   ```bash
   k6 run spike-test.js
   ```

3. Observe the load shedding behavior in the results.

## Interpreting Results

### Key Metrics

- **http_reqs**: Total number of HTTP requests made
- **http_req_duration**: Request duration statistics (min, max, avg, p90, p95, p99)
- **http_req_failed**: Percentage of failed requests
- **vus**: Number of active virtual users
- **iterations**: Number of completed test iterations

### Sample Output

```
     ✓ status is 200
     ✓ response time < 500ms

     checks.........................: 100.00% ✓ 1234  ✗ 0
     data_received..................: 1.2 MB  12 kB/s
     data_sent......................: 456 kB  4.5 kB/s
     http_req_blocked...............: avg=1.2ms   min=0s      max=45ms
     http_req_connecting............: avg=0.8ms   min=0s      max=32ms
     http_req_duration..............: avg=45ms    min=12ms    max=234ms
       { expected_response:true }...: avg=45ms    min=12ms    max=234ms
     http_req_failed................: 0.00%   ✓ 0     ✗ 1234
     http_req_receiving.............: avg=0.2ms   min=0s      max=5ms
     http_req_sending...............: avg=0.1ms   min=0s      max=2ms
     http_req_tls_handshaking.......: avg=0s      min=0s      max=0s
     http_req_waiting...............: avg=44ms    min=11ms    max=232ms
     http_reqs......................: 1234    12.34/s
     iteration_duration.............: avg=80ms    min=50ms    max=300ms
     iterations.....................: 1234    12.34/s
     vus............................: 20      min=1   max=20
     vus_max........................: 20      min=20  max=20
