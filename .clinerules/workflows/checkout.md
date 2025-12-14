# Branch Checkout Workflow

This workflow creates a new feature branch from the latest main branch, ready for development.

## Step 1: Pre-flight Checks

### 1.1 Check for uncommitted changes
First, I'll check if there are any uncommitted changes in the working directory.

```bash
git status --porcelain
```

If there are uncommitted changes, I'll ask if you want to:
- Stash them temporarily (`git stash`)
- Commit them to the current branch
- Abort the workflow

### 1.2 Display current branch
I'll show the current branch for context.

```bash
git branch --show-current
```

### 1.3 Fetch latest from origin
I'll fetch the latest changes from origin to ensure we branch from the most recent main.

```bash
git fetch origin main
```

## Step 2: Create New Feature Branch

### 2.1 Select branch type
I'll ask you to select the type of branch you're creating:

| Type | Prefix | Use Case |
|------|--------|----------|
| Feature | `feature/` | New functionality or enhancement |
| Fix | `fix/` | Bug fix |
| Bugfix | `bugfix/` | Bug fix (alternative) |
| Chore | `chore/` | Maintenance, refactoring, dependencies |

### 2.2 Enter branch name
I'll ask for a descriptive name for your branch. The name will be automatically formatted:
- Converted to lowercase
- Spaces replaced with hyphens
- Special characters removed

**Examples:**
- "Add Weather API" → `feature/add-weather-api`
- "Fix temperature calculation" → `fix/fix-temperature-calculation`
- "Update dependencies" → `chore/update-dependencies`

### 2.3 Create and checkout branch
I'll create the new branch from the latest origin/main:

```bash
git checkout -b users/davhar/<type>/<formatted-name> origin/main
```

## Step 3: Ready for Development

### 3.1 Confirm branch creation
I'll verify the new branch is active:

```bash
git branch --show-current
```

### 3.2 Display status
I'll show you're ready to start development with a clean working directory.

**Note:** The branch will remain local until you're ready to create a PR. Use `/pullrequest.md` when you're ready to push and create a pull request.

---

## Usage

To invoke this workflow, type in the chat:
```
/checkout.md
```

## Branch Naming Convention

This workflow enforces the following branch naming convention:

- `users/davhar/feature/<name>` - New features or enhancements
- `users/davhar/fix/<name>` - Bug fixes
- `users/davhar/bugfix/<name>` - Bug fixes (alternative prefix)
- `users/davhar/chore/<name>` - Maintenance tasks, refactoring, dependency updates

All branch names are lowercase with hyphens separating words.
