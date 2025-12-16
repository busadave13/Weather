import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter } from 'k6/metrics';

// Custom metrics
const loadSheddingRate = new Rate('load_shedding_rate');
const successRate = new Rate('success_rate');
const rejectedRequests = new Counter('rejected_requests');
const successfulRequests = new Counter('successful_requests');

// Configuration via environment variables
// Usage: k6 run -e RPS=100 -e DURATION=30s alerts-load-test.js
const RPS = __ENV.RPS ? parseInt(__ENV.RPS) : 50;
const DURATION = __ENV.DURATION || '30s';
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5081';

export const options = {
    scenarios: {
        alerts_load: {
            executor: 'constant-arrival-rate',
            rate: RPS,
            timeUnit: '1s',
            duration: DURATION,
            preAllocatedVUs: Math.max(RPS * 2, 50),
            maxVUs: Math.max(RPS * 4, 200),
        },
    },
    thresholds: {
        'http_req_duration': ['p(95)<500'],
        'success_rate': ['rate>0.5'],
    },
};

export default function () {
    const response = http.get(`${BASE_URL}/api/alerts`);

    // Check for success (204 No Content)
    const isSuccess = response.status === 204;

    // Check for load shedding (503 Service Unavailable)
    const isLoadShed = response.status === 503;

    // Check for load shedding header
    const hasLoadSheddingHeader = response.headers['X-Load-Shedding'] === 'true';

    // Record metrics
    successRate.add(isSuccess);
    loadSheddingRate.add(isLoadShed);

    if (isSuccess) {
        successfulRequests.add(1);
    }

    if (isLoadShed) {
        rejectedRequests.add(1);
    }

    check(response, {
        'status is 204 or 503': (r) => r.status === 204 || r.status === 503,
        'successful request': (r) => r.status === 204,
        'load shedding response': (r) => r.status === 503,
        'has retry-after header when shed': (r) => r.status !== 503 || r.headers['Retry-After'] !== undefined,
    });
}

export function handleSummary(data) {
    const totalRequests = data.metrics.http_reqs.values.count;
    const successCount = data.metrics.successful_requests ? data.metrics.successful_requests.values.count : 0;
    const rejectedCount = data.metrics.rejected_requests ? data.metrics.rejected_requests.values.count : 0;
    const loadSheddingPct = rejectedCount > 0 ? ((rejectedCount / totalRequests) * 100).toFixed(2) : '0.00';

    console.log('\n========================================');
    console.log('         ALERTS LOAD TEST SUMMARY');
    console.log('========================================');
    console.log(`Target RPS:        ${RPS}`);
    console.log(`Duration:          ${DURATION}`);
    console.log(`Base URL:          ${BASE_URL}`);
    console.log('----------------------------------------');
    console.log(`Total Requests:    ${totalRequests}`);
    console.log(`Successful (204):  ${successCount}`);
    console.log(`Load Shed (503):   ${rejectedCount}`);
    console.log(`Load Shedding %:   ${loadSheddingPct}%`);
    console.log('========================================\n');

    return {
        stdout: JSON.stringify({
            config: { rps: RPS, duration: DURATION, baseUrl: BASE_URL },
            results: {
                totalRequests,
                successCount,
                rejectedCount,
                loadSheddingPercentage: parseFloat(loadSheddingPct)
            }
        }, null, 2)
    };
}
