# 🐳 Docker Quick Start - 3 Dakikada Başla!

## ⚡ Super Quick Start

```bash
# 1. Repository'yi klonla
git clone https://github.com/youruser/getir.git
cd getir

# 2. Docker ile başlat
docker-compose up -d

# 3. Tarayıcıda aç
http://localhost:7001
```

**HAZIR! ✅** API çalışıyor, database hazır, Swagger açık!

---

## 📊 Ne Oluyor?

### 3 Container Ayağa Kalkıyor:

1. **🗄️ SQL Server 2022**
   - Port: 1433
   - Database: GetirDb
   - Auto-initialized with schema

2. **🔧 DB Initializer**
   - Runs once
   - Creates tables
   - Seeds test data
   - Exits after completion

3. **🚀 Getir API**
   - Port: 7001
   - Swagger: Enabled
   - Connected to SQL Server
   - Health checks ready

---

## 🎯 İlk Test

### 1. Health Check
```bash
curl http://localhost:7001/health
```

**Beklenen:**
```json
{
  "status": "Healthy",
  "checks": [...]
}
```

### 2. Swagger UI
```
http://localhost:7001
```

### 3. Postman
```
Import: docs/Getir-API.postman_collection.json
Base URL: http://localhost:7001
Test: Register → Login → Create Order
```

---

## 🛑 Durdurma

```bash
# Container'ları durdur (veriler kalır)
docker-compose stop

# Container'ları sil (veriler kalır)
docker-compose down

# Her şeyi sil (veriler dahil)
docker-compose down -v
```

---

## 🔄 Kod Değişikliği Yaptıktan Sonra

```bash
# API'yi yeniden build et
docker-compose build api

# Yeni version'u başlat
docker-compose up -d api

# Log'ları izle
docker-compose logs -f api
```

---

## 🐛 Sorun Giderme

### Problem: Port 7001 kullanımda

```yaml
# docker-compose.yml'de port değiştir
services:
  api:
    ports:
      - "7002:8080"  # 7001 yerine 7002
```

### Problem: SQL Server başlamıyor

```bash
# Log'lara bak
docker-compose logs sqlserver

# Yeniden başlat
docker-compose restart sqlserver
```

### Problem: Database boş

```bash
# Yeniden initialize et
docker-compose down -v
docker-compose up -d
```

---

## 💡 Yararlı Komutlar

```bash
# Container'ları listele
docker ps

# API log'larını izle
docker-compose logs -f api

# SQL Server'a bağlan
docker exec -it getir-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P GetirDb_Strong!Pass123

# Container'ı yeniden başlat
docker-compose restart api

# Disk kullanımı
docker system df
```

---

## 🎉 Başarı Kriterleri

✅ `docker ps` → 2 container çalışıyor (sqlserver, api)  
✅ `http://localhost:7001/health` → Status: Healthy  
✅ `http://localhost:7001` → Swagger UI açılıyor  
✅ Postman → Register endpoint çalışıyor  

---

## 📖 Daha Fazla Bilgi

- **Full Docker Guide:** [DOCKER-GUIDE.md](DOCKER-GUIDE.md)
- **Production Deployment:** docker-compose.prod.yml
- **Environment Variables:** env.example

---

**Docker ile 3 dakikada hazır! 🚀**
