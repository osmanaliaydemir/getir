# 🧪 Test Suite - Getir Clone API

## 📊 Test Coverage Overview

### Test Projects

| Project | Type | Test Count | Coverage Target |
|---------|------|------------|-----------------|
| **Getir.UnitTests** | Unit Tests | 20+ | 80%+ |
| **Getir.IntegrationTests** | Integration Tests | 10+ | API Coverage |

---

## 🚀 Running Tests

### Quick Run (All Tests)

```bash
# Windows
dotnet test

# Linux/macOS
dotnet test
```

### Run with Coverage

**Windows (PowerShell):**
```powershell
cd tests
.\run-tests-with-coverage.ps1
```

**Linux/macOS:**
```bash
cd tests
chmod +x run-tests.sh
./run-tests.sh
```

### Run Specific Test Project

```bash
# Unit tests only
dotnet test tests/Getir.UnitTests

# Integration tests only
dotnet test tests/Getir.IntegrationTests
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
dotnet test --filter "FullyQualifiedName~CartServiceTests"
```

### Run Single Test

```bash
dotnet test --filter "FullyQualifiedName~RegisterAsync_WithNewEmail_ShouldSucceed"
```

---

## 📋 Test Categories

### Unit Tests (Getir.UnitTests)

#### ✅ AuthService Tests (5 tests)
```
✓ Register with new email → Success
✓ Register with existing email → AUTH_EMAIL_EXISTS
✓ Login with valid credentials → Success + Token
✓ Login with invalid password → AUTH_INVALID_CREDENTIALS
✓ Login with inactive account → AUTH_ACCOUNT_DEACTIVATED
```

#### ✅ CartService Tests (4 tests)
```
✓ Add item to empty cart → Success
✓ Add item from different merchant → CART_DIFFERENT_MERCHANT
✓ Add item with insufficient stock → INSUFFICIENT_STOCK
✓ Clear cart with items → All items removed
```

#### ✅ CouponService Tests (6 tests)
```
✓ Validate percentage coupon → Correct discount calculation
✓ Validate with max discount cap → Capped at maximum
✓ Validate fixed amount coupon → Fixed discount
✓ Validate below minimum amount → isValid: false
✓ Validate expired coupon → isValid: false
✓ Validate usage limit reached → isValid: false
✓ Create coupon with unique code → Success
✓ Create coupon with duplicate code → CONFLICT_COUPON_CODE
```

#### ✅ OrderService Tests (5 tests)
```
✓ Create order with valid data → Success + Transaction commit
✓ Create order below minimum amount → BELOW_MINIMUM_ORDER + Rollback
✓ Create order with insufficient stock → INSUFFICIENT_STOCK + Rollback
✓ Create order with multiple products → Correct total calculation
✓ Create order with inactive merchant → NOT_FOUND_MERCHANT
```

### Integration Tests (Getir.IntegrationTests)

#### ✅ AuthEndpoints Tests (5 tests)
```
✓ Register with valid data → 200 + Token
✓ Register with duplicate email → 409
✓ Login with valid credentials → 200 + Token
✓ Login with invalid password → 401
✓ Logout with valid token → 200
```

#### ✅ CategoryEndpoints Tests (3 tests)
```
✓ Get categories → 200 + Paged result
✓ Create category with auth → 200 + Created
✓ Create category without auth → 401
```

---

## 📈 Coverage Report

### Generate Coverage Report

```bash
# Windows
cd tests
.\run-tests-with-coverage.ps1

# Linux/macOS
cd tests
./run-tests.sh
```

Report location: `tests/TestResults/CoverageReport/index.html`

### Coverage Metrics

**Current Target:**
- **Line Coverage:** 80%+
- **Branch Coverage:** 70%+
- **Method Coverage:** 85%+

**Uncovered Areas (Acceptable):**
- Program.cs (startup code)
- Middleware exception paths
- DTO/Entity properties

---

## 🎯 Test Patterns Used

### AAA Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Setup test data and mocks
    var service = new Service(...);
    
    // Act - Execute the method
    var result = await service.MethodAsync(request);
    
    // Assert - Verify results
    result.Success.Should().BeTrue();
}
```

### Mocking with Moq

```csharp
var mockRepo = new Mock<IRepository<Entity>>();
mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(entity);
```

### Fluent Assertions

```csharp
result.Success.Should().BeTrue();
result.ErrorCode.Should().Be("EXPECTED_CODE");
result.Value.Should().NotBeNull();
```

### Test Data with Bogus

```csharp
var user = TestDataGenerator.CreateUser();
var product = TestDataGenerator.CreateProduct(merchantId, stockQuantity: 100);
```

---

## 🔍 What's Tested

### Business Logic Coverage

✅ **Authentication**
- Password hashing
- Token generation
- Refresh token rotation
- Account activation check

✅ **Shopping Cart**
- Merchant constraint (single merchant rule)
- Stock validation
- Quantity updates
- Cart clearing

✅ **Coupons**
- Percentage discount calculation
- Fixed amount discount
- Maximum discount cap
- Minimum order amount
- Date range validation
- Usage limit tracking

✅ **Orders**
- Transaction management
- Stock updates
- Minimum order amount
- Multi-product totals
- Merchant validation

✅ **Integration**
- API endpoint responses
- Authentication flow
- Error handling
- Status codes

---

## 🛠️ Test Tools

| Tool | Purpose | Version |
|------|---------|---------|
| **xUnit** | Test framework | 2.9.2 |
| **Moq** | Mocking framework | 4.20.72 |
| **FluentAssertions** | Readable assertions | 6.12.1 |
| **Bogus** | Test data generation | 35.6.1 |
| **Coverlet** | Code coverage | 6.0.2 |
| **WebApplicationFactory** | Integration testing | 9.0.0 |

---

## 📝 Adding New Tests

### Unit Test Template

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

public class YourServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly YourService _service;

    public YourServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new YourService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var request = new Request(...);
        
        // Mock setup
        var mockRepo = new Mock<IRepository<Entity>>();
        _unitOfWorkMock.Setup(u => u.Repository<Entity>()).Returns(mockRepo.Object);

        // Act
        var result = await _service.MethodAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        mockRepo.Verify(r => r.SomeMethod(...), Times.Once);
    }
}
```

### Integration Test Template

```csharp
using FluentAssertions;
using Getir.IntegrationTests.Setup;
using System.Net.Http.Json;
using Xunit;

namespace Getir.IntegrationTests.Api;

public class YourEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public YourEndpointsTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Endpoint_Scenario_ShouldReturnExpectedStatus()
    {
        // Arrange
        var request = new RequestDto(...);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/endpoint", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 🎯 Test Best Practices

### ✅ DO
- Follow AAA pattern (Arrange-Act-Assert)
- Test one thing per test
- Use descriptive test names
- Mock external dependencies
- Test both success and failure paths
- Verify method calls (Times.Once, Times.Never)
- Test business rules explicitly

### ❌ DON'T
- Test framework code (EF Core, ASP.NET)
- Test DTOs/POCOs (no logic)
- Create flaky tests
- Depend on test execution order
- Use real database in unit tests
- Test multiple scenarios in one test

---

## 🐛 Debugging Tests

### Visual Studio
1. Set breakpoint in test
2. Right-click test → Debug Test(s)
3. Step through code

### VS Code
1. Install .NET Test Explorer extension
2. Click debug icon on test
3. Use debugger

### Command Line
```bash
# Verbose output
dotnet test --verbosity detailed

# Debug specific test
dotnet test --filter "FullyQualifiedName~YourTestName" --logger "console;verbosity=detailed"
```

---

## 📊 Coverage Thresholds

### Current Goals

```xml
<PropertyGroup>
  <CoverageThreshold>80</CoverageThreshold>
  <BranchCoverageThreshold>70</BranchCoverageThreshold>
</PropertyGroup>
```

### Exemptions
- Program.cs (startup code)
- Middleware constructors
- Entity/DTO properties
- Migration files

---

## 🎉 CI/CD Integration

Tests run automatically on:
- ✅ Push to main/develop
- ✅ Pull requests
- ✅ Manual workflow dispatch

**Pipeline Steps:**
1. Restore dependencies
2. Build solution
3. Run tests with coverage
4. Upload coverage report
5. Fail if coverage < threshold

---

**Happy Testing! 🚀**
