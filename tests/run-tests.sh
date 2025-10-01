#!/bin/bash

# Test Coverage Script for Getir API (Linux/macOS)

echo "🧪 Running tests with coverage..."

# Install reportgenerator if not exists
dotnet tool install --global dotnet-reportgenerator-globaltool 2>/dev/null

# Clean previous coverage
rm -rf TestResults

# Run tests with coverage
dotnet test ../Getir.sln \
    --collect:"XPlat Code Coverage" \
    --results-directory:"TestResults" \
    --verbosity:normal \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura

# Generate HTML report
COVERAGE_FILE=$(find TestResults -name "coverage.cobertura.xml" | head -n 1)

if [ -f "$COVERAGE_FILE" ]; then
    echo ""
    echo "📊 Generating coverage report..."
    
    reportgenerator \
        -reports:"$COVERAGE_FILE" \
        -targetdir:"TestResults/CoverageReport" \
        -reporttypes:"Html;TextSummary"
    
    echo ""
    echo "✅ Coverage report generated!"
    echo "📁 Location: TestResults/CoverageReport/index.html"
    
    # Display summary
    if [ -f "TestResults/CoverageReport/Summary.txt" ]; then
        echo ""
        echo "📈 Coverage Summary:"
        cat TestResults/CoverageReport/Summary.txt
    fi
    
    # Open report (platform specific)
    if command -v xdg-open &> /dev/null; then
        xdg-open TestResults/CoverageReport/index.html
    elif command -v open &> /dev/null; then
        open TestResults/CoverageReport/index.html
    fi
else
    echo ""
    echo "⚠️ No coverage file found!"
fi
