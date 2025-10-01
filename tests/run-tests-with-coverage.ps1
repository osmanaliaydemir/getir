# Test Coverage Script for Getir API

Write-Host "🧪 Running tests with coverage..." -ForegroundColor Cyan

# Install reportgenerator if not exists
dotnet tool install --global dotnet-reportgenerator-globaltool 2>$null

# Clean previous coverage
Remove-Item -Path "TestResults" -Recurse -Force -ErrorAction SilentlyContinue

# Run tests with coverage
dotnet test ../Getir.sln `
    --collect:"XPlat Code Coverage" `
    --results-directory:"TestResults" `
    --verbosity:normal `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura

# Generate HTML report
$coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse

if ($coverageFiles) {
    $coverageFile = $coverageFiles[0].FullName
    
    Write-Host "`n📊 Generating coverage report..." -ForegroundColor Cyan
    
    reportgenerator `
        -reports:"$coverageFile" `
        -targetdir:"TestResults/CoverageReport" `
        -reporttypes:"Html;TextSummary"
    
    Write-Host "`n✅ Coverage report generated!" -ForegroundColor Green
    Write-Host "📁 Location: TestResults/CoverageReport/index.html" -ForegroundColor Yellow
    
    # Display summary
    if (Test-Path "TestResults/CoverageReport/Summary.txt") {
        Write-Host "`n📈 Coverage Summary:" -ForegroundColor Cyan
        Get-Content "TestResults/CoverageReport/Summary.txt"
    }
    
    # Open report in browser
    $reportPath = Resolve-Path "TestResults/CoverageReport/index.html"
    Write-Host "`n🌐 Opening coverage report in browser..." -ForegroundColor Cyan
    Start-Process $reportPath
}
else {
    Write-Host "`n⚠️ No coverage file found!" -ForegroundColor Yellow
}
