# Weather
Test service for mockery

## Cline Workflows

This project includes custom Cline workflows to automate common development tasks.

### Branch Checkout Workflow

Create a new feature branch from the latest main branch, ready for development.

**Usage:** Type `/checkout.md` in the Cline chat.

**What it does:**

1. **Pre-flight Checks**
   - Verifies no uncommitted changes (prompts to stash/commit if found)
   - Displays current branch for context
   - Fetches latest from origin main

2. **Branch Creation**
   - Prompts for branch type (`feature/`, `fix/`, `bugfix/`, `chore/`)
   - Prompts for descriptive branch name
   - Auto-formats name (lowercase, hyphens)
   - Creates branch from latest origin/main

3. **Ready for Development**
   - Confirms new branch is active
   - Branch remains local until PR workflow is used

**Branch Naming Convention:**
- `users/davhar/feature/<name>` - New features or enhancements
- `users/davhar/fix/<name>` - Bug fixes
- `users/davhar/bugfix/<name>` - Bug fixes (alternative)
- `users/davhar/chore/<name>` - Maintenance tasks

---

### Pull Request Workflow

Create a pull request for the current feature branch with automated checks and PR generation.

**Usage:** Type `/pullrequest.md` in the Cline chat.

**What it does:**

1. **Pre-flight Checks**
   - Verifies no uncommitted changes (prompts to commit/stash if found)
   - Confirms you're on a feature branch (not main)
   - Checks if branch is up-to-date with main (prompts to rebase if behind)
   - Runs unit tests (`dotnet test`) and aborts if tests fail

2. **PR Generation**
   - Auto-generates PR title from branch name
   - Creates description with Summary, Changes, Testing sections
   - Includes standard checklist items

3. **PR Creation**
   - Pushes branch to origin
   - Creates PR via GitHub API targeting `main` branch
   - Displays PR URL for easy access

**Standard Checklist Items:**
- [ ] Unit tests pass
- [ ] Code has been self-reviewed
- [ ] Documentation updated (if applicable)
- [ ] No breaking changes introduced
