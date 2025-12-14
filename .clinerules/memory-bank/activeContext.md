# Active Context: Weather

## Current Session Focus
Setting up Cline workflow automation for the Weather project.

## Recent Changes

### Session: 2024-12-14
1. **Created PR workflow** (`.clinerules/workflows/pullrequest.md`)
   - Pre-flight checks: uncommitted changes, branch status, unit tests
   - Auto-generates PR title and description
   - Creates PR via GitHub MCP

2. **Created checkout workflow** (`.clinerules/workflows/checkout.md`)
   - Pre-flight checks: uncommitted changes, fetch latest
   - Branch type selection (feature, fix, bugfix, chore)
   - Auto-formats branch names

3. **Updated README.md**
   - Documented both workflows with usage instructions

4. **Initialized memory-bank**
   - Created project documentation structure

## Active Decisions
- Branch naming convention: `users/davhar/<type>/<name>` (lowercase, hyphens)
- Base branch: `main`
- Tests run before PR creation: `dotnet test`
- Branch stays local until PR workflow is used

## Current State
- Project is a skeleton with workflows defined
- No application code implemented yet
- Ready for feature development

## Next Steps
1. Implement Weather service functionality
2. Add unit tests
3. Configure GitHub Actions CI/CD
4. Add Docker support

## Open Questions
- None currently

## Important Patterns to Remember
- Use `/checkout.md` to start new work
- Use `/pullrequest.md` to create PRs
- All branches should follow naming convention
- Tests must pass before PR creation
