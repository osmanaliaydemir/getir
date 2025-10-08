# Git Hooks

## Pre-commit Hook

Automatically runs linting and formatting checks before each commit.

### Installation

#### Linux/Mac
```bash
# Make hook executable
chmod +x .githooks/pre-commit

# Configure git to use this hooks directory
git config core.hooksPath .githooks
```

#### Windows (PowerShell)
```powershell
# Configure git to use this hooks directory
git config core.hooksPath .githooks

# PowerShell script will run automatically
```

### What it checks

1. ✅ **Flutter Analyze** - No linter errors
2. ✅ **Dart Format** - Code style consistency
3. ⚠️ **Tests** (optional) - Can be enabled by uncommenting

### Bypass hook (not recommended)

If you need to bypass the pre-commit hook:

```bash
git commit --no-verify -m "your message"
```

⚠️ **Warning:** Only bypass for emergency hotfixes!

### Manual run

Test the hook manually:

```bash
# Linux/Mac
.githooks/pre-commit

# Windows
.githooks/pre-commit.ps1
```
