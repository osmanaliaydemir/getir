# 🚀 Getir Clone API - .NET 9

Modern, scalable ve production-ready bir e-ticaret/delivery API'si. Clean Architecture, DDD prensipleri ve best practice'lerle geliştirilmiştir.

## 📋 Özellikler

### 🏗️ Mimari
- **.NET 9** runtime
- **Clean Architecture** (WebApi → Application → Domain → Infrastructure)
- **Generic Repository + Unit of Work** pattern
- **SOLID** prensipleri
- **Domain-Driven Design** yaklaşımı

### 🔐 Güvenlik
- **JWT Authentication** (Access + Refresh Token)
- **Password Hashing** (RFC2898 PBKDF2)
- **Authorization** middleware
- **Secure token validation**

### 🛠️ Kalite & Monitoring
- **FluentValidation** - Request validation
- **Serilog** - Structured logging
- **Health Checks** - Database monitoring
- **API Versioning** - v1 desteği
- **Error Handling** - Global exception middleware
- **Request Correlation** - X-Request-Id tracking

### 📦 Teknik Stack
- **Database**: SQL Server
- **ORM**: Entity Framework Core 9
- **API**: Minimal API
- **Documentation**: Swagger/OpenAPI
- **Validation**: FluentValidation
- **Logging**: Serilog

## 🚀 Hızlı Başlangıç

### 🐳 Yöntem 1: Docker ile (ÖNERİLEN) ⭐

**En kolay ve hızlı yöntem!**

```bash
# Tek komut - Her şey hazır!
docker-compose up -d

# API: http://localhost:7001
# Swagger: http://localhost:7001
# Health: http://localhost:7001/health
```

**Gereksinim:** Sadece [Docker Desktop](https://docs.docker.com/desktop/)

✅ SQL Server otomatik kuruluyor  
✅ Database otomatik oluşturuluyor  
✅ API otomatik başlıyor  
✅ Tüm konfigürasyon hazır  

**Detaylı bilgi:** [DOCKER-GUIDE.md](docs/DOCKER-GUIDE.md) | [Quick Start](docs/DOCKER-QUICK-START.md)

---

### 💻 Yöntem 2: Manuel Kurulum

**Ön Gereksinimler:**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (OAA\MSSQLSERVER2014)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) veya [VS Code](https://code.visualstudio.com/)

### 1️⃣ Kurulum

```bash
# Repository'yi klonla
git clone https://github.com/yourusername/getir-clone.git
cd getir-clone

# Paketleri yükle
dotnet restore

# Solution'ı build et
dotnet build
```

### 2️⃣ Veritabanı Kurulumu

#### Option A: SQL Script ile (Önerilen)

```bash
# Windows Authentication ile
sqlcmd -S OAA\MSSQLSERVER2014 -E -i database/schema.sql

# veya SQL Authentication ile
sqlcmd -S OAA\MSSQLSERVER2014 -U sa -P YourPassword -i database/schema.sql
```

#### Option B: EF Core Migration ile

```bash
# Migration oluştur (eğer şema değişirse)
cd src/Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../WebApi

# Database'i güncelle
dotnet ef database update --startup-project ../WebApi
```

### 3️⃣ Konfigürasyon

`src/WebApi/appsettings.json` dosyasını düzenle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=OAA\\MSSQLSERVER2014;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyMinimum32Characters!",
    "Issuer": "GetirAPI",
    "Audience": "GetirApp"
  }
}
```

**Not:** SQL Authentication kullanmak isterseniz:
```json
"DefaultConnection": "Server=OAA\\MSSQLSERVER2014;Database=GetirDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### 4️⃣ Çalıştırma

```bash
cd src/WebApi
dotnet run

# veya watch mode ile
dotnet watch run
```

API başarıyla başladığında:
- **Swagger UI**: https://localhost:7001
- **Health Check**: https://localhost:7001/health

## 📮 Postman Collection

Tüm endpoint'leri test etmek için hazır Postman collection:

```bash
# Postman'de import edin
docs/Getir-API.postman_collection.json
```

**Özellikler:**
- ✅ Otomatik token yönetimi (login sonrası token'lar kaydedilir)
- ✅ Otomatik ID tracking (merchant/product/order ID'leri)
- ✅ Test scriptleri (her request'te validation)
- ✅ Environment variables
- ✅ Bearer auth otomasyonu

**Hızlı başlangıç:**
1. Collection'ı import et
2. `Register` endpoint'ini çalıştır (token otomatik kaydedilir)
3. Category ID'yi manuel set et: `{{categoryId}}`
4. Diğer endpoint'leri sırayla çalıştır

Detaylı kullanım: [POSTMAN-GUIDE.md](docs/POSTMAN-GUIDE.md)

## 📚 API Kullanımı

### Authentication Flow

#### 1. Register
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Test123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+905551234567"
}
```

#### 2. Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "Test123!"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-token",
  "expiresAt": "2025-10-01T10:30:00Z"
}
```

#### 3. Korumalı Endpoint'lere Erişim
```http
GET /api/v1/orders
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Örnek Request'ler

#### Merchant Oluşturma
```http
POST /api/v1/merchants
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Migros",
  "categoryId": "guid-here",
  "address": "Kadıköy, İstanbul",
  "latitude": 40.9897,
  "longitude": 29.0257,
  "phoneNumber": "+902161234567",
  "minimumOrderAmount": 50,
  "deliveryFee": 15
}
```

#### Sipariş Oluşturma (Transaction örneği)
```http
POST /api/v1/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "merchantId": "guid-here",
  "items": [
    {
      "productId": "guid-here",
      "quantity": 2
    }
  ],
  "deliveryAddress": "Kadıköy Mah. No:123",
  "deliveryLatitude": 40.9897,
  "deliveryLongitude": 29.0257,
  "paymentMethod": "CreditCard"
}
```

## 🏗️ Proje Yapısı

```
getir/
├── src/
│   ├── WebApi/                  # API Layer
│   │   ├── Endpoints/          # Minimal API endpoints
│   │   ├── Middleware/         # Custom middleware
│   │   ├── Configuration/      # Startup configs
│   │   └── Program.cs
│   ├── Application/            # Business Logic
│   │   ├── Services/          # Service implementations
│   │   ├── DTO/               # Data Transfer Objects
│   │   ├── Validators/        # FluentValidation rules
│   │   ├── Abstractions/      # Repository interfaces
│   │   └── Common/            # Result, PagedResult
│   ├── Domain/                # Domain Models
│   │   └── Entities/         # EF Core entities
│   └── Infrastructure/        # Data & External Services
│       ├── Persistence/      # EF Core, Repositories
│       └── Security/         # JWT, Password hashing
├── database/
│   └── schema.sql            # Database schema
├── .github/
│   └── workflows/
│       └── ci.yml           # CI/CD pipeline
└── README.md
```

## 🔍 Temel Katmanlar

### 1. **Domain Layer** (En içteki katman)
- Pure entities (POCO)
- Business rules
- Hiçbir external dependency yok

### 2. **Application Layer**
- Business logic & use cases
- Service interfaces & implementations
- DTO'lar ve validasyon kuralları
- Repository abstractions

### 3. **Infrastructure Layer**
- Data access (EF Core)
- External services
- Repository implementations
- Security & JWT

### 4. **WebApi Layer** (En dıştaki katman)
- API endpoints
- Middleware
- Dependency injection
- Configuration

## 🧪 Kalite Kontrolleri

### Error Codes Convention
```
AUTH_*          → 401 Unauthorized
NOT_FOUND_*     → 404 Not Found
VALIDATION_*    → 400 Bad Request
FORBIDDEN_*     → 403 Forbidden
CONFLICT_*      → 409 Conflict
```

### Logging
Her request otomatik olarak `X-Request-Id` ile tag'lenir:
```
[10:30:45 INF] abc-123-def POST /api/v1/orders responded 200 in 124ms
```

### Pagination
Tüm list endpoint'leri standart pagination destekler:
```http
GET /api/v1/merchants?page=1&pageSize=20&sortBy=name&sortDir=asc
```

Response:
```json
{
  "items": [...],
  "total": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## 🔄 Database Migration (Opsiyonel)

Eğer Code-First yaklaşıma geçmek isterseniz:

```bash
# Migration oluştur
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebApi

# Database güncelle
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi

# Migration geri al
dotnet ef migrations remove --project src/Infrastructure --startup-project src/WebApi
```

## 🧰 Faydalı Komutlar

```bash
# Build
dotnet build

# Run
dotnet run --project src/WebApi

# Watch mode (auto-reload)
dotnet watch run --project src/WebApi

# Tests (ileride eklenecek)
dotnet test

# Clean
dotnet clean
```

## 📊 Health Check

```bash
curl https://localhost:7001/health
```

Response:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "sql-server",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0234567"
    }
  ],
  "totalDuration": "00:00:00.0456789"
}
```

## 🎯 Sonraki Adımlar (TODO)

Şu anki implementasyon **iskelet + örnek CRUD'lar** içeriyor. `docs/endpoint.md` dosyasındaki 80+ endpoint'i tamamlamak için:

### Eklenecek Modüller:
- [ ] User Profile & Address Management
- [ ] Category Management
- [ ] Shopping Cart
- [ ] Payment Integration
- [ ] Courier Management
- [ ] Campaigns & Coupons
- [ ] Notifications (SignalR)
- [ ] Search & Filtering
- [ ] Rating & Reviews
- [ ] Admin Panel APIs

### Teknik İyileştirmeler:
- [x] **Unit Tests (xUnit + Moq)** ✅ 22/22 PASSED
- [x] **Test Coverage Setup (Coverlet)** ✅ Ready
- [x] **CI/CD with Tests** ✅ GitHub Actions
- [x] **Docker & Docker Compose** ✅ One-command setup
- [ ] Integration Tests (DB provider conflict - optional)
- [ ] Redis Caching
- [ ] RabbitMQ/Kafka Event Bus
- [ ] Elasticsearch
- [ ] CDN Integration
- [ ] Rate Limiting
- [ ] API Gateway (Ocelot/YARP)

## 📊 Implemented Features

### ✅ **Core Modules (8)**
1. 🔐 **Authentication** - JWT with Access/Refresh tokens
2. 📂 **Categories** - Full CRUD
3. 🏪 **Merchants** - Store management
4. 🍔 **Products** - Inventory management
5. 📦 **Orders** - Transaction-based ordering
6. 👤 **User Addresses** - Location management
7. 🛒 **Shopping Cart** - Smart cart with merchant constraint
8. 🎁 **Coupons & Campaigns** - Discount system

### ✅ **Advanced Features**
- 🔎 **Search & Filtering** - Multi-criteria search
- 🔔 **Notifications** - User notification system
- 🚴 **Courier Management** - Delivery tracking
- ❤️ **Health Checks** - System monitoring

### 📊 **Statistics**
- **Total Endpoints:** 44
- **Public Endpoints:** 12
- **Protected Endpoints:** 32
- **Database Tables:** 14
- **Services:** 12
- **Validators:** 15+
- **Entities:** 14

### 🎯 **Business Logic**
- ✅ Transaction management (UoW pattern)
- ✅ Stock control & updates
- ✅ Cart merchant constraint
- ✅ Coupon validation & calculation
- ✅ Address default management
- ✅ Soft delete pattern
- ✅ Pagination everywhere
- ✅ Error code standardization

---

## 📚 Documentation

- **[API Documentation](docs/API-DOCUMENTATION.md)** - Complete API reference with examples
- **[Endpoint Summary](docs/ENDPOINTS-SUMMARY.md)** - Quick reference for all 44 endpoints
- **[Postman Guide](docs/POSTMAN-GUIDE.md)** - Comprehensive testing guide
- **[Docker Guide](docs/DOCKER-GUIDE.md)** - 🐳 Docker deployment & commands
- **[Testing Guide](docs/TESTING-GUIDE.md)** - Unit tests & coverage
- **[Connection String Guide](docs/CONNECTION-STRING-GUIDE.md)** - Database configuration

---

## 📝 Lisans

MIT License

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📧 İletişim

Sorularınız için: support@getir.com

---

**⚡ Built with .NET 9 & ❤️ - Production Ready!**
