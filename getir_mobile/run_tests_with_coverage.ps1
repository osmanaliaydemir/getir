# PowerShell script for running Flutter tests with coverage
# Usage: .\run_tests_with_coverage.ps1

Write-Host "🧪 Running Flutter Tests with Coverage..." -ForegroundColor Cyan
Write-Host "==========================================="

# Clean previous coverage data
Write-Host "📁 Cleaning previous coverage..." -ForegroundColor Yellow
if (Test-Path "coverage") {
    Remove-Item -Recurse -Force "coverage"
}

# Run tests with coverage
Write-Host "🚀 Running tests..." -ForegroundColor Green
flutter test --coverage

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Tests passed successfully!" -ForegroundColor Green
    
    # Check if genhtml is available
    $genhtml = Get-Command genhtml -ErrorAction SilentlyContinue
    
    if ($genhtml) {
        Write-Host "📊 Generating HTML coverage report..." -ForegroundColor Cyan
        genhtml coverage/lcov.info -o coverage/html
        Write-Host "✅ Coverage report generated: coverage/html/index.html" -ForegroundColor Green
        Write-Host ""
        Write-Host "To view coverage report:" -ForegroundColor Yellow
        Write-Host "  start coverage\html\index.html" -ForegroundColor White
    } else {
        Write-Host "⚠️  lcov not installed. Install Perl and lcov for HTML reports." -ForegroundColor Yellow
        Write-Host "  Chocolatey: choco install lcov" -ForegroundColor White
    }
    
    Write-Host ""
    Write-Host "📈 Coverage file generated: coverage/lcov.info" -ForegroundColor Cyan
} else {
    Write-Host "❌ Tests failed!" -ForegroundColor Red
    exit 1
}

