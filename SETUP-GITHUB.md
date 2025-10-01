# 📤 GitHub'a Gönderme Talimatları

## 🚀 Adım Adım GitHub Push

### 1. Git Bash veya Terminal Aç

**Windows:** Git Bash'i çalıştır  
**Linux/macOS:** Terminal aç

### 2. Projeye Git

```bash
cd C:\Users\osmanali.aydemir\Desktop\projects\getir
```

### 3. Git Repository Başlat

```bash
git init
```

### 4. Dosyaları Stage'e Al

```bash
git add .
```

### 5. İlk Commit

```bash
git commit -m "feat: Initial commit - Getir Clone API with Clean Architecture

- 44 REST API endpoints
- Clean Architecture (4 layers)
- JWT Authentication (Access + Refresh)
- Generic Repository + Unit of Work
- Shopping Cart with merchant constraint
- Coupon system with validation
- Transaction-based order creation
- 22 Unit Tests (100% passing)
- Docker & Docker Compose setup
- Comprehensive documentation (10 files)
- Postman collection with auto token management
- CI/CD pipeline (GitHub Actions)
- Serilog logging with correlation
- FluentValidation
- Health checks
- API versioning

Tech Stack: .NET 9, EF Core 9, SQL Server, xUnit, Moq, Docker"
```

### 6. Remote Repository Ekle

```bash
git remote add origin https://github.com/osmanaliaydemir/GetirV2.git
```

### 7. Branch Ayarla

```bash
git branch -M main
```

### 8. GitHub'a Push

```bash
git push -u origin main
```

---

## ✅ Doğrulama

GitHub'da kontrol et:
- ✅ https://github.com/osmanaliaydemir/GetirV2
- ✅ Tüm dosyalar yüklendi mi?
- ✅ README düzgün görünüyor mu?
- ✅ Docker files var mı?

---

## 📝 GitHub Repository Ayarları (Opsiyonel)

### 1. Repository Description
```
🚀 Production-ready Getir Clone API built with .NET 9, Clean Architecture, Docker, and comprehensive testing. Features 44 endpoints, JWT auth, shopping cart, coupon system, and real-time notifications.
```

### 2. Topics (Tags)
```
dotnet
clean-architecture
rest-api
docker
jwt-authentication
entity-framework-core
xunit
getir-clone
ecommerce-api
csharp
sql-server
```

### 3. Enable GitHub Actions

Settings → Actions → Allow all actions

### 4. README Preview

GitHub otomatik README.md'yi gösterecek - Swagger önizlemesi harika görünecek!

---

## 🎉 Push Sonrası

### Otomatik Tetiklenecekler:

✅ **CI/CD Pipeline** başlayacak  
✅ **Tests** otomatik çalışacak  
✅ **Build** verify edilecek  
✅ **Coverage report** oluşacak  

---

## 🐛 Sorun Giderme

### Git kurulu değilse:

**Windows:**
```
https://git-scm.com/download/win
Git Bash ile kur
```

### Permission hatası:

```bash
# SSH key kullan (daha güvenli)
ssh-keygen -t ed25519 -C "osmanali.aydemir@gmail.com"
cat ~/.ssh/id_ed25519.pub
# GitHub Settings → SSH Keys → Add
```

### Large file uyarısı:

```bash
# .gitattributes ekle
*.dll filter=lfs diff=lfs merge=lfs -text
```

---

**Git komutlarını çalıştırdıktan sonra bana haber ver, finalize işlemine geçelim!** 🎯
