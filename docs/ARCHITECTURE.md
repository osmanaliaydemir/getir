# 🏗️ Architecture Documentation

## 📐 Clean Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                        WebApi Layer                          │
│  ┌────────────┐  ┌────────────┐  ┌──────────────────────┐  │
│  │ Endpoints  │  │Middleware │  │  Configuration       │  │
│  │  - Auth    │  │ - Error   │  │  - Swagger           │  │
│  │  - Cart    │  │ - Request │  │  - JWT               │  │
│  │  - Orders  │  │   ID      │  │  - Health Checks     │  │
│  └────────────┘  └────────────┘  └──────────────────────┘  │
└────────────────────────┬────────────────────────────────────┘
                         │ Uses Services
┌────────────────────────▼────────────────────────────────────┐
│                    Application Layer                         │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │  Services   │  │  DTOs        │  │  Validators      │   │
│  │ - AuthSvc   │  │ - Requests   │  │ - FluentValid.   │   │
│  │ - CartSvc   │  │ - Responses  │  │ - Rules          │   │
│  │ - OrderSvc  │  │ - Pagination │  │                  │   │
│  └─────────────┘  └──────────────┘  └──────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Common (Result, PagedResult, Extensions)           │   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Abstractions (IUnitOfWork, IRepository<T>)         │   │
│  └──────────────────────────────────────────────────────┘   │
└────────────────────────┬────────────────────────────────────┘
                         │ Uses Entities
┌────────────────────────▼────────────────────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Entities (POCOs)                                    │   │
│  │  - User, Order, Product, Merchant, Category, etc.   │   │
│  │  - No dependencies                                   │   │
│  │  - Pure business objects                             │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘
                         ▲ Implements
┌────────────────────────┴────────────────────────────────────┐
│                  Infrastructure Layer                        │
│  ┌─────────────────┐  ┌──────────────────────────────────┐ │
│  │  Persistence    │  │  Security                        │ │
│  │  - DbContext    │  │  - JwtTokenService               │ │
│  │  - Repositories │  │  - PasswordHasher                │ │
│  │  - UnitOfWork   │  │                                  │ │
│  └─────────────────┘  └──────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
```

---

## 🔄 Data Flow

### Request Flow (Example: Create Order)

```
1. HTTP Request
   POST /api/v1/orders
   Body: { merchantId, items, address, payment }
   
2. Endpoint (WebApi)
   ↓ Extract userId from JWT claims
   ↓ Validate request (FluentValidation)
   
3. Service (Application)
   ↓ Business logic
   ↓ Begin transaction
   ↓ Validate merchant
   ↓ Validate products & stock
   ↓ Calculate totals
   ↓ Create order entity
   ↓ Update stock
   ↓ Commit transaction
   
4. Repository (Infrastructure)
   ↓ EF Core operations
   ↓ Save to database
   
5. Response
   ← Return Result<OrderResponse>
   ← Convert to IResult
   ← HTTP 200 + JSON
```

---

## 🎯 Design Patterns Used

### 1. Repository Pattern
```csharp
public interface IGenericRepository<T>
{
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    // ...
}
```

**Benefits:**
- Abstraction over data access
- Testability (easy mocking)
- Centralized query logic

### 2. Unit of Work Pattern
```csharp
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>();
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
```

**Benefits:**
- Transaction management
- Single save point
- Repository caching

### 3. Result Pattern
```csharp
public class Result<T>
{
    public bool Success { get; set; }
    public T? Value { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
}
```

**Benefits:**
- No exceptions for business errors
- Standardized responses
- Error code tracking

### 4. Dependency Injection
```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

**Benefits:**
- Loose coupling
- Testability
- Lifetime management

---

## 🔐 Security Architecture

### Authentication Flow

```
1. User → POST /auth/register
2. API → Hash password (PBKDF2, 100k iterations)
3. API → Create user in DB
4. API → Generate Access Token (JWT, 60 min)
5. API → Generate Refresh Token (random, 7 days)
6. API → Store Refresh Token in DB
7. API → Return { accessToken, refreshToken }

Protected Request:
1. User → GET /orders (Header: Authorization: Bearer {token})
2. API → Validate JWT signature
3. API → Check expiration
4. API → Extract userId from claims
5. API → Process request with userId
```

### Password Security

```csharp
// Hashing
Salt (16 bytes random) + Password → PBKDF2 (100k iterations, SHA256) → Hash

// Storage
Base64(Salt + Hash) → Database

// Verification
Stored Hash → Extract Salt → Hash(Password + Salt) → Compare
```

---

## 📊 Database Schema Design

### Core Tables

```
Users ────────┬───── RefreshTokens
              ├───── Orders ────┬── OrderLines
              ├───── UserAddresses     │
              └───── CartItems         │
                                      │
Merchants ────┬───── Products ────────┘
              └───── Orders

Categories ───┬───── Merchants
              └───── Products

Coupons ──────┬───── CouponUsages
              └───── Orders (FK)

Campaigns ────────── Merchants (optional FK)
Notifications ────── Users
Couriers ─────────── Orders (optional FK)
```

### Indexing Strategy

**Primary Indexes:**
- All Id columns (clustered)
- Unique constraints (Email, OrderNumber, Token, Code)

**Search Indexes:**
- `Users.Email` (unique, for login)
- `Orders.OrderNumber` (unique, for tracking)
- `Orders.UserId, Status, CreatedAt` (composite, for user orders)
- `Products.MerchantId, IsActive` (composite, for listing)
- `Merchants.CategoryId, IsActive, Rating` (composite, for search)

---

## ⚡ Performance Considerations

### Query Optimization

```csharp
// ✅ Good - AsNoTracking for read-only
var products = await _unitOfWork.ReadRepository<Product>()
    .ListAsync(...);  // Implements AsNoTracking

// ✅ Good - Include for eager loading
var order = await _unitOfWork.Repository<Order>()
    .GetAsync(o => o.Id == id, include: "Merchant,OrderLines");

// ❌ Bad - N+1 query problem
foreach (var order in orders)
{
    var merchant = await GetMerchantAsync(order.MerchantId); // N queries!
}
```

### Pagination

```csharp
// Always paginate lists
public async Task<Result<PagedResult<T>>> GetListAsync(PaginationQuery query)
{
    var items = await repository.GetPagedAsync(
        page: query.Page,
        pageSize: query.PageSize  // Default: 20
    );
}
```

---

## 🧪 Testing Architecture

### Test Pyramid

```
        ╱╲
       ╱  ╲        E2E Tests (Future)
      ╱────╲       
     ╱      ╲      Integration Tests (Planned)
    ╱────────╲     
   ╱          ╲    Unit Tests (22 tests ✅)
  ╱────────────╲   
 ╱______________╲  
```

### Mocking Strategy

```csharp
// Mock repository
var mockRepo = new Mock<IGenericRepository<User>>();
mockRepo.Setup(r => r.GetByIdAsync(userId, ct))
    .ReturnsAsync(user);

// Mock UnitOfWork
var mockUoW = new Mock<IUnitOfWork>();
mockUoW.Setup(u => u.Repository<User>()).Returns(mockRepo.Object);
```

---

## 🔄 Transaction Management

### Order Creation Flow

```
BEGIN TRANSACTION
│
├─ Validate Merchant (exists, active)
│
├─ For each product:
│  ├─ Validate product (exists, active)
│  ├─ Check stock (quantity >= requested)
│  ├─ Calculate price (discounted or regular)
│  └─ Create OrderLine
│
├─ Check minimum order amount
│
├─ Create Order
├─ Create OrderLines
├─ Update Product stocks (decrease)
│
├─ SaveChanges()
│
COMMIT TRANSACTION
│
└─ Return Result<OrderResponse>

On Error → ROLLBACK TRANSACTION
```

---

## 📚 Module Dependencies

```
WebApi
  ↓ depends on
Application ←──────┐
  ↓ depends on     │ implements abstractions
Domain             │
                   │
Infrastructure ────┘
  ↓ depends on
Domain
```

**Rules:**
- ✅ Application defines interfaces (IUnitOfWork, IRepository)
- ✅ Infrastructure implements interfaces
- ✅ WebApi uses Application services
- ❌ Application never depends on Infrastructure directly

---

## 🎯 Error Handling Strategy

### Error Code Convention

```
AUTH_*          → 401 Unauthorized
NOT_FOUND_*     → 404 Not Found
VALIDATION_*    → 400 Bad Request
FORBIDDEN_*     → 403 Forbidden
CONFLICT_*      → 409 Conflict
*               → 500 Internal Server Error
```

### Middleware Pipeline

```
Request
  ↓
RequestIdMiddleware (Generate X-Request-Id)
  ↓
ErrorHandlingMiddleware (Catch exceptions)
  ↓
SerilogRequestLogging
  ↓
Authentication
  ↓
Authorization
  ↓
Endpoints
  ↓
Response
```

---

## 🚀 Deployment Architecture

### Docker Setup

```
┌─────────────────────────────────────┐
│      docker-compose.yml             │
├─────────────────────────────────────┤
│                                     │
│  ┌──────────────┐  ┌────────────┐ │
│  │  SQL Server  │  │   Getir    │ │
│  │  Container   │  │    API     │ │
│  │              │  │ Container  │ │
│  │  Port: 1433  │  │ Port: 7001 │ │
│  └──────┬───────┘  └─────┬──────┘ │
│         │                 │         │
│         └────── Network ──┘         │
│                                     │
│  ┌──────────────────────────────┐  │
│  │  Volumes (Data Persistence)  │  │
│  │  - sqlserver-data            │  │
│  └──────────────────────────────┘  │
└─────────────────────────────────────┘
```

---

## 📊 Request/Response Flow Diagram

```
┌──────────┐                                    ┌──────────┐
│  Client  │                                    │ Database │
└─────┬────┘                                    └────▲─────┘
      │                                              │
      │ 1. POST /api/v1/orders                       │
      │    Authorization: Bearer {token}             │
      ▼                                              │
┌──────────────┐                                     │
│ Middleware   │                                     │
│ - RequestId  │ Generate X-Request-Id               │
│ - ErrorHandl │                                     │
└──────┬───────┘                                     │
       │                                             │
       │ 2. Validate JWT                             │
       ▼                                             │
┌──────────────┐                                     │
│ Endpoint     │                                     │
│ OrderEndpoint│                                     │
└──────┬───────┘                                     │
       │                                             │
       │ 3. Call Service                             │
       ▼                                             │
┌──────────────┐                                     │
│ Service      │                                     │
│ OrderService │ 4. Business Logic                   │
│              │    - Validate                       │
│              │    - Calculate                      │
└──────┬───────┘    - Transform                     │
       │                                             │
       │ 5. Repository Operations                    │
       ▼                                             │
┌──────────────┐                                     │
│ Repository   │ 6. BEGIN TRANSACTION                │
│ UnitOfWork   ├─────────────────────────────────────┤
│              │ 7. INSERT/UPDATE queries            │
│              │ 8. COMMIT TRANSACTION               │
└──────┬───────┘                                     │
       │                                             │
       │ 9. Return Result<T>                         │
       ▼                                             │
┌──────────────┐                                     │
│ Endpoint     │ 10. result.ToIResult()              │
└──────┬───────┘                                     │
       │                                             │
       │ 11. HTTP 200 + JSON                         │
       ▼                                             │
┌──────────────┐                                     │
│   Client     │                                     │
└──────────────┘                                     │
```

---

## 🎯 Key Architectural Decisions

### 1. Minimal API vs Controllers
**Decision:** Minimal API  
**Reason:**
- Performance (+30% faster)
- Less boilerplate
- Modern .NET 9 approach
- Easier to maintain

### 2. DB-First vs Code-First
**Decision:** Code-First with manual entities  
**Reason:**
- Better control over migrations
- Easier testing (InMemory DB)
- Version control friendly

### 3. Result Pattern vs Exceptions
**Decision:** Result Pattern for business errors  
**Reason:**
- Explicit error handling
- No performance overhead
- Better API contracts
- Error code standardization

### 4. Generic Repository vs Direct DbContext
**Decision:** Generic Repository + UnitOfWork  
**Reason:**
- Testability (easy mocking)
- Consistent API
- Transaction management
- Repository caching

---

## 📦 Module Boundaries

### Auth Module
```
Responsibilities:
- User registration
- Login/logout
- Token management
- Password hashing

Dependencies:
- IUnitOfWork
- IJwtTokenService
- IPasswordHasher
```

### Cart Module
```
Responsibilities:
- Add/update/remove items
- Merchant constraint enforcement
- Stock validation
- Cart clearing

Dependencies:
- IUnitOfWork

Business Rules:
- Single merchant per cart
- Stock availability check
- Auto price calculation
```

### Order Module
```
Responsibilities:
- Order creation (with transaction)
- Order retrieval
- Stock management
- Price calculation

Dependencies:
- IUnitOfWork

Business Rules:
- Minimum order amount
- Stock decrease
- Transaction rollback on error
```

---

## 🔒 Security Layers

```
Layer 1: HTTPS (Transport Security)
   ↓
Layer 2: JWT Validation (Authentication)
   ↓
Layer 3: Authorization (User/Role checks)
   ↓
Layer 4: Input Validation (FluentValidation)
   ↓
Layer 5: Business Rules (Service Layer)
   ↓
Layer 6: SQL Injection Prevention (EF Core)
```

---

## 📈 Scalability Strategy

### Current (Single Instance)
```
Client → API → Database
```

### Future (Scaled)
```
Client → Load Balancer → API (3 instances)
                            ↓
                         Redis Cache
                            ↓
                         Database (Read Replicas)
```

### Docker Scaling
```bash
# Scale to 5 instances
docker-compose up -d --scale api=5
```

---

## 🎉 Summary

**Architecture Principles:**
- ✅ Clean Architecture
- ✅ SOLID principles
- ✅ DDD (Domain-Driven Design)
- ✅ Separation of Concerns
- ✅ Dependency Inversion
- ✅ Single Responsibility

**Quality Attributes:**
- ⭐ Maintainability: High
- ⭐ Testability: High
- ⭐ Scalability: Medium-High
- ⭐ Performance: Good
- ⭐ Security: High

---

**Architecture is solid and production-ready! 🚀**
