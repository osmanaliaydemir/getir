# Contributing to Getir Clone API

First off, thank you for considering contributing to Getir Clone API! 🎉

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Standards](#coding-standards)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing Guidelines](#testing-guidelines)

---

## 📜 Code of Conduct

This project adheres to professional development standards. Be respectful, constructive, and collaborative.

---

## 🤝 How Can I Contribute?

### Reporting Bugs 🐛

**Before submitting a bug report:**
- Check existing issues
- Ensure you're using the latest version
- Collect relevant information (logs, environment)

**Bug Report Template:**
```markdown
## Description
Clear description of the bug

## Steps to Reproduce
1. Step one
2. Step two
3. See error

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Environment
- OS: Windows 11
- .NET Version: 9.0
- SQL Server: 2022
```

### Suggesting Features 💡

**Feature Request Template:**
```markdown
## Feature Description
Clear description of the feature

## Use Case
Why this feature is needed

## Proposed Implementation
How it could be implemented

## Alternatives Considered
Other approaches you've thought about
```

### Code Contributions 💻

We welcome:
- Bug fixes
- New features
- Performance improvements
- Documentation improvements
- Test coverage improvements

---

## 🛠️ Development Setup

### Quick Start (Docker)

```bash
# Clone repository
git clone https://github.com/osmanaliaydemir/GetirV2.git
cd GetirV2

# Start with Docker
docker-compose up -d

# Done! API running at http://localhost:7001
```

### Manual Setup

```bash
# Prerequisites
- .NET 9 SDK
- SQL Server 2014+
- Git

# Clone and restore
git clone https://github.com/osmanaliaydemir/GetirV2.git
cd GetirV2
dotnet restore

# Setup database
sqlcmd -S OAA\MSSQLSERVER2014 -E -i database/schema.sql
sqlcmd -S OAA\MSSQLSERVER2014 -E -i database/schema-extensions.sql

# Run
cd src/WebApi
dotnet run
```

---

## 📐 Coding Standards

### Architecture

**Follow Clean Architecture:**
```
WebApi → Application → Domain ← Infrastructure
```

**Never:**
- ❌ Application → Infrastructure dependency
- ❌ Domain → any layer dependency
- ❌ Business logic in WebApi

### SOLID Principles

**Single Responsibility:**
```csharp
// ✅ Good
public class OrderService
{
    public async Task<Result<Order>> CreateOrderAsync(...) { }
}

// ❌ Bad
public class OrderService
{
    public async Task<Order> CreateOrderAsync(...) { }
    public async Task SendEmailAsync(...) { } // Wrong responsibility!
}
```

**Dependency Inversion:**
```csharp
// ✅ Good - Depend on abstraction
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
}

// ❌ Bad - Depend on concrete
public class OrderService
{
    private readonly UnitOfWork _unitOfWork;
}
```

### Naming Conventions

```csharp
// Classes, Interfaces, Methods: PascalCase
public class OrderService { }
public interface IOrderService { }
public async Task CreateOrderAsync() { }

// Private fields: _camelCase
private readonly IUnitOfWork _unitOfWork;

// Parameters, local variables: camelCase
public void Method(string productName) 
{
    var totalAmount = 100;
}

// Constants: PascalCase
public const string DefaultCurrency = "TRY";
```

### Code Style

```csharp
// ✅ Good - Explicit and readable
if (user == null)
{
    return Result.Fail("User not found", "NOT_FOUND_USER");
}

// ❌ Bad - Implicit, unclear
if (user == null) return Result.Fail("Not found", "ERR");

// ✅ Good - Use expression bodies for simple properties
public string FullName => $"{FirstName} {LastName}";

// ✅ Good - Use var when type is obvious
var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);

// ❌ Bad - Unnecessary verbosity
User user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
```

---

## 📝 Commit Guidelines

### Commit Message Format

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding tests
- `chore`: Build process, dependencies

**Examples:**

```bash
# Feature
git commit -m "feat(cart): add merchant constraint validation"

# Bug fix
git commit -m "fix(auth): resolve token expiration issue"

# Documentation
git commit -m "docs(readme): update Docker setup instructions"

# Test
git commit -m "test(coupon): add discount calculation tests"

# Refactor
git commit -m "refactor(order): extract price calculation to separate method"
```

---

## 🔄 Pull Request Process

### 1. Fork & Clone

```bash
# Fork on GitHub
# Clone your fork
git clone https://github.com/YOUR_USERNAME/GetirV2.git
cd GetirV2
git remote add upstream https://github.com/osmanaliaydemir/GetirV2.git
```

### 2. Create Feature Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/bug-description
```

### 3. Make Changes

- Follow coding standards
- Write tests for new features
- Update documentation
- Ensure all tests pass

### 4. Commit

```bash
git add .
git commit -m "feat(module): description"
```

### 5. Push

```bash
git push origin feature/your-feature-name
```

### 6. Create Pull Request

**PR Template:**
```markdown
## Description
What does this PR do?

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist
- [ ] Tests pass locally
- [ ] New tests added (if applicable)
- [ ] Documentation updated
- [ ] No linting errors
- [ ] Follows coding standards

## Related Issues
Closes #123
```

### 7. Code Review

- Address reviewer comments
- Make requested changes
- Push updates to same branch

---

## 🧪 Testing Guidelines

### Writing Tests

**AAA Pattern:**
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Setup
    var service = CreateService();
    var request = new Request(...);

    // Act - Execute
    var result = await service.MethodAsync(request);

    // Assert - Verify
    result.Success.Should().BeTrue();
}
```

### Test Coverage

**Required:**
- All new services must have unit tests
- Critical business logic must be tested
- Error scenarios must be covered
- Target: 80%+ coverage

**Run Tests:**
```bash
dotnet test tests/Getir.UnitTests
```

### Test Naming

```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public async Task AddItemAsync_DifferentMerchant_ShouldFail()

[Fact]
public async Task ValidateCouponAsync_ExpiredCoupon_ShouldReturnInvalid()
```

---

## 🏗️ Adding New Features

### 1. New Entity

```csharp
// Domain/Entities/YourEntity.cs
namespace Getir.Domain.Entities;

public class YourEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    // ...
}
```

### 2. DTO

```csharp
// Application/DTO/YourDtos.cs
namespace Getir.Application.DTO;

public record CreateYourEntityRequest(
    string Name,
    ...);

public record YourEntityResponse(
    Guid Id,
    string Name,
    ...);
```

### 3. Validator

```csharp
// Application/Validators/YourValidators.cs
public class CreateYourEntityRequestValidator : AbstractValidator<CreateYourEntityRequest>
{
    public CreateYourEntityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
```

### 4. Service

```csharp
// Application/Services/YourModule/IYourService.cs
public interface IYourService
{
    Task<Result<YourEntityResponse>> CreateAsync(CreateYourEntityRequest request, CancellationToken ct = default);
}

// Application/Services/YourModule/YourService.cs
public class YourService : IYourService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public YourService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<YourEntityResponse>> CreateAsync(...)
    {
        // Implementation with Result pattern
    }
}
```

### 5. Endpoints

```csharp
// WebApi/Endpoints/YourEndpoints.cs
public static class YourEndpoints
{
    public static void MapYourEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/your-module")
            .WithTags("Your Module");

        group.MapPost("/", async (
            [FromBody] CreateYourEntityRequest request,
            [FromServices] IYourService service,
            CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return result.ToIResult();
        })
        .WithName("CreateYourEntity")
        .Produces<YourEntityResponse>(200);
    }
}
```

### 6. Register in Program.cs

```csharp
// Add service
builder.Services.AddScoped<IYourService, YourService>();

// Add validator
services.AddScoped<IValidator<CreateYourEntityRequest>, CreateYourEntityRequestValidator>();

// Map endpoints
app.MapYourEndpoints();
```

### 7. Write Tests

```csharp
// tests/Getir.UnitTests/Services/YourServiceTests.cs
public class YourServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidData_ShouldSucceed()
    {
        // Test implementation
    }
}
```

---

## ✅ PR Checklist

Before submitting, ensure:

- [ ] Code follows SOLID principles
- [ ] Clean Architecture layers respected
- [ ] All tests pass (`dotnet test`)
- [ ] New tests added for new features
- [ ] Documentation updated
- [ ] No compiler warnings
- [ ] Result pattern used for service returns
- [ ] Error codes follow convention (AUTH_*, NOT_FOUND_*, etc.)
- [ ] Commit messages follow convention
- [ ] Branch is up to date with main

---

## 🎯 Priority Areas for Contribution

### High Priority
- 🔴 Integration test fixes (DB provider conflict)
- 🔴 Additional unit tests (MerchantService, ProductService)
- 🔴 Redis caching implementation
- 🔴 Rate limiting

### Medium Priority
- 🟡 SignalR real-time notifications
- 🟡 Background jobs (Hangfire)
- 🟡 Email/SMS service
- 🟡 Admin panel endpoints

### Low Priority
- 🟢 Elasticsearch integration
- 🟢 CQRS pattern
- 🟢 Event sourcing
- 🟢 Microservices migration

---

## 📧 Contact

Questions? Reach out:
- **GitHub Issues:** For bugs and features
- **Discussions:** For general questions
- **Email:** osmanali.aydemir@example.com

---

## 🙏 Thank You!

Your contributions make this project better for everyone!

---

**Happy Coding! 🚀**
