# Product Context: Weather

## Purpose
This project exists as a test service for mockery - providing a sandbox environment for testing .NET development practices, CI/CD pipelines, and Cline workflow automation.

## Problem Being Solved
- Need for a lightweight test project to demonstrate .NET best practices
- Testing ground for GitHub Actions and automation workflows
- Reference implementation for Cline workflow integration

## How It Should Work
1. **Service Layer:** Weather-related API endpoints (to be implemented)
2. **Testing:** Comprehensive unit tests using xUnit, Moq, and FluentAssertions
3. **Deployment:** Containerized via Docker, infrastructure managed with Terraform
4. **Development:** Streamlined with Cline workflows for branch creation and PR management

## User Experience Goals
- Developers can quickly create feature branches using `/checkout.md`
- PRs are created with consistent format and automated checks via `/pullrequest.md`
- Code quality maintained through pre-commit test runs
- Consistent branch naming conventions enforced

## Target Users
- Developers learning .NET best practices
- Teams testing CI/CD pipeline configurations
- Users exploring Cline workflow automation
