#!/bin/bash

# Default values
RPS=10
DURATION="1m"
BASE_URL="http://weather.local.com"

# Script name for help message
SCRIPT_NAME=$(basename "$0")

# Function to display help
show_help() {
    cat << EOF
Weather API Load Test Runner
============================

Usage: $SCRIPT_NAME [OPTIONS]

Description:
    Runs k6 load tests against the Weather API using load-test.js.
    All parameters are optional and have sensible defaults.

Options:
    -r, --rps <number>        Requests per second (default: $RPS)
    -d, --duration <duration> Test duration (default: $DURATION)
                              Examples: 30s, 1m, 5m, 1h
    -u, --url <url>           Base URL of the Weather API (default: $BASE_URL)
    -h, --help                Display this help message and exit

Examples:
    # Run with default settings
    ./$SCRIPT_NAME

    # Run with 50 requests per second for 2 minutes
    ./$SCRIPT_NAME --rps 50 --duration 2m

    # Run against local development server
    ./$SCRIPT_NAME --url http://localhost:5081 --rps 20 --duration 30s

    # Combined example
    ./$SCRIPT_NAME -r 100 -d 5m -u http://weather.local.com

Prerequisites:
    - k6 must be installed (https://k6.io/docs/getting-started/installation/)
    - The Weather API service must be running and accessible

EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -r|--rps)
            RPS="$2"
            shift 2
            ;;
        -d|--duration)
            DURATION="$2"
            shift 2
            ;;
        -u|--url)
            BASE_URL="$2"
            shift 2
            ;;
        -h|--help)
            show_help
            exit 0
            ;;
        *)
            echo "Error: Unknown option: $1"
            echo "Use --help for usage information."
            exit 1
            ;;
    esac
done

# Validate RPS is a number
if ! [[ "$RPS" =~ ^[0-9]+$ ]]; then
    echo "Error: RPS must be a positive integer."
    exit 1
fi

# Display configuration
echo "=========================================="
echo "Weather API Load Test"
echo "=========================================="
echo "Configuration:"
echo "  Base URL:  $BASE_URL"
echo "  RPS:       $RPS"
echo "  Duration:  $DURATION"
echo "=========================================="
echo ""

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Run k6 with environment variables
k6 run \
    -e BASE_URL="$BASE_URL" \
    -e RPS="$RPS" \
    -e DURATION="$DURATION" \
    "$SCRIPT_DIR/load-test.js"
