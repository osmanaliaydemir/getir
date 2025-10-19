# 🚀 Redis Caching Setup Guide - Getir API

## 📋 İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Redis Kurulumu](#redis-kurulumu)
3. [Proje Yapısı](#proje-yapısı)
4. [Konfigürasyon](#konfigürasyon)
5. [Kullanım Örnekleri](#kullanım-örnekleri)
6. [Cache Stratejileri](#cache-stratejileri)
7. [Production Deployment](#production-deployment)
8. [Monitoring & Troubleshooting](#monitoring--troubleshooting)

---

## 🎯 Genel Bakış

Bu projede **Redis caching altyapısı** global standartlara uygun şekilde kurulmuştur:

### ✅ Özellikler
- **Hybrid Caching:** Redis primary, MemoryCache fallback
- **Circuit Breaker Pattern:** Redis çöktüğünde otomatik fallback
- **Cache Invalidation:** Pattern-based cache temizleme
- **Health Checks:** Redis sağlık durumu monitoring
- **Centralized Keys:** Tüm cache key'ler merkezi yönetim
- **Clean Architecture:** Framework-agnostic design

### 📊 Mimari

```
┌─────────────────────────────────────────────────┐
│  WebApi Layer (Controllers)                    │
│  └─ Dependency Injection: ICacheService        │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Application Layer (Services)                   │
│  ├─ ProductService                              │
│  ├─ MerchantService                             │
│  └─ Uses: CacheKeys + ICacheService             │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────┐
│  Infrastructure Layer (Implementation)          │
│  ├─ RedisCacheService (Primary)                 │
│  ├─ MemoryCacheService (Fallback)               │
│  └─ Connection: StackExchange.Redis             │
└─────────────────────────────────────────────────┘
```

---

## 🔧 Redis Kurulumu

### 1️⃣ Windows (Memurai - Redis Fork)

**Memurai** Windows için optimize edilmiş Redis fork'udur:

```powershell
# Chocolatey ile kurulum
choco install memurai-developer

# Manuel kurulum
# https://www.memurai.com/get-memurai adresinden indirin
```

**Servisi başlatma:**
```powershell
# Windows Service olarak çalışır, otomatik başlar
net start Memurai

# Kontrol
redis-cli ping
# Beklenen çıktı: PONG
```

### 2️⃣ Windows (WSL2 + Docker)

```bash
# WSL2'de Docker ile Redis
docker run -d --name redis-getir \
  -p 6379:6379 \
  redis:7-alpine \
  redis-server --appendonly yes

# Kontrol
docker exec -it redis-getir redis-cli ping
```

### 3️⃣ Linux/macOS

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install redis-server
sudo systemctl start redis-server

# macOS (Homebrew)
brew install redis
brew services start redis

# Kontrol
redis-cli ping
```

### 4️⃣ Docker Compose (Önerilen - Development)

Proje root'unda `docker-compose.yml` oluşturun:

```yaml
version: '3.8'

services:
  redis:
    image: redis:7-alpine
    container_name: getir-redis
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes --requirepass "" --maxmemory 256mb --maxmemory-policy allkeys-lru
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 3
    restart: unless-stopped

volumes:
  redis-data:
    driver: local
```

**Başlatma:**
```bash
docker-compose up -d redis
docker-compose logs -f redis
```

---

## 📂 Proje Yapısı

### Oluşturulan Dosyalar

```
src/
├── Application/
│   └── Common/
│       ├── ICacheService.cs              ✅ Cache abstraction
│       └── CacheKeys.cs                  ✅ Centralized key management
│
├── Infrastructure/
│   ├── Configuration/
│   │   └── RedisSettings.cs              ✅ Strongly-typed settings
│   └── Services/
│       └── Caching/
│           ├── RedisCacheService.cs      ✅ Redis implementation
│           └── MemoryCacheService.cs     ✅ Fallback implementation
│
└── WebApi/
    ├── Extensions/
    │   └── InfrastructureServiceExtensions.cs  ✅ DI registration
    ├── HealthChecks/
    │   └── RedisHealthCheck.cs           ✅ Health monitoring
    └── appsettings.json                  ✅ Configuration
```

---

## ⚙️ Konfigürasyon

### appsettings.json

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,abortConnect=false,connectRetry=3"
  },
  "Redis": {
    "Enabled": true,
    "InstanceName": "Getir:",
    "Configuration": "localhost:6379",
    "AbortOnConnectFail": false,
    "ConnectRetry": 3,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "KeepAlive": 60,
    "AllowAdmin": false,
    "Ssl": false,
    "Password": "",
    "DefaultDatabase": 0
  }
}
```

### appsettings.Development.json

```json
{
  "Redis": {
    "Enabled": true,
    "Configuration": "localhost:6379",
    "AbortOnConnectFail": false
  }
}
```

### appsettings.Production.json

```json
{
  "Redis": {
    "Enabled": true,
    "Configuration": "your-redis.cache.windows.net:6380",
    "Password": "YOUR_PRODUCTION_REDIS_PASSWORD",
    "Ssl": true,
    "AbortOnConnectFail": false,
    "ConnectRetry": 5,
    "ConnectTimeout": 10000,
    "SyncTimeout": 10000
  }
}
```

### Environment Variables (Önerilen - Production)

```bash
# Azure Redis Cache örneği
export Redis__Enabled=true
export Redis__Configuration="getir-cache.redis.cache.windows.net:6380"
export Redis__Password="your_secure_password_here"
export Redis__Ssl=true
```

---

## 💻 Kullanım Örnekleri

### 1️⃣ Cache-Aside Pattern (En Yaygın)

```csharp
// ProductService.cs
public async Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken ct)
{
    var cacheKey = CacheKeys.Product(id);
    
    return await GetOrSetCacheAsync(
        cacheKey,
        async () =>
        {
            // Database'den çek
            var product = await _repository.GetByIdAsync(id, ct);
            return Result.Ok(product);
        },
        TimeSpan.FromMinutes(CacheKeys.TTL.Medium), // 15 dakika
        ct);
}
```

### 2️⃣ Write-Through Pattern (Update ile cache güncelleme)

```csharp
// ProductService.cs - UpdateProductAsync
public async Task<Result> UpdateProductAsync(Guid id, UpdateRequest request)
{
    var product = await _repository.GetByIdAsync(id);
    
    // Update database
    product.Name = request.Name;
    await _repository.UpdateAsync(product);
    
    // ============= CACHE INVALIDATION =============
    // 1. Tek ürünü invalidate et
    await _cacheService.RemoveAsync(CacheKeys.Product(id));
    
    // 2. Ürün listelerini invalidate et (pattern-based)
    await _cacheService.RemoveByPatternAsync(
        CacheKeys.AllProductsByMerchant(product.MerchantId)
    );
    
    return Result.Ok();
}
```

### 3️⃣ Pattern-Based Cache Invalidation

```csharp
// Merchant update olduğunda ilgili tüm cache'leri temizle
await _cacheService.RemoveByPatternAsync("merchant:123e4567:*");

// Tüm product listelerini temizle
await _cacheService.RemoveByPatternAsync("products:*");
```

### 4️⃣ Direct Cache Usage

```csharp
// Manuel cache kullanımı
public class MyService
{
    private readonly ICacheService _cache;
    
    // Cache'e yaz
    await _cache.SetAsync(
        "my-key", 
        myObject, 
        TimeSpan.FromMinutes(10)
    );
    
    // Cache'den oku
    var cached = await _cache.GetAsync<MyObject>("my-key");
    
    // Kontrol et
    var exists = await _cache.ExistsAsync("my-key");
    
    // Sil
    await _cache.RemoveAsync("my-key");
}
```

---

## 🎯 Cache Stratejileri

### TTL (Time-To-Live) Önerileri

```csharp
// CacheKeys.TTL constants
public static class TTL
{
    public const int VeryShort = 2;      // 2 dk - Volatile data (cart, session)
    public const int Short = 5;          // 5 dk - Dynamic data (search results)
    public const int Medium = 15;        // 15 dk - Semi-static (products)
    public const int Long = 30;          // 30 dk - Static (merchants, categories)
    public const int VeryLong = 60;      // 1 saat - Rarely changing (configs)
    public const int ExtraLong = 240;    // 4 saat - System settings
}
```

### Cache Key Naming Convention

```csharp
// ✅ İYİ - Namespace kullan
"product:123e4567-e89b-12d3-a456-426614174000"
"products:merchant:123:page:1:size:20"
"merchant:owner:456"

// ❌ KÖTÜ - Flat key'ler
"product_123"
"merchantdata"
"cache_123_456"
```

### Hangi Veriler Cache'lenmeli?

#### ✅ Cache Kullanılmalı
- Ürün listeleri (15 dk)
- Merchant bilgileri (30 dk)
- Kategoriler (1 saat)
- Search sonuçları (5 dk)
- Statik içerik (4 saat)
- API rate limit counter

#### ❌ Cache Kullanılmamalı
- Sipariş durumları (real-time)
- Ödeme işlemleri (critical)
- User authentication state
- Financial transactions
- Real-time tracking data

---

## 🚀 Production Deployment

### Azure Redis Cache

```bash
# Azure CLI ile Redis Cache oluştur
az redis create \
  --name getir-production-cache \
  --resource-group getir-rg \
  --location westeurope \
  --sku Basic \
  --vm-size c0

# Connection string al
az redis list-keys \
  --name getir-production-cache \
  --resource-group getir-rg
```

**appsettings.Production.json:**
```json
{
  "Redis": {
    "Enabled": true,
    "Configuration": "getir-production-cache.redis.cache.windows.net:6380",
    "Password": "AZURE_REDIS_PRIMARY_KEY",
    "Ssl": true,
    "ConnectRetry": 5,
    "ConnectTimeout": 10000
  }
}
```

### AWS ElastiCache

```bash
# Terraform ile ElastiCache cluster
resource "aws_elasticache_cluster" "redis" {
  cluster_id           = "getir-redis"
  engine               = "redis"
  node_type            = "cache.t3.micro"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  port                 = 6379
}
```

### Docker Production

```yaml
# docker-compose.prod.yml
services:
  redis:
    image: redis:7-alpine
    command: >
      redis-server
      --requirepass ${REDIS_PASSWORD}
      --maxmemory 2gb
      --maxmemory-policy allkeys-lru
      --appendonly yes
      --tcp-backlog 511
      --timeout 0
      --tcp-keepalive 300
    volumes:
      - redis-prod-data:/data
    restart: always
    networks:
      - backend
    deploy:
      resources:
        limits:
          memory: 2.5G
```

---

## 📊 Monitoring & Troubleshooting

### Health Check Endpoint

```bash
# Redis durumunu kontrol et
curl https://yourapi.com/health

# Response:
{
  "status": "Healthy",
  "checks": [
    {
      "name": "redis_cache",
      "status": "Healthy",
      "description": "Redis is healthy. Ping: 2.5ms",
      "data": {
        "ping_ms": 2.5,
        "servers": 1,
        "status": "healthy"
      }
    }
  ]
}
```

### Redis Monitoring Commands

```bash
# Redis'e bağlan
redis-cli -h localhost -p 6379

# Tüm key'leri listele (PRODUCTION'DA KULLANMA!)
KEYS *

# Key sayısı
DBSIZE

# Memory kullanımı
INFO memory

# Hit/Miss ratio
INFO stats

# Belirli pattern'deki key'leri bul
SCAN 0 MATCH product:* COUNT 100

# Key'in TTL'ini kontrol et
TTL product:123e4567-e89b-12d3-a456-426614174000

# Key'in değerini gör
GET product:123e4567-e89b-12d3-a456-426614174000
```

### Sık Karşılaşılan Problemler

#### 1. Redis Connection Timeout

**Hata:** `StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s)`

**Çözüm:**
```json
{
  "Redis": {
    "AbortOnConnectFail": false,  // Fallback to MemoryCache
    "ConnectRetry": 5,
    "ConnectTimeout": 10000
  }
}
```

#### 2. Memory Limit Aşımı

**Hata:** `OOM command not allowed when used memory > 'maxmemory'`

**Çözüm:**
```bash
# redis.conf veya command ile
redis-cli CONFIG SET maxmemory 2gb
redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

#### 3. Slow Queries

**Hata:** Response time yavaş

**Çözüm:**
```bash
# Slow log kontrol et
redis-cli SLOWLOG GET 10

# Network latency kontrol et
redis-cli --latency

# Ping kontrol
redis-cli PING
```

#### 4. Cache Stampede

**Problem:** Aynı anda binlerce istek aynı cache'i oluşturmaya çalışıyor

**Çözüm:**
```csharp
// Lock-based approach (already implemented in RedisCacheService)
private readonly SemaphoreSlim _lock = new(1, 1);

await _lock.WaitAsync();
try
{
    // Cache operation
}
finally
{
    _lock.Release();
}
```

---

## 🔐 Security Best Practices

### 1. Redis Password Protection

```bash
# redis.conf
requirepass "your_secure_password_here"

# Connection string
redis-cli -h localhost -p 6379 -a "your_secure_password"
```

### 2. Network Security

```bash
# redis.conf - Sadece localhost'tan bağlantı
bind 127.0.0.1 ::1

# Veya belirli IP'lerden
bind 192.168.1.100 127.0.0.1
```

### 3. Command Filtering

```bash
# redis.conf - Tehlikeli komutları kapat
rename-command FLUSHDB ""
rename-command FLUSHALL ""
rename-command KEYS ""
rename-command CONFIG "CONFIG_ADMIN_ONLY"
```

### 4. SSL/TLS Encryption

```json
{
  "Redis": {
    "Ssl": true,
    "Configuration": "secure-redis.com:6380"
  }
}
```

---

## 📈 Performance Tips

### 1. Connection Pooling

✅ **StackExchange.Redis otomatik yapar** - `IConnectionMultiplexer` singleton olmalı

### 2. Pipeline Operations

```csharp
// Batch operations (already in RedisCacheService.GetManyAsync)
var tasks = keys.Select(k => _cache.GetAsync<T>(k));
await Task.WhenAll(tasks);
```

### 3. Compression

Büyük objeler için:
```csharp
// JSON + GZip compression
var json = JsonSerializer.Serialize(obj);
var compressed = GZip.Compress(json);
await redis.StringSetAsync(key, compressed);
```

### 4. Key Expiration Policy

```bash
# LRU (Least Recently Used) - Önerilen
maxmemory-policy allkeys-lru

# LFU (Least Frequently Used) - Alternatif
maxmemory-policy allkeys-lfu
```

---

## 🧪 Testing

### Unit Test Örneği

```csharp
public class RedisCacheServiceTests
{
    [Fact]
    public async Task GetAsync_WhenRedisDown_ShouldFallbackToMemoryCache()
    {
        // Arrange
        var redis = null; // Simulate Redis down
        var memoryCache = new MemoryCacheService(...);
        var sut = new RedisCacheService(redis, logger, memoryCache);
        
        // Act
        await sut.SetAsync("test-key", myObject, TimeSpan.FromMinutes(5));
        var result = await sut.GetAsync<MyObject>("test-key");
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(myObject.Name);
    }
}
```

---

## 📞 Support & Links

- **Redis Documentation:** https://redis.io/documentation
- **StackExchange.Redis:** https://stackexchange.github.io/StackExchange.Redis/
- **Azure Redis Cache:** https://learn.microsoft.com/azure/azure-cache-for-redis/
- **Memurai (Windows):** https://www.memurai.com/

---

## ✅ Checklist

- [ ] Redis server kuruldu ve çalışıyor
- [ ] `appsettings.json` yapılandırıldı
- [ ] Health check endpoint test edildi (`/health`)
- [ ] Cache key naming convention anlaşıldı
- [ ] ProductService cache çalışıyor
- [ ] MerchantService cache çalışıyor
- [ ] Cache invalidation test edildi
- [ ] Production Redis yapılandırıldı (Azure/AWS)
- [ ] Monitoring kuruldu
- [ ] Security ayarları yapıldı

---

**🚀 Redis caching altyapınız production-ready!**

Sorular için: [GitHub Issues](https://github.com/your-repo/issues)

