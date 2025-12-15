# Publishing Docker Images and Helm Charts

This document explains how the automated publishing process works for Weather.

## Workflow Overview

The `publish-docker-helm.yml` workflow automatically publishes:
- Docker images to GitHub Container Registry (ghcr.io)
- Helm charts to GitHub Container Registry (as OCI artifacts)

## Trigger

The workflow runs automatically when:
- A Pull Request is **merged** to the `main` branch
- Manually triggered via workflow_dispatch

## Prerequisites

### GitHub Secrets Required

1. **GITHUB_TOKEN** (Automatic)
   - Automatically provided by GitHub Actions
   - No setup required - has permissions to push to ghcr.io

### Repository Permissions

Ensure the workflow has appropriate permissions:
- `contents: write` - For creating releases and committing version bumps
- `packages: write` - For pushing to GitHub Container Registry
- `id-token: write` - For OIDC authentication

## What Gets Published

### Docker Images

Three tags are created for each build:

1. **latest** - Always points to the most recent build
   ```bash
   docker pull ghcr.io/busadave13/weather:latest
   ```

2. **Version tag** - Based on GitVersion semantic versioning (e.g., `1.0.0`)
   ```bash
   docker pull ghcr.io/busadave13/weather:1.0.0
   ```

3. **Commit SHA** - Specific git commit reference
   ```bash
   docker pull ghcr.io/busadave13/weather:abc1234
   ```

### Helm Charts

Published as OCI artifacts to GitHub Container Registry:

```bash
# Install from GitHub Container Registry
helm install weather oci://ghcr.io/busadave13/helm/weather --version 1.0.0

# Pull chart locally
helm pull oci://ghcr.io/busadave13/helm/weather --version 1.0.0
```

## Versioning

Version is automatically calculated using GitVersion based on:
- Git commit history
- Branch names
- Tags

The `.gitversion.yml` file configures the versioning strategy.

### Version Types

1. **PR Merged to main** - Clean semantic version (e.g., `1.0.0`)
2. **Manual trigger (prerelease: false)** - Clean semantic version
3. **Manual trigger (prerelease: true) from main** - Release candidate (e.g., `1.0.0-rc.42`)
4. **Manual trigger (prerelease: true) from feature branch** - Branch-specific (e.g., `1.0.0-feature-xyz.42`)

## Release Notes

The workflow automatically creates a GitHub Release with:
- Docker pull commands
- Helm install commands
- PR title in the release notes
- Version details

## Manual Trigger

You can manually trigger the workflow:

1. Go to: **Actions** → **Publish Docker Image and Helm Charts**
2. Click **Run workflow**
3. Select options:
   - **version_bump**: patch, minor, or major
   - **prerelease**: Create a prerelease version
4. Click **Run workflow**

## Workflow Steps

1. ✅ Checkout code with full git history
2. 🔧 Setup .NET 9.0
3. 📦 Restore dependencies
4. 🔨 Build the project
5. 🧪 Run tests
6. 🔢 Calculate version with GitVersion
7. 🐳 Build and push Docker image (3 tags)
8. 📦 Update and package Helm chart
9. 📤 Push Helm chart to ghcr.io (OCI)
10. 💾 Commit version bump to Chart.yaml
11. 🏷️ Create GitHub Release
12. 📋 Generate summary

## Using Published Artifacts

### Docker Image

```bash
# Pull and run latest
docker pull ghcr.io/busadave13/weather:latest
docker run -p 8080:8080 ghcr.io/busadave13/weather:latest

# Pull specific version
docker pull ghcr.io/busadave13/weather:1.0.0
```

### Helm Chart

```bash
# Login to GitHub Container Registry (if private)
echo $GITHUB_TOKEN | helm registry login ghcr.io -u USERNAME --password-stdin

# Install from OCI registry
helm install weather oci://ghcr.io/busadave13/helm/weather \
  --version 1.0.0 \
  --namespace dev \
  --create-namespace

# Upgrade existing installation
helm upgrade weather oci://ghcr.io/busadave13/helm/weather \
  --version 1.0.0 \
  --namespace dev

# View chart values
helm show values oci://ghcr.io/busadave13/helm/weather --version 1.0.0
```

## Troubleshooting

### Workflow Fails to Publish

1. **Check Permissions**
   - Ensure workflow has `packages: write` permission
   - Check repository settings for package access

2. **Version Already Exists**
   - GitVersion automatically increments versions
   - If tag already exists, release creation will be skipped (continue-on-error)

3. **Tests Failing**
   - Workflow stops if tests fail
   - Fix tests before merging PR

### Can't Pull Docker Image

```bash
# For public packages
docker pull ghcr.io/busadave13/weather:latest

# For private packages, authenticate first
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin
docker pull ghcr.io/busadave13/weather:latest
```

### Can't Pull Helm Chart

```bash
# Login to GitHub Container Registry
echo $GITHUB_TOKEN | helm registry login ghcr.io -u USERNAME --password-stdin

# Pull chart
helm pull oci://ghcr.io/busadave13/helm/weather --version 1.0.0
```

## Best Practices

1. **Use meaningful PR titles** - They appear in release notes
2. **Follow semantic versioning** - GitVersion handles this automatically
3. **Test locally** before creating PR
4. **Review workflow runs** in GitHub Actions tab
5. **Use prerelease flag** for testing builds from feature branches

## Additional Resources

- [GitHub Container Registry](https://ghcr.io/busadave13/weather)
- [Helm OCI Documentation](https://helm.sh/docs/topics/registries/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitVersion Documentation](https://gitversion.net/docs/)
