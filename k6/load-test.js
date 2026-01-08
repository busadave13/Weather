import http from 'k6/http';
import { check } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const requestDuration = new Trend('weather_request_duration');
const errorRate = new Rate('errors');

// Configuration from environment variables
const BASE_URL = __ENV.BASE_URL || 'http://weather.local.com';
const RPS = parseInt(__ENV.RPS) || 10;
const DURATION = __ENV.DURATION || '1m';

export const options = {
    scenarios: {
        constant_load: {
            executor: 'constant-arrival-rate',
            rate: RPS,
            timeUnit: '1s',
            duration: DURATION,
            preAllocatedVUs: Math.max(RPS * 2, 10),
            maxVUs: Math.max(RPS * 5, 50),
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.1'],
        weather_request_duration: ['p(95)<400'],
        errors: ['rate<0.1'],
    },
};

const headers = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    'X-Mockery-Mocks': 'windsensor/success, windsensor/success-2, temperaturesensor/success, precipitationsensor/success'
};

export default function () {
    const url = `${BASE_URL}/api/weather`;
    const response = http.get(url, { headers });

    requestDuration.add(response.timings.duration);

    const success = check(response, {
        'status is 200': (r) => r.status === 200,
        'response time < 500ms': (r) => r.timings.duration < 500,
    });

    errorRate.add(success ? 0 : 1);
}

export function setup() {
    console.log(`Load Test Configuration:`);
    console.log(`  BASE_URL: ${BASE_URL}`);
    console.log(`  RPS: ${RPS}`);
    console.log(`  DURATION: ${DURATION}`);

    const response = http.get(`${BASE_URL}/health/live`);
    if (response.status !== 200) {
        throw new Error(`Service not reachable at ${BASE_URL}. Status: ${response.status}`);
    }

    console.log('Service is reachable. Starting test...');
    return { startTime: Date.now() };
}

export function teardown(data) {
    const duration = (Date.now() - data.startTime) / 1000;
    console.log(`Test completed in ${duration.toFixed(2)} seconds`);
}
