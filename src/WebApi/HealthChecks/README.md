# Health Checks Implementation - Global Standards

## 📋 Genel Bakış

Bu proje, **global standartlarda** enterprise-level health check sistemi içermektedir. Kubernetes, Docker, Azure App Service ve diğer cloud platformlarıyla tam uyumludur.

## 🎯 Özellikler

### ✅ Desteklenen Health Checks

1. **SQL Server Health Check** - Veritabanı bağlantı kontrolü
2. **DbContext Health Check** - EF Core, migration durumu ve query performance
3. **Redis Health Check** - Cache sunucusu durumu (fallback: MemoryCache)
4. **SignalR Health Check** - Real-time hub'ların durumu
5. **Memory Health Check** - RAM kullanımı ve GC istatistikleri
6. **Disk Space Health Check** - Disk alanı kullanımı
7. **External API Health Check** - İnternet bağlantısı ve external API erişimi
8. **Startup Time Health Check** - Uygulama başlatma süresi ve uptime

### 🔗 Endpoint'ler

| Endpoint | Açıklama | Kullanım |
|----------|----------|----------|
| `/health` | Tüm health check'lerin detaylı JSON çıktısı | Genel monitoring |
| `/health/live` | Liveness probe (uygulama çalışıyor mu?) | Kubernetes liveness |
| `/health/ready` | Readiness probe (trafik alabilir mi?) | Kubernetes readiness |
| `/health/startup` | Startup probe (başlangıç tamamlandı mı?) | Kubernetes startup |
| `/health/simple` | Basit status çıktısı (healthy/unhealthy) | Load balancer checks |
| `/health-ui` | Visual dashboard (UI) | Yönetici paneli |

## 🚀 Kurulum

### 1. NuGet Paketleri (Otomatik Yüklendi)

```xml
<PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.SignalR" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.UI" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="9.0.0" />
<PackageReference Include="AspNetCore.HealthChecks.UI.InMemory.Storage" Version="9.0.0" />
```

### 2. appsettings.json Konfigürasyonu

```json
{
  "HealthChecks": {
    "Memory": {
      "Enabled": true,
      "ThresholdBytes": 1073741824,  // 1GB
      "Tags": [ "memory", "system" ]
    },
    "DiskSpace": {
      "Enabled": true,
      "ThresholdBytes": 1073741824,  // 1GB
      "Tags": [ "disk", "system" ]
    },
    "Database": {
      "Enabled": true,
      "TimeoutSeconds": 5,
      "Tags": [ "db", "sqlserver", "ready" ]
    },
    "Redis": {
      "Enabled": true,
      "TimeoutSeconds": 5,
      "Tags": [ "cache", "redis", "ready" ]
    }
  },
  "HealthChecksUI": {
    "Enabled": true,
    "UIPath": "/health-ui",
    "ApiPath": "/health-api",
    "EvaluationTimeInSeconds": 30
  }
}
```

## 📊 Response Format (JSON)

### Başarılı Response Örneği

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:01.2345678",
  "entries": {
    "sql_server": {
      "status": "Healthy",
      "description": "Database is healthy",
      "duration": "00:00:00.123",
      "data": {}
    },
    "redis": {
      "status": "Healthy",
      "description": "Redis is connected",
      "duration": "00:00:00.045",
      "data": {
        "ping_time_ms": 2.5
      }
    },
    "memory_usage": {
      "status": "Healthy",
      "description": "Memory usage is healthy: 256.45MB",
      "duration": "00:00:00.001",
      "data": {
        "allocated_mb": 256.45,
        "threshold_mb": 1024.0,
        "usage_percentage": 25.04,
        "gen0_collections": 12,
        "gen1_collections": 3,
        "gen2_collections": 1
      }
    }
  }
}
```

### Degraded/Unhealthy Response Örneği

```json
{
  "status": "Degraded",
  "totalDuration": "00:00:02.1234567",
  "entries": {
    "redis": {
      "status": "Degraded",
      "description": "Redis is not connected. Using MemoryCache fallback.",
      "duration": "00:00:00.567",
      "data": {
        "status": "disconnected",
        "fallback": "memory_cache"
      }
    }
  }
}
```

## 🔧 Kubernetes Integration

### Deployment YAML Örneği

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: getir-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: getir-api:latest
        ports:
        - containerPort: 80
        
        # Liveness Probe - Uygulama çalışıyor mu?
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        # Readiness Probe - Trafik alabilir mi?
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        # Startup Probe - İlk başlatma tamamlandı mı?
        startupProbe:
          httpGet:
            path: /health/startup
            port: 80
          initialDelaySeconds: 0
          periodSeconds: 10
          timeoutSeconds: 3
          failureThreshold: 30
```

## 🐳 Docker Health Check

### Dockerfile Örneği

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Health check ekleme
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost/health/live || exit 1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/WebApi/WebApi.csproj", "src/WebApi/"]
RUN dotnet restore "src/WebApi/WebApi.csproj"
COPY . .
WORKDIR "/src/src/WebApi"
RUN dotnet build "WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Getir.WebApi.dll"]
```

## 🎨 Health Check UI

UI'ı etkinleştirmek için `appsettings.json`:

```json
{
  "HealthChecksUI": {
    "Enabled": true,
    "UIPath": "/health-ui",
    "ApiPath": "/health-api"
  }
}
```

UI'a erişim: `https://yourdomain.com/health-ui`

## 📈 Monitoring & Alerting

### Prometheus Integration

Health check'ler otomatik olarak `/metrics` endpoint'ine eklenir (prometheus-net paketi ile).

### Application Insights

Azure Application Insights ile otomatik entegrasyon mevcuttur.

## 🔐 Güvenlik

⚠️ **Production Önerisi:**

Health check endpoint'leri hassas bilgiler içerebilir. Production ortamında:

1. `/health-ui` endpoint'ini sadece internal network'ten erişilebilir yapın
2. API Key veya JWT authentication ekleyin
3. Response'larda hassas bilgileri (connection strings, vb.) göstermeyin

### Authentication Ekleme (İsteğe Bağlı)

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).RequireAuthorization(); // JWT authentication gerektirir
```

## 🧪 Test Etme

### cURL ile Test

```bash
# Tüm health check'ler
curl https://localhost:5001/health

# Liveness probe
curl https://localhost:5001/health/live

# Readiness probe
curl https://localhost:5001/health/ready

# Simple status
curl https://localhost:5001/health/simple
```

### PowerShell ile Test

```powershell
Invoke-RestMethod -Uri "https://localhost:5001/health" -Method Get
```

## 📝 Custom Health Check Ekleme

Yeni bir health check eklemek için:

1. `IHealthCheck` interface'ini implement edin:

```csharp
public class CustomHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Your check logic here
        var isHealthy = await CheckSomethingAsync();
        
        if (isHealthy)
        {
            return HealthCheckResult.Healthy("Everything is good");
        }
        
        return HealthCheckResult.Unhealthy("Something is wrong");
    }
}
```

2. `HealthChecksConfig.cs` içinde kaydedin:

```csharp
healthChecksBuilder.AddCheck<CustomHealthCheck>(
    "custom_check",
    tags: new[] { "custom", "ready" });
```

## 🏷️ Health Check Tags

| Tag | Açıklama | Kullanım |
|-----|----------|----------|
| `live` | Liveness probe için | Kubernetes liveness |
| `ready` | Readiness probe için | Kubernetes readiness |
| `startup` | Startup probe için | Kubernetes startup |
| `db` | Database checks | Filtering |
| `cache` | Cache checks | Filtering |
| `external` | External dependency checks | Filtering |

## 📚 Kaynaklar

- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [AspNetCore.Diagnostics.HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Kubernetes Health Checks](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)

## 🎉 Sonuç

Bu implementasyon ile:
- ✅ Production-ready health monitoring
- ✅ Kubernetes native support
- ✅ Docker health checks
- ✅ Visual dashboard
- ✅ Detailed JSON responses
- ✅ Configurable thresholds
- ✅ Multiple endpoints for different purposes

**Global standartlarda, enterprise-level health check sistemi hazır!** 🚀

