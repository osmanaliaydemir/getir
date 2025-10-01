# 🐳 Docker Deployment Guide

## 🚀 Hızlı Başlangıç

### Tek Komutla Başlat

```bash
# Tüm sistemi başlat (API + Database)
docker-compose up

# Background'da çalıştır
docker-compose up -d

# Log'ları takip et
docker-compose logs -f api
```

**Sonuç:**
- ✅ SQL Server: `localhost:1433`
- ✅ API: `http://localhost:7001`
- ✅ Swagger: `http://localhost:7001`
- ✅ Health Check: `http://localhost:7001/health`

---

## 📋 Gereksinimler

### Windows
```bash
# Docker Desktop kur
https://docs.docker.com/desktop/install/windows-install/

# Kontrol et
docker --version
docker-compose --version
```

### Linux
```bash
# Docker kur
sudo apt-get update
sudo apt-get install docker.io docker-compose

# Servisi başlat
sudo systemctl start docker
sudo systemctl enable docker
```

### macOS
```bash
# Docker Desktop kur
https://docs.docker.com/desktop/install/mac-install/

# Kontrol et
docker --version
```

---

## 🏗️ Docker Yapısı

### docker-compose.yml İçeriği

```yaml
services:
  sqlserver:    # SQL Server 2022
  db-init:      # Database initialization
  api:          # Getir API
```

### Container'lar

| Container | Image | Port | Purpose |
|-----------|-------|------|---------|
| **getir-sqlserver** | mssql/server:2022 | 1433 | Database |
| **getir-db-init** | mssql-tools | - | DB Schema init |
| **getir-api** | getir-api:latest | 7001→8080 | API |

---

## 🔧 Temel Komutlar

### Başlatma
```bash
# İlk kez başlat (build + run)
docker-compose up --build

# Mevcut image'ları kullan
docker-compose up

# Background'da çalıştır
docker-compose up -d
```

### Durdurma
```bash
# Durdur (container'lar kalır)
docker-compose stop

# Durdur ve sil (volumeler kalır)
docker-compose down

# Her şeyi sil (volumeler dahil)
docker-compose down -v
```

### Yeniden Build
```bash
# Kod değişti, yeniden build et
docker-compose build api

# Cache kullanma, sıfırdan build
docker-compose build --no-cache api

# Build ve başlat
docker-compose up --build
```

### Log İzleme
```bash
# Tüm log'lar
docker-compose logs -f

# Sadece API log'ları
docker-compose logs -f api

# Son 100 satır
docker-compose logs --tail=100 api
```

### Container İçine Gir
```bash
# API container'ına shell
docker exec -it getir-api /bin/bash

# SQL Server'a bağlan
docker exec -it getir-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P GetirDb_Strong!Pass123
```

---

## 🗄️ Database Yönetimi

### Database Scripts

Container başladığında otomatik çalışır:
1. `database/schema.sql` - İlk tablolar
2. `database/schema-extensions.sql` - Ek tablolar

### Manuel Database Reset

```bash
# Container'ı durdur
docker-compose down

# Volume'u sil (database temizlenir)
docker volume rm getir-sqlserver-data

# Yeniden başlat
docker-compose up
```

### Database Backup

```bash
# Backup oluştur
docker exec getir-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P GetirDb_Strong!Pass123 \
  -Q "BACKUP DATABASE GetirDb TO DISK='/var/opt/mssql/backup/GetirDb.bak'"

# Container'dan backup'ı çıkar
docker cp getir-sqlserver:/var/opt/mssql/backup/GetirDb.bak ./GetirDb.bak
```

---

## 🔐 Environment Variables

### .env Dosyası Kullanımı

```bash
# .env.example'dan kopyala
cp .env.example .env

# .env dosyasını düzenle
nano .env
```

**.env içeriği:**
```env
SQL_SA_PASSWORD=YourStrongPassword123!
JWT_SECRET=YourSuperSecretKey...
```

**docker-compose.yml'de kullanımı:**
```yaml
environment:
  - SA_PASSWORD=${SQL_SA_PASSWORD}
```

---

## 🚀 Production Deployment

### Production Build

```bash
# Production compose kullan
docker-compose -f docker-compose.prod.yml up -d

# Scale API instances
docker-compose -f docker-compose.prod.yml up -d --scale api=3
```

### Cloud Deployment

#### Azure Container Instances
```bash
# Build & push
docker build -t getirapi.azurecr.io/getir-api:latest .
docker push getirapi.azurecr.io/getir-api:latest

# Deploy
az container create \
  --name getir-api \
  --image getirapi.azurecr.io/getir-api:latest \
  --resource-group getir-rg \
  --ports 80 443
```

#### AWS ECS
```bash
# Push to ECR
aws ecr get-login-password --region eu-west-1 | docker login --username AWS --password-stdin xxx.dkr.ecr.eu-west-1.amazonaws.com
docker tag getir-api:latest xxx.dkr.ecr.eu-west-1.amazonaws.com/getir-api:latest
docker push xxx.dkr.ecr.eu-west-1.amazonaws.com/getir-api:latest
```

---

## 🧪 Testing with Docker

### Run Tests in Docker

```bash
# Build test image
docker build --target build -t getir-tests .

# Run tests
docker run --rm getir-tests dotnet test
```

### Integration Tests with Docker

```bash
# Use docker-compose for integration tests
docker-compose -f docker-compose.test.yml up --abort-on-container-exit
```

---

## 📊 Monitoring & Debugging

### Container Status

```bash
# Çalışan container'lar
docker ps

# Tüm container'lar
docker ps -a

# Resource kullanımı
docker stats
```

### Disk Kullanımı

```bash
# Docker disk kullanımı
docker system df

# Volume'leri listele
docker volume ls

# Kullanılmayan volume'leri temizle
docker volume prune
```

### Cleanup

```bash
# Durdurulmuş container'ları sil
docker container prune

# Kullanılmayan image'ları sil
docker image prune

# Her şeyi temizle (DİKKAT!)
docker system prune -a --volumes
```

---

## 🐛 Troubleshooting

### Problem: Container başlamıyor

```bash
# Log'lara bak
docker-compose logs api

# Detaylı log
docker-compose logs --tail=500 api
```

### Problem: Database bağlantı hatası

```bash
# SQL Server sağlıklı mı?
docker exec getir-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P GetirDb_Strong!Pass123 -Q "SELECT @@VERSION"

# Network kontrolü
docker network inspect getir-network
```

### Problem: Port zaten kullanımda

```bash
# Portları değiştir (docker-compose.yml)
ports:
  - "7002:8080"  # 7001 yerine 7002
```

### Problem: Yavaş başlangıç

```bash
# Health check başlangıç süresini artır
healthcheck:
  start_period: 60s  # 40s yerine 60s
```

---

## 🔍 Useful Commands

### Image Yönetimi

```bash
# Image'ları listele
docker images

# Image sil
docker rmi getir-api:latest

# Yeniden build (no cache)
docker-compose build --no-cache
```

### Volume Yönetimi

```bash
# Volume'leri listele
docker volume ls

# Volume detayları
docker volume inspect getir-sqlserver-data

# Volume backup
docker run --rm -v getir-sqlserver-data:/data -v $(pwd):/backup alpine tar czf /backup/sqlserver-backup.tar.gz -C /data .
```

### Network Yönetimi

```bash
# Network'leri listele
docker network ls

# Network inspect
docker network inspect getir-network

# Container IP'leri
docker network inspect getir-network | grep IPv4Address
```

---

## 📊 Performance Tuning

### API Optimization

```dockerfile
# Dockerfile'da
ENV DOTNET_TieredPGO=1
ENV DOTNET_ReadyToRun=1
ENV DOTNET_EnableDiagnostics=0  # Production'da
```

### SQL Server Memory

```yaml
# docker-compose.yml'de
environment:
  - MSSQL_MEMORY_LIMIT_MB=2048
```

### Resource Limits

```yaml
deploy:
  resources:
    limits:
      cpus: '2.0'
      memory: 2G
    reservations:
      cpus: '1.0'
      memory: 1G
```

---

## 🎯 Development Workflow

### 1. İlk Kurulum
```bash
git clone https://github.com/youruser/getir.git
cd getir
docker-compose up -d
# ✅ HAZIR! (3 dakika)
```

### 2. Kod Değişikliği
```bash
# Kod değiştir
# Container'ı yeniden build et
docker-compose build api
docker-compose up -d api
```

### 3. Database Değişikliği
```bash
# Schema script düzenle
# Database'i sıfırla
docker-compose down -v
docker-compose up -d
```

### 4. Log Kontrolü
```bash
docker-compose logs -f api
```

---

## 📋 Checklist

### ✅ İlk Kullanım
- [ ] Docker Desktop kuruldu
- [ ] `.env` dosyası oluşturuldu
- [ ] `docker-compose up` çalıştırıldı
- [ ] `http://localhost:7001/health` → 200 OK
- [ ] Swagger açıldı
- [ ] Postman collection test edildi

### ✅ Production Hazırlık
- [ ] Production compose hazır
- [ ] Environment variables set
- [ ] SSL certificates hazır
- [ ] Backup strategy
- [ ] Monitoring setup
- [ ] Log aggregation

---

## 🎉 Advantages

### Development
✅ Tek komutla setup  
✅ Tutarlı environment  
✅ Hızlı onboarding (yeni developer)  
✅ Isolated testing  

### Production
✅ Same environment guarantee  
✅ Easy scaling  
✅ Zero downtime deployment  
✅ Resource control  

### Operations
✅ Easy backup/restore  
✅ Quick rollback  
✅ Container orchestration ready  
✅ Cloud-native  

---

**Docker ile development ve production arasında fark yok! 🚀**
