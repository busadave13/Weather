#!/bin/bash
#
# Fortio Load Test Script for Weather API with Constant QPS
# Provides pretty-printed results with load shedding statistics.
#
# Usage:
#   ./run-test.sh [QPS] [DURATION] [URL] [MOCKERY_MOCKS] [CONNECTIONS] [--local]
#
# Examples:
#   ./run-test.sh 150 30s          # Test load shedding at 150 QPS
#   ./run-test.sh 200 1m           # 1-minute test at 200 QPS

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULT_FILE="$SCRIPT_DIR/fortio-result.json"

# Default parameters
QPS=${1:-100}
DURATION=${2:-"30s"}
URL=${3:-"http://localhost:5081/api/weather"}
MOCKERY_MOCKS=${4:-"windsensor/success, temperaturesensor/success, precipitationsensor/success"}
CONNECTIONS=${5:-8}
USE_DOCKER=true

# Check for --local flag
for arg in "$@"; do
    if [ "$arg" == "--local" ]; then
        USE_DOCKER=false
    fi
done

# Colors
CYAN='\033[0;36m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
RED='\033[0;31m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Cleanup function
cleanup() {
    if [ -f "$RESULT_FILE" ]; then
        rm -f "$RESULT_FILE"
    fi
}
trap cleanup EXIT

echo ""
echo -e "${CYAN}============================================${NC}"
echo -e "${CYAN}     Fortio Load Test - Weather API${NC}"
echo -e "${CYAN}============================================${NC}"
echo "Target URL:    $URL"
echo "QPS:           $QPS requests/second"
echo "Duration:      $DURATION"
echo "Connections:   $CONNECTIONS"
echo -e "${CYAN}============================================${NC}"
echo ""
echo -e "${YELLOW}Running load test...${NC}"
echo ""

if [ "$USE_DOCKER" = true ]; then
    DOCKER_URL="${URL/localhost/host.docker.internal}"
    
    docker run --rm \
        fortio/fortio load \
        -qps "$QPS" \
        -t "$DURATION" \
        -c "$CONNECTIONS" \
        -payload "" \
        -json - \
        -H "X-Mockery-Mocks: $MOCKERY_MOCKS" \
        "$DOCKER_URL" 2>&1 > "$RESULT_FILE"
else
    if ! command -v fortio &> /dev/null; then
        echo -e "${RED}Fortio is not installed.${NC}"
        exit 1
    fi

    fortio load \
        -qps "$QPS" \
        -t "$DURATION" \
        -c "$CONNECTIONS" \
        -payload "" \
        -json - \
        -H "X-Mockery-Mocks: $MOCKERY_MOCKS" \
        "$URL" 2>&1 > "$RESULT_FILE"
fi

# Check if jq is available for JSON parsing
if command -v jq &> /dev/null; then
    # Extract the JSON part (skip log lines)
    JSON_CONTENT=$(grep -o '{.*' "$RESULT_FILE" | head -1)
    
    if [ -n "$JSON_CONTENT" ]; then
        # Parse metrics using jq
        TOTAL_REQUESTS=$(echo "$JSON_CONTENT" | jq '[.RetCodes | to_entries[] | .value] | add')
        CODE_200=$(echo "$JSON_CONTENT" | jq '.RetCodes["200"] // 0')
        CODE_503=$(echo "$JSON_CONTENT" | jq '.RetCodes["503"] // 0')
        ACTUAL_QPS=$(echo "$JSON_CONTENT" | jq '.ActualQPS | . * 10 | round / 10')
        AVG_LATENCY=$(echo "$JSON_CONTENT" | jq '.DurationHistogram.Avg * 1000 | . * 100 | round / 100')
        P50=$(echo "$JSON_CONTENT" | jq '.DurationHistogram.Percentiles[0].Value * 1000 | . * 100 | round / 100')
        P90=$(echo "$JSON_CONTENT" | jq '.DurationHistogram.Percentiles[2].Value * 1000 | . * 100 | round / 100')
        P99=$(echo "$JSON_CONTENT" | jq '.DurationHistogram.Percentiles[4].Value * 1000 | . * 100 | round / 100')
        MAX_LATENCY=$(echo "$JSON_CONTENT" | jq '.DurationHistogram.Max * 1000 | . * 100 | round / 100')
        
        # Calculate percentages
        if [ "$TOTAL_REQUESTS" -gt 0 ]; then
            PCT_200=$(echo "scale=1; $CODE_200 * 100 / $TOTAL_REQUESTS" | bc)
            PCT_503=$(echo "scale=1; $CODE_503 * 100 / $TOTAL_REQUESTS" | bc)
        else
            PCT_200=0
            PCT_503=0
        fi
        
        # Print pretty results
        echo ""
        echo -e "${GREEN}============================================${NC}"
        echo -e "${GREEN}           LOAD TEST RESULTS${NC}"
        echo -e "${GREEN}============================================${NC}"
        echo ""
        echo "  Total Requests:    $TOTAL_REQUESTS"
        echo "  Actual QPS:        $ACTUAL_QPS"
        echo ""
        echo -e "${WHITE}  HTTP Status Codes:${NC}"
        
        if [ "$CODE_200" -gt 0 ]; then
            echo -e "${GREEN}    ✓ 200 OK:        $CODE_200 ($PCT_200%)${NC}"
        fi
        if [ "$CODE_503" -gt 0 ]; then
            echo -e "${YELLOW}    ⚠ 503 Shed:      $CODE_503 ($PCT_503%)${NC}"
        fi
        
        echo ""
        echo -e "${WHITE}  Latency (ms):${NC}"
        echo "    Avg:    $AVG_LATENCY"
        echo "    P50:    $P50"
        echo "    P90:    $P90"
        echo "    P99:    $P99"
        echo "    Max:    $MAX_LATENCY"
        echo ""
        
        # Load shedding summary
        if [ "$CODE_503" -gt 0 ]; then
            echo -e "${YELLOW}  ============================================${NC}"
            echo -e "${YELLOW}  LOAD SHEDDING:  $PCT_503% of requests rejected${NC}"
            echo -e "${YELLOW}  ============================================${NC}"
        else
            echo -e "${GREEN}  ============================================${NC}"
            echo -e "${GREEN}  NO LOAD SHEDDING - All requests succeeded${NC}"
            echo -e "${GREEN}  ============================================${NC}"
        fi
        echo ""
    else
        echo -e "${RED}Could not parse Fortio output${NC}"
        cat "$RESULT_FILE"
    fi
else
    # Fallback: just show raw output if jq not available
    echo -e "${YELLOW}Note: Install 'jq' for pretty-printed results${NC}"
    echo ""
    cat "$RESULT_FILE"
fi
