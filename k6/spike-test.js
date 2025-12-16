import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const weatherRequestDuration = new Trend('weather_request_duration');
const loadSheddingCount = new Counter('load_shedding_count');
const successfulRequests = new Counter('successful_requests');
const failedRequests = new Counter('failed_requests');
const errorRate = new Rate('errors');

// Configuration
const BASE_URL = __ENV.K6_BASE_URL || 'http://localhost:5081';

// Spike test options - sudden traffic spikes to test load shedding
export const options = {
    stages: [
        // Baseline traffic
        { duration: '30s', target: 5 },   // Start with 5 VUs for 30 seconds
        
        // SPIKE! Sudden increase
        { duration: '10s', target: 100 }, // Spike to 100 VUs in 10 seconds
        { duration: '1m', target: 100 },  // Stay at 100 VUs for 1 minute
        
        // Recovery
        { duration: '10s', target: 5 },   // Drop back to 5 VUs
        { duration: '30s', target: 5 },   // Stay at 5 VUs for 30 seconds
        
        // Second SPIKE!
        { duration: '10s', target: 150 }, // Spike to 150 VUs
        { duration: '1m', target: 150 },  // Stay at 150 VUs for 1 minute
        
        // Final recovery
        { duration: '30s', target: 0 },   // Ramp down to 0
    ],
    thresholds: {
        // Spike tests expect some failures, but not complete failure
        http_req_duration: ['p(90)<3000'],  // 90% under 3 seconds
        http_req_failed: ['rate<0.6'],       // Allow up to 60% failures during spikes
        errors: ['rate<0.6'],
    },
};

// Request headers
const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

// Test the weather endpoint during spike
function testWeatherEndpoint() {
    const startTime = Date.now();
    const url = `${BASE_URL}/api/weather`;
    const response = http.get(url, { headers, tags: { type: 'weather' } });
    
    weatherRequestDuration.add(response.timings.duration);
    
    // Check for load shedding response
    if (response.status === 503) {
        const isLoadShedding = response.headers['X-Load-Shedding'] === 'true';
        if (isLoadShedding) {
            loadSheddingCount.add(1);
            // Load shedding is working as designed
            check(response, {
                'spike: load shedding triggered': (r) => r.status === 503,
                'spike: has retry-after header': (r) => r.headers['Retry-After'] !== undefined,
            });
            return { response, loadShed: true };
        }
        // 503 but not load shedding - actual error
        failedRequests.add(1);
        errorRate.add(1);
        return { response, loadShed: false };
    }
    
    const success = check(response, {
        'spike: status is 200': (r) => r.status === 200,
        'spike: response time < 3000ms': (r) => r.timings.duration < 3000,
    });
    
    if (success) {
        successfulRequests.add(1);
        errorRate.add(0);
    } else {
        failedRequests.add(1);
        errorRate.add(1);
    }
    
    return { response, loadShed: false };
}

// Monitor health during spikes
function testHealthEndpoint() {
    const response = http.get(`${BASE_URL}/health/live`, { 
        headers, 
        tags: { type: 'health' },
        timeout: '10s'  // Longer timeout during spikes
    });
    
    check(response, {
        'health: service still alive during spike': (r) => r.status === 200,
    });
    
    return response;
}

// Main spike test function
export default function () {
    // Fire requests as fast as possible during spikes
    testWeatherEndpoint();
    
    // Very short sleep to simulate concurrent requests
    sleep(0.05);
    
    // Check health occasionally
    if (__ITER % 100 === 0) {
        testHealthEndpoint();
    }
}

// Setup function
export function setup() {
    console.log(`Starting SPIKE test against: ${BASE_URL}`);
    console.log('This test simulates sudden traffic spikes to validate load shedding behavior.');
    console.log('');
    console.log('Test phases:');
    console.log('  1. Baseline (5 VUs) - 30 seconds');
    console.log('  2. SPIKE to 100 VUs - 1 minute');
    console.log('  3. Recovery (5 VUs) - 30 seconds');
    console.log('  4. SPIKE to 150 VUs - 1 minute');
    console.log('  5. Final ramp down');
    console.log('');
    
    // Verify the service is reachable
    const response = http.get(`${BASE_URL}/health/live`);
    if (response.status !== 200) {
        throw new Error(`Service not reachable at ${BASE_URL}. Status: ${response.status}`);
    }
    
    // Check if load shedding is enabled
    const configResponse = http.get(`${BASE_URL}/api/config`);
    if (configResponse.status === 200) {
        try {
            const config = JSON.parse(configResponse.body);
            console.log(`Load shedding enabled: ${config.loadSheddingEnabled || 'unknown'}`);
        } catch {
            console.log('Could not parse config response');
        }
    }
    
    console.log('Service is reachable. Starting spike test...');
    console.log('');
    
    return { startTime: Date.now() };
}

// Teardown function
export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000;
    console.log('');
    console.log('═'.repeat(60));
    console.log(`Spike test completed in ${duration.toFixed(2)} seconds`);
    console.log('═'.repeat(60));
}

// Custom summary handler
export function handleSummary(data) {
    // Calculate load shedding statistics
    const totalReqs = data.metrics.http_reqs?.values?.count || 0;
    const shedCount = data.metrics.load_shedding_count?.values?.count || 0;
    const successCount = data.metrics.successful_requests?.values?.count || 0;
    const failCount = data.metrics.failed_requests?.values?.count || 0;
    
    console.log('');
    console.log('📊 SPIKE TEST ANALYSIS');
    console.log('─'.repeat(40));
    console.log(`Total HTTP requests:    ${totalReqs}`);
    console.log(`Successful requests:    ${successCount}`);
    console.log(`Failed requests:        ${failCount}`);
    console.log(`Load shedding events:   ${shedCount}`);
    console.log('');
    
    if (totalReqs > 0) {
        const successRate = ((successCount / totalReqs) * 100).toFixed(2);
        const shedRate = ((shedCount / totalReqs) * 100).toFixed(2);
        console.log(`Success rate:           ${successRate}%`);
        console.log(`Load shedding rate:     ${shedRate}%`);
    }
    
    console.log('─'.repeat(40));
    
    // Determine if load shedding is working
    if (shedCount > 0) {
        console.log('✅ Load shedding is ACTIVE and protecting the service');
    } else if (totalReqs > 100) {
        console.log('⚠️  No load shedding events detected.');
        console.log('   Either load shedding is disabled or threshold wasn\'t reached.');
    }
    
    console.log('');
    
    return {
        'stdout': textSummary(data, { indent: '  ', enableColors: true }),
    };
}

// Import text summary helper
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
