import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const weatherRequestDuration = new Trend('weather_request_duration');
const healthRequestDuration = new Trend('health_request_duration');
const errorRate = new Rate('errors');

// Configuration
const BASE_URL = __ENV.K6_BASE_URL || 'http://localhost:5081';

// Test options
export const options = {
    stages: [
        // Ramp up
        { duration: '30s', target: 10 },  // Ramp up to 10 VUs over 30 seconds
        { duration: '1m', target: 10 },   // Stay at 10 VUs for 1 minute
        { duration: '30s', target: 20 },  // Ramp up to 20 VUs over 30 seconds
        { duration: '1m', target: 20 },   // Stay at 20 VUs for 1 minute
        { duration: '30s', target: 0 },   // Ramp down to 0 VUs
    ],
    thresholds: {
        // 95% of requests should complete within 500ms
        http_req_duration: ['p(95)<500'],
        // Error rate should be less than 10%
        http_req_failed: ['rate<0.1'],
        // Weather endpoint specific thresholds
        weather_request_duration: ['p(95)<400'],
        // Health check should be fast
        health_request_duration: ['p(99)<100'],
        // Custom error rate
        errors: ['rate<0.1'],
    },
};

// Request headers
const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

// Test the weather endpoint
function testWeatherEndpoint() {
    const url = `${BASE_URL}/api/weather`;
    const response = http.get(url, { headers, tags: { type: 'weather' } });
    
    weatherRequestDuration.add(response.timings.duration);
    
    const success = check(response, {
        'weather: status is 200': (r) => r.status === 200,
        'weather: response time < 500ms': (r) => r.timings.duration < 500,
        'weather: has temperature data': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.temperature !== undefined;
            } catch {
                return false;
            }
        },
    });
    
    if (!success) {
        errorRate.add(1);
    } else {
        errorRate.add(0);
    }
    
    return response;
}

// Test health endpoints
function testHealthEndpoints() {
    // Live health check
    const liveResponse = http.get(`${BASE_URL}/health/live`, { 
        headers, 
        tags: { type: 'health', endpoint: 'live' } 
    });
    healthRequestDuration.add(liveResponse.timings.duration);
    
    check(liveResponse, {
        'health/live: status is 200': (r) => r.status === 200,
        'health/live: response time < 100ms': (r) => r.timings.duration < 100,
    });
    
    // Ready health check
    const readyResponse = http.get(`${BASE_URL}/health/ready`, { 
        headers, 
        tags: { type: 'health', endpoint: 'ready' } 
    });
    healthRequestDuration.add(readyResponse.timings.duration);
    
    check(readyResponse, {
        'health/ready: status is 200': (r) => r.status === 200,
        'health/ready: response time < 100ms': (r) => r.timings.duration < 100,
    });
    
    return { liveResponse, readyResponse };
}

// Test config endpoint
function testConfigEndpoint() {
    const url = `${BASE_URL}/api/config`;
    const response = http.get(url, { headers, tags: { type: 'config' } });
    
    check(response, {
        'config: status is 200': (r) => r.status === 200,
        'config: response time < 200ms': (r) => r.timings.duration < 200,
    });
    
    return response;
}

// Main test function
export default function () {
    // Primary test: Weather endpoint (most frequent)
    testWeatherEndpoint();
    sleep(0.5);
    
    // Secondary: Health checks (every 5th iteration)
    if (__ITER % 5 === 0) {
        testHealthEndpoints();
        sleep(0.1);
    }
    
    // Tertiary: Config endpoint (every 10th iteration)
    if (__ITER % 10 === 0) {
        testConfigEndpoint();
        sleep(0.1);
    }
    
    // Random sleep between iterations (0.5-1.5 seconds)
    sleep(Math.random() + 0.5);
}

// Setup function - runs once before the test
export function setup() {
    console.log(`Starting load test against: ${BASE_URL}`);
    
    // Verify the service is reachable
    const response = http.get(`${BASE_URL}/health/live`);
    if (response.status !== 200) {
        throw new Error(`Service not reachable at ${BASE_URL}. Status: ${response.status}`);
    }
    
    console.log('Service is reachable. Starting test...');
    return { startTime: Date.now() };
}

// Teardown function - runs once after the test
export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000;
    console.log(`Test completed in ${duration.toFixed(2)} seconds`);
}
