# 🔗 SQL Server Connection String Rehberi

## 🎯 Varsayılan Yapılandırma

Proje **Windows Authentication (Integrated Security)** kullanacak şekilde yapılandırılmıştır.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

---

## 🔐 Authentication Yöntemleri

### 1. Windows Authentication (Önerilen) ✅

**Avantajlar:**
- ✅ Kullanıcı adı/şifre yönetimi yok
- ✅ Daha güvenli (Windows credentials kullanır)
- ✅ Development ortamı için ideal
- ✅ Active Directory entegrasyonu

**Connection String:**
```json
"Server=localhost;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Alternatif (aynı anlama gelir):**
```json
"Server=localhost;Database=GetirDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Gereksinimler:**
- Windows hesabınızın SQL Server'da login yetkisi olmalı
- SQL Server'ın Windows Authentication mode'da çalışması

---

### 2. SQL Server Authentication

**Kullanım Durumları:**
- Production ortamları
- Container/Docker kullanımı
- CI/CD pipelines
- Cross-platform deployment

**Connection String:**
```json
"Server=localhost;Database=GetirDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Güvenlik Önerileri:**
- ❌ appsettings.json'a şifre yazmayın
- ✅ Environment Variables kullanın
- ✅ Azure Key Vault / Secret Manager kullanın
- ✅ User Secrets kullanın (development)

---

## 🛠️ Yapılandırma Örnekleri

### Development (User Secrets)

```bash
# User secrets başlat
cd src/WebApi
dotnet user-secrets init

# Connection string ekle
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True"
```

### Production (Environment Variables)

**Linux/macOS:**
```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=GetirDb;User Id=app_user;Password=$DB_PASSWORD;TrustServerCertificate=True"
```

**Windows PowerShell:**
```powershell
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=GetirDb;User Id=app_user;Password=$DB_PASSWORD;TrustServerCertificate=True"
```

**Docker:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=sql-server;Database=GetirDb;User Id=sa;Password=${SA_PASSWORD}
```

### Azure App Service

**Configuration → Application settings:**
```
Name: ConnectionStrings__DefaultConnection
Value: Server=yourserver.database.windows.net;Database=GetirDb;User Id=admin;Password=...
```

---

## 🔍 Connection String Parametreleri

| Parametre | Açıklama | Örnek Değer |
|-----------|----------|-------------|
| `Server` | SQL Server adresi | `localhost`, `.\SQLEXPRESS`, `server.domain.com` |
| `Database` | Veritabanı adı | `GetirDb` |
| `Integrated Security` | Windows Auth kullan | `True` veya `SSPI` |
| `Trusted_Connection` | Windows Auth (alternatif) | `True` |
| `User Id` | SQL kullanıcı adı | `sa`, `app_user` |
| `Password` | SQL şifresi | `YourPassword123!` |
| `TrustServerCertificate` | SSL sertifika güvenilirliği | `True` (dev), `False` (prod) |
| `MultipleActiveResultSets` | MARS desteği | `True` |
| `Encrypt` | Bağlantı şifreleme | `True` (önerilen) |
| `Connection Timeout` | Bağlantı zaman aşımı | `30` (saniye) |

---

## 🐛 Troubleshooting

### Problem: "Login failed for user"

**Windows Authentication için:**
```sql
-- SQL Server'da Windows kullanıcısını ekle
USE master;
CREATE LOGIN [DOMAIN\Username] FROM WINDOWS;
GO

USE GetirDb;
CREATE USER [DOMAIN\Username] FOR LOGIN [DOMAIN\Username];
ALTER ROLE db_owner ADD MEMBER [DOMAIN\Username];
GO
```

**SQL Authentication için:**
```sql
-- SQL Server'da kullanıcı oluştur
USE master;
CREATE LOGIN app_user WITH PASSWORD = 'StrongPassword123!';
GO

USE GetirDb;
CREATE USER app_user FOR LOGIN app_user;
ALTER ROLE db_owner ADD MEMBER app_user;
GO
```

### Problem: "A network-related or instance-specific error"

**Çözümler:**
1. SQL Server servisinin çalıştığını kontrol edin
2. TCP/IP protokolünün enabled olduğunu kontrol edin
3. SQL Server Browser servisini başlatın
4. Firewall ayarlarını kontrol edin
5. Server name'i doğrulayın: `localhost`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`

### Problem: "The certificate chain was issued by an authority that is not trusted"

**Çözüm:**
Connection string'e ekleyin:
```
TrustServerCertificate=True
```

**Production için (önerilmez):**
```
Encrypt=False
```

---

## 🔒 Güvenlik Best Practices

### ✅ YAPILMASI GEREKENLER

1. **User Secrets kullanın (Development)**
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."
   ```

2. **Environment Variables kullanın (Production)**
   ```bash
   export ConnectionStrings__DefaultConnection="..."
   ```

3. **Azure Key Vault / AWS Secrets Manager kullanın**
   ```csharp
   builder.Configuration.AddAzureKeyVault(...);
   ```

4. **Least Privilege Principle**
   - App kullanıcısına sadece gerekli yetkileri verin
   - `sa` hesabı kullanmayın production'da

5. **Connection String Encryption**
   - appsettings.json'da asla şifre saklamayın
   - Encrypted configuration providers kullanın

### ❌ YAPILMAMASI GEREKENLER

1. ❌ appsettings.json'a şifre yazmak
2. ❌ Git'e şifre commit etmek
3. ❌ Production'da `sa` hesabı kullanmak
4. ❌ TrustServerCertificate=True (production)
5. ❌ Encrypt=False (production)

---

## 📝 Farklı SQL Server Türleri

### LocalDB
```json
"Server=(localdb)\\MSSQLLocalDB;Database=GetirDb;Integrated Security=True"
```

### SQL Express (Named Instance)
```json
"Server=.\\SQLEXPRESS;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True"
```

### SQL Express (Default Instance)
```json
"Server=localhost;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True"
```

### Azure SQL Database
```json
"Server=tcp:yourserver.database.windows.net,1433;Database=GetirDb;User Id=admin@yourserver;Password=...;Encrypt=True"
```

### Docker Container
```json
"Server=localhost,1433;Database=GetirDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
```

---

## 🧪 Test Connection

### C# Console Test:
```csharp
using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=GetirDb;Integrated Security=True;TrustServerCertificate=True";

try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Connection successful!");
    Console.WriteLine($"Server Version: {connection.ServerVersion}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Connection failed: {ex.Message}");
}
```

### sqlcmd Test:
```bash
# Windows Authentication
sqlcmd -S localhost -E -Q "SELECT @@VERSION"

# SQL Authentication
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT @@VERSION"
```

---

**Önerilen Yapılandırma:**
- **Development:** Windows Authentication + User Secrets
- **Staging:** SQL Authentication + Environment Variables
- **Production:** SQL Authentication + Azure Key Vault/AWS Secrets Manager
