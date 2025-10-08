# 🔧 Backend Integration Plan - getir_mobile

**Tarih:** 8 Ekim 2025  
**Konu:** Flutter app'in backend'e bağlanması için yapılması gerekenler

---

## 🤔 Şu Anki Durum - Sorun Ne?

### Mevcut Kod Yapısı

Flutter app'de tüm API çağrıları var ama **backend yok:**

```dart
// lib/core/constants/environment_config.dart
static const String apiBaseUrl = 'https://api.getir.com/v1';
static const String signalrHubUrl = 'https://api.getir.com/hubs';
```

**Sorun:** Bu URL'ler gerçek değil (placeholder). API'ye istek atıldığında **404 ya da connection error** dönecek.

### Kod Örneği - Nasıl Çalışıyor?

```dart
// AuthDataSource - API çağrısı yapıyor
Future<AuthResponse> login(LoginRequest request) async {
  final response = await _dio.post(
    '/api/v1/auth/login',  // ← Bu endpoint YOK
    data: request.toJson(),
  );
  return AuthResponse.fromJson(response.data);
}
```

**Sonuç:** Bu kod çalıştırıldığında **network error** alacak çünkü backend yok.

---

## 🎯 Backend Tarafında Yapılması Gerekenler

Sen .NET developer'sın, işte **senin yapman gereken .NET backend çalışmaları:**

### 1️⃣ Authentication API Endpoints (Kritik)

**Gerekli Endpoints:**

```csharp
// Controllers/AuthController.cs

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    // 1. Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Email + password validation
        // JWT token generation
        // Return: { accessToken, refreshToken, user }
    }

    // 2. Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // User creation
        // Email verification
        // JWT token generation
        // Return: { accessToken, refreshToken, user }
    }

    // 3. Logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // Invalidate refresh token
        // Clear user session
    }

    // 4. Refresh Token
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Validate refresh token
        // Generate new access token
        // Return: { accessToken, refreshToken }
    }

    // 5. Forgot Password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        // Generate reset token
        // Send email
        // Return: success message
    }

    // 6. Reset Password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // Validate reset token
        // Update password
        // Return: success message
    }

    // 7. Validate Token
    [HttpGet("validate-token")]
    [Authorize]
    public async Task<IActionResult> ValidateToken()
    {
        // Check if token is valid
        // Return: { valid: true/false }
    }
}
```

**Request/Response Models:**

```csharp
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string ProfileImageUrl { get; set; }
    public string Role { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

### 2️⃣ Cart API Endpoints

```csharp
// Controllers/CartController.cs

[ApiController]
[Route("api/v1/cart")]
[Authorize]
public class CartController : ControllerBase
{
    // 1. Get Cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Return user's cart with items
    }

    // 2. Add to Cart
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        // Add product to cart
        // Calculate totals
        // Return: CartItem
    }

    // 3. Update Cart Item
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateCartItem(string itemId, [FromBody] UpdateCartItemRequest request)
    {
        // Update quantity
        // Recalculate totals
        // Return: CartItem
    }

    // 4. Remove from Cart
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveFromCart(string itemId)
    {
        // Remove item
        // Recalculate totals
    }

    // 5. Clear Cart
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        // Remove all items
    }

    // 6. Apply Coupon
    [HttpPost("coupons/apply")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        // Validate coupon
        // Calculate discount
        // Return: Cart with discount
    }

    // 7. Remove Coupon
    [HttpDelete("coupons/remove")]
    public async Task<IActionResult> RemoveCoupon()
    {
        // Remove coupon
        // Recalculate totals
    }
}
```

---

### 3️⃣ Product API Endpoints

```csharp
// Controllers/ProductsController.cs

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    // 1. Get All Products (with pagination)
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? merchantId = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null)
    {
        // Return paginated products
    }

    // 2. Get Product by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        // Return product details
    }

    // 3. Get Products by Merchant
    [HttpGet("merchant/{merchantId}")]
    public async Task<IActionResult> GetProductsByMerchant(string merchantId)
    {
        // Return merchant's products
    }

    // 4. Search Products
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query)
    {
        // Full-text search
        // Return matching products
    }

    // 5. Get Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        // Return list of categories
    }

    // 6. Get Featured Products
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts()
    {
        // Return featured products
    }

    // 7. Get Popular Products
    [HttpGet("popular")]
    public async Task<IActionResult> GetPopularProducts()
    {
        // Return popular products
    }
}
```

---

### 4️⃣ Merchant API Endpoints

```csharp
// Controllers/MerchantsController.cs

[ApiController]
[Route("api/v1/merchants")]
public class MerchantsController : ControllerBase
{
    // 1. Get All Merchants
    [HttpGet]
    public async Task<IActionResult> GetMerchants(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] double? latitude = null,
        [FromQuery] double? longitude = null,
        [FromQuery] double radius = 5.0)
    {
        // Return merchants with distance calculation
    }

    // 2. Get Merchant by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMerchantById(string id)
    {
        // Return merchant details
    }

    // 3. Search Merchants
    [HttpGet("search")]
    public async Task<IActionResult> SearchMerchants([FromQuery] string query)
    {
        // Search by name, category, tags
    }

    // 4. Get Featured Merchants
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedMerchants()
    {
        // Return featured merchants
    }

    // 5. Get Categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        // Return merchant categories
    }

    // 6. Get Nearby Merchants
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearbyMerchants(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radius = 5.0)
    {
        // PostGIS/SQL Server spatial query
        // Return nearby merchants
    }
}
```

---

### 5️⃣ Order API Endpoints

```csharp
// Controllers/OrdersController.cs

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    // 1. Create Order
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Validate cart
        // Create order
        // Process payment
        // Clear cart
        // Trigger SignalR notification
        // Return: Order
    }

    // 2. Get User Orders
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status = null)
    {
        // Return user's orders
    }

    // 3. Get Order by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        // Return order details
    }

    // 4. Cancel Order
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(string id)
    {
        // Cancel order
        // Refund payment
    }
}
```

---

### 6️⃣ SignalR Hubs (Real-time)

```csharp
// Hubs/OrderHub.cs

public class OrderHub : Hub
{
    // Order status updates
    public async Task JoinOrderGroup(string orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    public async Task LeaveOrderGroup(string orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    // Backend sends updates to clients
    public async Task SendOrderUpdate(string orderId, OrderStatus status)
    {
        await Clients.Group($"order_{orderId}")
            .SendAsync("ReceiveOrderUpdate", new { orderId, status });
    }
}

// Hubs/TrackingHub.cs
public class TrackingHub : Hub
{
    // Courier location updates
    public async Task SendLocationUpdate(string orderId, double lat, double lng)
    {
        await Clients.Group($"tracking_{orderId}")
            .SendAsync("ReceiveLocationUpdate", new { lat, lng });
    }
}
```

**SignalR Startup Configuration:**
```csharp
// Program.cs
builder.Services.AddSignalR();

app.MapHub<OrderHub>("/hubs/order");
app.MapHub<TrackingHub>("/hubs/tracking");
app.MapHub<NotificationHub>("/hubs/notification");
```

---

### 7️⃣ Database Schema

**Minimum Tables:**

```sql
-- Users Table
CREATE TABLE Users (
    Id NVARCHAR(50) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    ProfileImageUrl NVARCHAR(500),
    Role NVARCHAR(50) DEFAULT 'User',
    IsEmailVerified BIT DEFAULT 0,
    IsPhoneVerified BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- RefreshTokens Table
CREATE TABLE RefreshTokens (
    Id NVARCHAR(50) PRIMARY KEY,
    UserId NVARCHAR(50) FOREIGN KEY REFERENCES Users(Id),
    Token NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    RevokedAt DATETIME2
);

-- Merchants Table
CREATE TABLE Merchants (
    Id NVARCHAR(50) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    LogoUrl NVARCHAR(500),
    BannerUrl NVARCHAR(500),
    Category NVARCHAR(100),
    Rating DECIMAL(3,2),
    ReviewCount INT DEFAULT 0,
    DeliveryFee DECIMAL(10,2),
    MinimumOrder DECIMAL(10,2),
    EstimatedDeliveryTime INT, -- minutes
    IsOpen BIT DEFAULT 1,
    IsFeatured BIT DEFAULT 0,
    Address NVARCHAR(500),
    Phone NVARCHAR(20),
    Email NVARCHAR(255),
    Tags NVARCHAR(500), -- JSON array
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Products Table
CREATE TABLE Products (
    Id NVARCHAR(50) PRIMARY KEY,
    MerchantId NVARCHAR(50) FOREIGN KEY REFERENCES Merchants(Id),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(10,2) NOT NULL,
    ImageUrl NVARCHAR(500),
    Category NVARCHAR(100),
    Stock INT DEFAULT 0,
    IsAvailable BIT DEFAULT 1,
    Rating DECIMAL(3,2),
    ReviewCount INT DEFAULT 0,
    Tags NVARCHAR(500), -- JSON array
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Carts Table
CREATE TABLE Carts (
    Id NVARCHAR(50) PRIMARY KEY,
    UserId NVARCHAR(50) FOREIGN KEY REFERENCES Users(Id),
    Subtotal DECIMAL(10,2) DEFAULT 0,
    DeliveryFee DECIMAL(10,2) DEFAULT 0,
    Total DECIMAL(10,2) DEFAULT 0,
    CouponCode NVARCHAR(50),
    DiscountAmount DECIMAL(10,2),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- CartItems Table
CREATE TABLE CartItems (
    Id NVARCHAR(50) PRIMARY KEY,
    CartId NVARCHAR(50) FOREIGN KEY REFERENCES Carts(Id),
    ProductId NVARCHAR(50) FOREIGN KEY REFERENCES Products(Id),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    AddedAt DATETIME2 DEFAULT GETDATE()
);

-- Orders Table
CREATE TABLE Orders (
    Id NVARCHAR(50) PRIMARY KEY,
    UserId NVARCHAR(50) FOREIGN KEY REFERENCES Users(Id),
    MerchantId NVARCHAR(50) FOREIGN KEY REFERENCES Merchants(Id),
    Status NVARCHAR(50) DEFAULT 'Pending',
    Subtotal DECIMAL(10,2),
    DeliveryFee DECIMAL(10,2),
    Total DECIMAL(10,2),
    PaymentMethod NVARCHAR(50),
    DeliveryAddress NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
```

---

### 8️⃣ JWT Authentication Setup

```csharp
// Program.cs

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            )
        };
    });

// Token generation service
public class TokenService
{
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
```

---

### 9️⃣ Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GetirDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "https://api.getir.com",
    "Audience": "getir-mobile-app",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "CORS": {
    "AllowedOrigins": ["https://localhost:5001", "capacitor://localhost", "http://localhost"]
  }
}
```

---

### 🔟 CORS Configuration

```csharp
// Program.cs

builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp", policy =>
    {
        policy.WithOrigins(
            "capacitor://localhost",
            "http://localhost",
            "https://localhost:5001"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

app.UseCors("MobileApp");
```

---

## 📋 Backend Development Checklist

### Phase 1: Core Backend (2-3 hafta)

- [ ] **Week 1: Authentication**
  - [ ] User entity & database
  - [ ] JWT token service
  - [ ] AuthController endpoints
  - [ ] Password hashing (BCrypt)
  - [ ] Email verification
  - [ ] Refresh token mechanism

- [ ] **Week 2: Core Entities**
  - [ ] Merchant CRUD
  - [ ] Product CRUD
  - [ ] Cart system
  - [ ] Order system
  - [ ] Database migrations

- [ ] **Week 3: Integration**
  - [ ] SignalR hubs
  - [ ] Real-time updates
  - [ ] Payment integration
  - [ ] Error handling
  - [ ] Logging

---

### Phase 2: Advanced Features (1-2 hafta)

- [ ] **Week 4: Polish**
  - [ ] Address management
  - [ ] Profile management
  - [ ] Notification system
  - [ ] Review system
  - [ ] Search optimization

- [ ] **Week 5: Production**
  - [ ] Unit tests
  - [ ] Integration tests
  - [ ] Performance testing
  - [ ] Deployment setup
  - [ ] Monitoring

---

## 🔄 Flutter Tarafında Yapılacak (Backend hazır olduktan sonra)

### 1. Environment Config Güncelleme

```dart
// lib/core/constants/environment_config.dart

// DEV environment
static const String devApiBaseUrl = 'http://localhost:5000/api/v1';
static const String devSignalRUrl = 'http://localhost:5000/hubs';

// PROD environment
static const String prodApiBaseUrl = 'https://your-backend.azurewebsites.net/api/v1';
static const String prodSignalRUrl = 'https://your-backend.azurewebsites.net/hubs';
```

### 2. Test Backend Connectivity

```dart
// Test script
void testBackendConnection() async {
  final dio = Dio(BaseOptions(baseUrl: 'http://localhost:5000'));
  
  try {
    final response = await dio.get('/api/v1/health');
    print('✅ Backend connected: ${response.statusCode}');
  } catch (e) {
    print('❌ Backend connection failed: $e');
  }
}
```

### 3. Error Handling İyileştirme

Backend hazır olunca, gerçek error response'ları test edip error handling'i iyileştir:

```dart
// Repository'lerde try-catch ekle
@override
Future<UserEntity> login(String email, String password) async {
  try {
    final response = await _dataSource.login(request);
    return response.toUserModel().toDomain();
  } on DioException catch (e) {
    throw ExceptionFactory.fromDioError(e);
  } catch (e) {
    throw AppException(message: 'Unexpected error: $e');
  }
}
```

---

## ⚙️ Deployment Options

### Option 1: Local Development
```
.NET Backend: http://localhost:5000
Flutter App: Android Emulator → 10.0.2.2:5000
```

### Option 2: Azure App Service
```
.NET Backend: Azure App Service
Database: Azure SQL Database
SignalR: Azure SignalR Service
```

### Option 3: Docker
```
Docker Compose:
  - Backend container
  - Database container
  - Redis container (optional)
```

---

## 🎯 Özet - Backend Nedir, Ne Yapacaksın?

### Basitçe Açıklarsak:

**Şu an:**
```
Flutter App → API Call → ❌ Backend YOK → Error!
```

**Olması gereken:**
```
Flutter App → API Call → ✅ .NET Backend → Database → Response
```

### Senin Yapacağın:

1. **Visual Studio'da yeni .NET Web API projesi aç**
2. **AuthController, ProductsController, vs. yaz**
3. **SQL Server database setup yap**
4. **JWT authentication ekle**
5. **SignalR hub'larını kur**
6. **Local'de çalıştır (localhost:5000)**
7. **Flutter app'i bağla (environment config değiştir)**
8. **Test et**

**Süre:** 3-4 hafta full-time development

---

## 📝 En Basit Test Senaryosu

### Backend'i Test Et (Postman):

```http
POST http://localhost:5000/api/v1/auth/login
Content-Type: application/json

{
  "email": "test@getir.com",
  "password": "Test123456"
}

Expected Response:
{
  "accessToken": "eyJhbGciOiJIUzI1...",
  "refreshToken": "abc123...",
  "user": {
    "id": "1",
    "email": "test@getir.com",
    "firstName": "Test",
    "lastName": "User"
  }
}
```

### Flutter'dan Test Et:

```dart
final dio = Dio(BaseOptions(baseUrl: 'http://localhost:5000'));
final response = await dio.post('/api/v1/auth/login', data: {
  'email': 'test@getir.com',
  'password': 'Test123456',
});
print(response.data); // Token gelecek
```

---

## 🚨 Kritik Not

**Backend olmadan Flutter app çalışmaz!**

Şu an Flutter app'de:
- ✅ UI var
- ✅ State management var
- ✅ API client kodu var
- ❌ **Backend YOK** → App çalışmıyor

Backend bitince:
- ✅ Login çalışacak
- ✅ Ürünler yüklenecek
- ✅ Sepet çalışacak
- ✅ Sipariş verilebilecek

---

## 📊 Gerekli Teknolojiler (.NET Tarafı)

```
✅ ASP.NET Core 8.0
✅ Entity Framework Core
✅ SQL Server (veya PostgreSQL)
✅ JWT Authentication
✅ SignalR
✅ AutoMapper
✅ FluentValidation
✅ Serilog (logging)
```

**Senin zaten bildiğin teknolojiler!** 💪

---

## 🎯 Sonuç

**Backend hazır değil = App çalışmıyor**

**Çözüm:**
1. .NET Web API projesi oluştur
2. Yukarıdaki endpoint'leri yaz
3. Database setup yap
4. Flutter app'i bağla

**Alternatif (Hızlı Test İçin):**
- Mock API kullan (json-server, MockAPI.io)
- Gerçek backend gelene kadar test et

---

**Anladın mı şimdi backend konusunu?** 

Backend = Senin yazacağın .NET API servisleri + Database 🎯
