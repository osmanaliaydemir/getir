#!/bin/bash
# Run Flutter tests with coverage report
# Usage: ./run_tests_with_coverage.sh

echo "🧪 Running Flutter Tests with Coverage..."
echo "==========================================="

# Clean previous coverage data
echo "📁 Cleaning previous coverage..."
rm -rf coverage

# Run tests with coverage
echo "🚀 Running tests..."
flutter test --coverage

# Check if tests passed
if [ $? -eq 0 ]; then
  echo "✅ Tests passed successfully!"
  
  # Generate HTML coverage report (requires lcov)
  if command -v genhtml &> /dev/null; then
    echo "📊 Generating HTML coverage report..."
    genhtml coverage/lcov.info -o coverage/html
    echo "✅ Coverage report generated: coverage/html/index.html"
    echo ""
    echo "To view coverage report:"
    echo "  open coverage/html/index.html    (macOS)"
    echo "  xdg-open coverage/html/index.html (Linux)"
    echo "  start coverage/html/index.html    (Windows)"
  else
    echo "⚠️  lcov not installed. Install with:"
    echo "  brew install lcov (macOS)"
    echo "  sudo apt-get install lcov (Linux)"
  fi
  
  # Display coverage summary
  echo ""
  echo "📈 Coverage Summary:"
  echo "-------------------"
  if command -v lcov &> /dev/null; then
    lcov --summary coverage/lcov.info
  else
    echo "Coverage file: coverage/lcov.info"
  fi
else
  echo "❌ Tests failed!"
  exit 1
fi

