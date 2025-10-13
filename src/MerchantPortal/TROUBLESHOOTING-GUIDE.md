# Merchant Portal - Troubleshooting Guide 🔧

**Date:** 2025-10-13  
**Version:** 1.0

---

## 🐛 Common Issues & Solutions

### 1. **SSL Connection Error** ✅ FIXED

**Error:**
```
System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception.
```

**Cause:** Development'ta self-signed certificate

**Solution:**
```csharp
// Program.cs - HttpClient configuration
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    
    return handler;
});
```

**Alternative:** HTTP kullan
```json
// appsettings.Development.json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:7000"  // HTTPS yerine HTTP
  }
}
```

---

### 2. **EmailConfiguration MissingMethodException** ✅ FIXED

**Error:**
```
System.MissingMethodException: Cannot dynamically create an instance of type 'EmailConfiguration'. 
Reason: No parameterless constructor defined.
```

**Cause:** Record type configuration binding sorunu

**Solution:** Record'u class'a çevir
```csharp
// Before (Record)
public record EmailConfiguration(string SmtpServer, ...);

// After (Class)
public class EmailConfiguration
{
    public string SmtpServer { get; set; } = default!;
    public int SmtpPort { get; set; }
    // ... other properties
}
```

---

### 3. **NullReferenceException in ReadOnlyRepository** ⚠️

**Error:**
```
System.NullReferenceException: Object reference not set to an instance of an object.
ReadOnlyRepository.ctor()
```

**Possible Causes:**

#### **Cause A: AppDbContext not registered**
```csharp
// Ensure in WebApi/Program.cs:
builder.Services.AddDbContext<AppDbContext>(options => ...);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

#### **Cause B: Connection string missing**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;"
  }
}
```

**Check:**
```bash
# Test database connection
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

#### **Cause C: Database not accessible**
- SQL Server running olmalı
- Connection string doğru olmalı
- Firewall açık olmalı
- User credentials geçerli olmalı

**Solution:** Detaylı hata mesajı için null check eklendi
```csharp
// UnitOfWork.cs
public UnitOfWork(AppDbContext context)
{
    _context = context ?? throw new ArgumentNullException(
        nameof(context), 
        "AppDbContext cannot be null. Ensure DbContext is registered in DI.");
}
```

---

### 4. **SignalR Connection Failed**

**Error:**
```
Failed to start the connection: Error: Connection disconnected with error 'Error: ...'.
```

**Solutions:**

#### **A) Check Hub URL:**
```json
// appsettings.Development.json
{
  "ApiSettings": {
    "SignalRHubUrl": "https://localhost:7001/hubs/orders"  // Doğru path
  }
}
```

#### **B) Check CORS:**
```csharp
// WebApi/Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5001")  // MerchantPortal URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

#### **C) WebSockets enabled:**
```csharp
// Program.cs
app.UseWebSockets();
```

---

### 5. **Authentication Failed - 401 Unauthorized**

**Error:**
```
401 Unauthorized when calling API endpoints
```

**Solutions:**

#### **A) Token not sent:**
```csharp
// Check ApiClient.cs
_httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

#### **B) Token expired:**
```csharp
// Check token lifetime
"Jwt": {
  "AccessTokenMinutes": 60,  // 1 hour
  "RefreshTokenMinutes": 10080  // 7 days
}
```

#### **C) Role mismatch:**
```csharp
// Controller requires specific role
[Authorize(Roles = "MerchantOwner,Admin")]
```

---

### 6. **Database Migration Issues**

**Error:**
```
Cannot open database "db29009" requested by the login.
```

**Solutions:**

#### **A) Create database:**
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

#### **B) Run migrations:**
```bash
cd database
# SQL Server Management Studio'da çalıştır:
sqlcmd -S your-server -d db29009 -U db29009 -P password -i schema.sql
sqlcmd -S your-server -d db29009 -U db29009 -P password -i seed-data.sql
```

#### **C) Check connection string:**
```
Server=db29009.public.databaseasp.net;
Database=db29009;
User Id=db29009;
Password=Ap6-=2PtcE!7;
TrustServerCertificate=True;
```

---

### 7. **Port Already in Use**

**Error:**
```
Failed to bind to address https://localhost:7001: address already in use.
```

**Solutions:**

#### **A) Kill existing process:**
```bash
# Windows
netstat -ano | findstr :7001
taskkill /PID <process_id> /F

# Linux/Mac
lsof -ti:7001 | xargs kill -9
```

#### **B) Change port:**
```json
// appsettings.Development.json (WebApi)
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:7002"  // Farklı port
      }
    }
  }
}
```

---

### 8. **CORS Error in Browser**

**Error:**
```
Access to XMLHttpRequest at 'https://localhost:7001/api/...' 
from origin 'https://localhost:5001' has been blocked by CORS policy
```

**Solution:**
```csharp
// WebApi/Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://localhost:5001",  // MerchantPortal
            "http://localhost:5000"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Use before MapControllers
app.UseCors();
```

---

### 9. **View Not Found**

**Error:**
```
InvalidOperationException: The view 'Index' was not found.
```

**Solutions:**

#### **A) Check file path:**
```
Views/
  ├── Dashboard/
  │   └── Index.cshtml  ✅
  ├── Shared/
  │   └── _Layout.cshtml  ✅
```

#### **B) Check namespace:**
```csharp
// Controller
namespace Getir.MerchantPortal.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();  // Looks for Views/Dashboard/Index.cshtml
    }
}
```

---

### 10. **Session Lost / User Logged Out**

**Error:**
```
User is logged out unexpectedly
```

**Solutions:**

#### **A) Check session timeout:**
```csharp
// Program.cs
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);  // Increase
    options.Cookie.IsEssential = true;
});
```

#### **B) Check cookie settings:**
```csharp
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
    options.SlidingExpiration = true;  // Renew on activity
});
```

---

## 🔍 Debugging Tips

### 1. **Enable Verbose Logging**

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 2. **Check HTTP Traffic**

**Browser DevTools:**
- Network tab
- Check request headers (Authorization)
- Check response status codes
- Check response body

**Fiddler/Postman:**
- Capture HTTP traffic
- Test API endpoints directly
- Verify token validity

### 3. **Database Connection Test**

```csharp
// Add to WebApi/Program.cs (temporary)
var app = builder.Build();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var canConnect = await context.Database.CanConnectAsync();
    Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");
}
```

### 4. **SignalR Connection Test**

```javascript
// Browser console
connection.on("connected", function() {
    console.log("SignalR connected!");
});

connection.onclose(error => {
    console.error("SignalR disconnected:", error);
});
```

---

## 📞 Support Checklist

Eğer sorun devam ederse, şunları kontrol et:

- [ ] WebApi çalışıyor mu? (`dotnet run` in src/WebApi)
- [ ] Database erişilebilir mi?
- [ ] Connection string doğru mu?
- [ ] Migrations çalıştırıldı mı?
- [ ] Seed data eklendi mi?
- [ ] CORS configuration doğru mu?
- [ ] SSL certificate trust edildi mi? (`dotnet dev-certs https --trust`)
- [ ] Firewall WebApi portunu engelliyor mu?
- [ ] Anti-virus engelliyor mu?
- [ ] NuGet packages restore edildi mi?

---

## 🚀 Quick Start (When Everything Fails)

```bash
# 1. Clean all
dotnet clean Getir.sln

# 2. Restore packages
dotnet restore Getir.sln

# 3. Build solution
dotnet build Getir.sln --configuration Release

# 4. Database setup
cd database
# Run schema.sql and seed-data.sql in SQL Server

# 5. Trust SSL certificate
dotnet dev-certs https --trust

# 6. Run WebApi (Terminal 1)
cd src/WebApi
dotnet run

# 7. Run MerchantPortal (Terminal 2)
cd src/MerchantPortal
dotnet run

# 8. Open browser
# https://localhost:5001
```

---

## 📚 Useful Commands

```bash
# Check running processes
netstat -ano | findstr :7001
netstat -ano | findstr :5001

# Clear NuGet cache
dotnet nuget locals all --clear

# Rebuild solution
dotnet clean && dotnet build

# Check EF migrations
dotnet ef migrations list --project src/Infrastructure --startup-project src/WebApi

# Create new migration
dotnet ef migrations add MigrationName --project src/Infrastructure --startup-project src/WebApi

# Update database
dotnet ef database update --project src/Infrastructure --startup-project src/WebApi
```

---

**Last Updated:** 2025-10-13  
**Author:** AI Assistant with Osman Ali Aydemir

