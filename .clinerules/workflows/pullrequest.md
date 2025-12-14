# Pull Request Creation Workflow

This workflow creates a pull request for the current feature branch to merge into main.

## Step 1: Pre-flight Checks

### 1.1 Check for uncommitted changes
First, I'll check if there are any uncommitted changes in the working directory.

```bash
git status --porcelain
```

If there are uncommitted changes, I'll ask if you want to:
- Commit them before proceeding
- Stash them temporarily
- Abort the workflow

### 1.2 Verify current branch
I'll verify we're on a feature branch and not on main.

```bash
git branch --show-current
```

If on main, I'll abort the workflow with a message to create/checkout a feature branch first.

### 1.3 Verify branch is up-to-date with main
I'll fetch the latest from origin and check if the current branch needs to be updated.

```bash
git fetch origin main
git log HEAD..origin/main --oneline
```

If the branch is behind main, I'll prompt you to rebase or merge before creating the PR.

### 1.4 Run unit tests
I'll run the unit tests to ensure everything passes before creating the PR.

```bash
dotnet test
```

If tests fail, I'll abort the workflow and show the failures.

## Step 2: Gather PR Information

### 2.1 Analyze changes
I'll analyze the commits on this branch compared to main to generate the PR content.

```bash
git log origin/main..HEAD --oneline
git diff origin/main --stat
```

### 2.2 Generate PR Title
I'll generate a title based on the branch name, converting formats like:
- `feature/add-weather-api` → "Add weather api"
- `fix/temperature-calculation` → "Fix temperature calculation"
- `bugfix/null-reference` → "Bugfix null reference"

You can customize the title if needed.

### 2.3 Generate PR Description
I'll create a description with the following sections:

**Template:**
```markdown
## Summary
[Brief description of what this PR accomplishes]

## Changes Made
[List of changes based on commit analysis]

## Testing
- Unit tests have been run and pass
- [Any additional testing notes]

## Checklist
- [ ] Unit tests pass
- [ ] Code has been self-reviewed
- [ ] Documentation updated (if applicable)
- [ ] No breaking changes introduced
```

## Step 3: Push and Create PR

### 3.1 Push branch to origin
If the branch hasn't been pushed or has new commits:

```bash
git push -u origin HEAD
```

### 3.2 Create Pull Request
Using the GitHub MCP tool, I'll create the pull request:

```xml
<use_mcp_tool>
  <server_name>github.com/github/github-mcp-server</server_name>
  <tool_name>create_pull_request</tool_name>
  <arguments>
    {
      "owner": "[repository-owner]",
      "repo": "[repository-name]",
      "title": "[generated-title]",
      "body": "[generated-description]",
      "head": "[current-branch]",
      "base": "main"
    }
  </arguments>
</use_mcp_tool>
```

## Step 4: Post-PR Actions

After the PR is created, I'll:
1. Display the PR URL for easy access
2. Show a summary of the PR details
3. Ask if you want to request any reviewers

---

## Usage

To invoke this workflow, type in the chat:
```
/pullrequest.md
```

## Notes

- This workflow assumes a .NET project with `dotnet test` available
- The base branch is always `main`
- Repository owner and name are detected from the git remote configuration
