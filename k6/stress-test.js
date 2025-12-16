import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const weatherRequestDuration = new Trend('weather_request_duration');
const loadSheddingCount = new Counter('load_shedding_count');
const errorRate = new Rate('errors');

// Configuration
const BASE_URL = __ENV.K6_BASE_URL || 'http://localhost:5081';

// Stress test options - aggressive ramp up to find breaking points
export const options = {
    stages: [
        // Gradual ramp up
        { duration: '1m', target: 50 },   // Ramp up to 50 VUs over 1 minute
        { duration: '2m', target: 50 },   // Stay at 50 VUs for 2 minutes
        { duration: '1m', target: 100 },  // Ramp up to 100 VUs over 1 minute
        { duration: '2m', target: 100 },  // Stay at 100 VUs for 2 minutes
        { duration: '1m', target: 200 },  // Ramp up to 200 VUs over 1 minute
        { duration: '2m', target: 200 },  // Stay at 200 VUs for 2 minutes
        { duration: '1m', target: 0 },    // Ramp down to 0 VUs
    ],
    thresholds: {
        // More lenient thresholds for stress testing
        http_req_duration: ['p(95)<2000'],  // 95% under 2 seconds
        http_req_failed: ['rate<0.5'],       // Allow up to 50% failures (stress test)
        weather_request_duration: ['p(95)<1500'],
        errors: ['rate<0.5'],
    },
};

// Request headers
const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

// Test the weather endpoint under stress
function testWeatherEndpoint() {
    const url = `${BASE_URL}/api/weather`;
    const response = http.get(url, { headers, tags: { type: 'weather' } });
    
    weatherRequestDuration.add(response.timings.duration);
    
    // Check for load shedding response
    if (response.status === 503 && response.headers['X-Load-Shedding'] === 'true') {
        loadSheddingCount.add(1);
        // Load shedding is expected behavior, not an error
        check(response, {
            'weather: load shedding active': (r) => r.status === 503,
            'weather: has retry-after header': (r) => r.headers['Retry-After'] !== undefined,
        });
        return response;
    }
    
    const success = check(response, {
        'weather: status is 200': (r) => r.status === 200,
        'weather: response time < 2000ms': (r) => r.timings.duration < 2000,
        'weather: has valid response': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.temperature !== undefined || body.error !== undefined;
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

// Quick health check to monitor service health during stress
function testHealthEndpoint() {
    const response = http.get(`${BASE_URL}/health/live`, { 
        headers, 
        tags: { type: 'health' },
        timeout: '5s'
    });
    
    check(response, {
        'health: service is alive': (r) => r.status === 200,
    });
    
    return response;
}

// Main stress test function
export default function () {
    // Hammer the weather endpoint
    testWeatherEndpoint();
    
    // Minimal sleep to maximize load
    sleep(0.1);
    
    // Occasional health check to verify service is still up
    if (__ITER % 50 === 0) {
        testHealthEndpoint();
    }
}

// Setup function
export function setup() {
    console.log(`Starting STRESS test against: ${BASE_URL}`);
    console.log('WARNING: This test will push the service to its limits!');
    
    // Verify the service is reachable
    const response = http.get(`${BASE_URL}/health/live`);
    if (response.status !== 200) {
        throw new Error(`Service not reachable at ${BASE_URL}. Status: ${response.status}`);
    }
    
    // Check current config
    const configResponse = http.get(`${BASE_URL}/api/config`);
    if (configResponse.status === 200) {
        console.log(`Current config: ${configResponse.body}`);
    }
    
    console.log('Service is reachable. Starting stress test...');
    return { startTime: Date.now() };
}

// Teardown function
export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000;
    console.log(`Stress test completed in ${duration.toFixed(2)} seconds`);
    console.log('Review the load_shedding_count metric to see how many requests were shed');
}

// Handle summary - custom summary at the end
export function handleSummary(data) {
    const summary = {
        'stdout': textSummary(data, { indent: '  ', enableColors: true }),
    };
    
    // Additional analysis
    if (data.metrics.load_shedding_count) {
        const shedCount = data.metrics.load_shedding_count.values.count || 0;
        const totalReqs = data.metrics.http_reqs.values.count || 0;
        const shedPercentage = ((shedCount / totalReqs) * 100).toFixed(2);
        console.log(`\n📊 Load Shedding Analysis:`);
        console.log(`   Total requests: ${totalReqs}`);
        console.log(`   Requests shed: ${shedCount} (${shedPercentage}%)`);
    }
    
    return summary;
}

// Import text summary helper
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
