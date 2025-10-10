# Pre-commit hook for Flutter project (PowerShell)
# Runs linting and formatting checks before commit

Write-Host "🔍 Running pre-commit checks..." -ForegroundColor Cyan

# Change to project root
$projectRoot = git rev-parse --show-toplevel
Set-Location "$projectRoot\getir_mobile"

# 1. Run Flutter Analyze
Write-Host "📊 Running Flutter analyze..." -ForegroundColor Cyan
flutter analyze --no-pub
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Flutter analyze failed. Please fix the issues before committing." -ForegroundColor Red
    exit 1
}
Write-Host "✅ Flutter analyze passed" -ForegroundColor Green

# 2. Run Dart Format Check
Write-Host "🎨 Checking code formatting..." -ForegroundColor Cyan
dart format --set-exit-if-changed lib/ test/
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Code formatting issues found. Auto-formatting..." -ForegroundColor Yellow
    dart format lib/ test/
    Write-Host "✅ Code formatted. Please review and re-commit." -ForegroundColor Green
    exit 1
}
Write-Host "✅ Code formatting check passed" -ForegroundColor Green

# 3. Run Tests (optional - can be disabled for faster commits)
# Uncomment to enable test running on commit
# Write-Host "🧪 Running tests..." -ForegroundColor Cyan
# flutter test --no-pub
# if ($LASTEXITCODE -ne 0) {
#     Write-Host "❌ Tests failed. Please fix before committing." -ForegroundColor Red
#     exit 1
# }
# Write-Host "✅ Tests passed" -ForegroundColor Green

Write-Host "✅ All pre-commit checks passed!" -ForegroundColor Green
Write-Host "🚀 Proceeding with commit..." -ForegroundColor Cyan

exit 0
