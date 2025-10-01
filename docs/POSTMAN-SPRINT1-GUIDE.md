# 📮 Postman Collection - Sprint 1 Kullanım Kılavuzu

## 🎯 Genel Bakış

Sprint 1'de **Role-Based Authorization** sistemi eklendi. Bu kılavuz, yeni Postman collection'ın nasıl kullanılacağını açıklar.

## 📥 Kurulum

### 1. Collection'ı İçe Aktar

```bash
# Postman'de:
File → Import → docs/Getir-API-v2.postman_collection.json
```

### 2. Environment Oluştur (Opsiyonel)

Postman'de yeni bir environment oluşturup aşağıdaki değişkenleri ekleyebilirsiniz:

```
baseUrl: https://localhost:7001
categoryId: {existing-category-guid}
```

---

## 🚀 Hızlı Başlangıç

### Adım 1: API'yi Başlat

```bash
cd src/WebApi
dotnet run
```

### Adım 2: Database Migration'ı Çalıştır

```bash
sqlcmd -S OAA\MSSQLSERVER2014 -E -i database/migrations/001_add_roles_and_owner.sql
```

### Adım 3: Test Senaryolarını Çalıştır

Collection'da 3 ana bölüm var:

1. **🔐 Authentication (Sprint 1 Updated)** - 7 endpoint
2. **🏪 Merchants (Sprint 1 Updated)** - 5 endpoint  
3. **📋 Role Test Scenarios** - 3 test

---

## 👥 Kullanıcı Rolleri

### 1. Customer (Müşteri) - Role: 1
```http
POST /api/v1/auth/register
{
  "email": "customer@test.com",
  "password": "Test123!",
  "firstName": "Ali",
  "lastName": "Demir",
  "phoneNumber": "+905551234567"
  // role belirtilmezse otomatik Customer olur
}
```

**Yetkiler:**
- ✅ Sipariş verebilir
- ✅ Sepet yönetimi
- ❌ Merchant oluşturamaz
- ❌ Ürün ekleyemez

### 2. MerchantOwner (İşletme Sahibi) - Role: 2
```http
POST /api/v1/auth/register
{
  "email": "owner@migros.com",
  "password": "Test123!",
  "firstName": "Mehmet",
  "lastName": "Çelik",
  "phoneNumber": "+905551234568",
  "role": 2
}
```

**Yetkiler:**
- ✅ Merchant oluşturabilir
- ✅ Kendi merchant'ını güncelleyebilir
- ✅ Kendi ürünlerini yönetebilir
- ❌ Başkasının merchant'ını düzenleyemez
- ❌ Merchant silemez

### 3. Courier (Kurye) - Role: 3
```http
POST /api/v1/auth/register
{
  "email": "courier@getir.com",
  "password": "Test123!",
  "firstName": "Ayşe",
  "lastName": "Yılmaz",
  "phoneNumber": "+905551234569",
  "role": 3
}
```

**Yetkiler:**
- ✅ Sipariş teslimatı
- ✅ Konum güncellemesi
- ❌ Merchant işlemleri yapamaz

### 4. Admin (Yönetici) - Role: 4
```http
POST /api/v1/auth/register
{
  "email": "admin@getir.com",
  "password": "Test123!",
  "firstName": "Admin",
  "lastName": "User",
  "phoneNumber": "+905551234570",
  "role": 4
}
```

**Yetkiler:**
- ✅ TÜM yetkilere sahip
- ✅ Merchant silebilir
- ✅ Tüm merchant'ları güncelleyebilir

---

## 📝 Test Senaryoları

### Senaryo 1: Tam İş Akışı (MerchantOwner)

#### 1. MerchantOwner Olarak Kayıt Ol
```
Request: 2. Register MerchantOwner
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-10-01T12:00:00Z",
  "role": 2,
  "userId": "abc-123-def",
  "email": "owner@migros.com",
  "fullName": "Mehmet Çelik"
}
```

✅ **Otomatik Kaydedilen Değişkenler:**
- `merchantOwnerToken`
- `merchantOwnerId`

#### 2. Category ID'yi Manuel Set Et
```
# Postman Variables'da:
categoryId = {existing-guid}
```

#### 3. Merchant Oluştur
```
Request: Create Merchant (MerchantOwner or Admin)
Token: {{merchantOwnerToken}} (otomatik)
```

**Response:**
```json
{
  "id": "merchant-guid",
  "ownerId": "abc-123-def",
  "ownerName": "Mehmet Çelik",
  "name": "Migros Kadıköy",
  // ... diğer alanlar
}
```

✅ **Otomatik Kaydedilen Değişkenler:**
- `merchantId`

#### 4. Kendi Merchant'ını Güncelle
```
Request: Update Merchant (Owner or Admin)
Token: {{merchantOwnerToken}}
```

✅ **Başarılı** - 200 OK

#### 5. Merchant'ı Silmeye Çalış
```
Request: Delete Merchant (Admin Only)
Token: {{merchantOwnerToken}}
```

❌ **Başarısız** - 403 Forbidden (Sadece Admin silebilir)

---

### Senaryo 2: Yetki Testleri (Customer vs MerchantOwner)

#### 1. Customer Olarak Kayıt Ol
```
Request: 1. Register Customer (Default)
```

#### 2. Merchant Oluşturmaya Çalış
```
Request: Test: Customer tries to create Merchant
Token: {{accessToken}} (Customer token)
```

**Beklenen Sonuç:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

✅ Test geçti - Customer merchant oluşturamaz

#### 3. MerchantOwner Olarak Login Yap
```
Request: 5. Login
Body: { "email": "owner@migros.com", "password": "Test123!" }
```

#### 4. Merchant Oluştur
```
Request: Create Merchant (MerchantOwner or Admin)
Token: {{merchantOwnerToken}} (Login'den gelen)
```

✅ Başarılı - MerchantOwner oluşturabilir

---

### Senaryo 3: Admin Yetkiler

#### 1. Admin Olarak Kayıt Ol
```
Request: 4. Register Admin
```

#### 2. Herhangi Bir Merchant'ı Sil
```
Request: Delete Merchant (Admin Only)
Token: {{adminToken}}
```

✅ Başarılı - Sadece Admin silebilir

---

## 🔍 Response Değişiklikleri

### Auth Response (Register/Login)

**Önce (Sprint 1 Öncesi):**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-10-01T12:00:00Z"
}
```

**Sonra (Sprint 1):**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "expiresAt": "2025-10-01T12:00:00Z",
  "role": 2,                        // ✨ YENİ
  "userId": "abc-123-def",          // ✨ YENİ
  "email": "owner@migros.com",      // ✨ YENİ
  "fullName": "Mehmet Çelik"        // ✨ YENİ
}
```

### Merchant Response

**Önce:**
```json
{
  "id": "guid",
  "name": "Migros",
  // ...
}
```

**Sonra:**
```json
{
  "id": "guid",
  "ownerId": "user-guid",           // ✨ YENİ
  "ownerName": "Mehmet Çelik",      // ✨ YENİ
  "name": "Migros",
  // ...
}
```

---

## 🔧 Environment Variables

Collection otomatik olarak şu değişkenleri yönetir:

### Genel
- `baseUrl` - API base URL (default: https://localhost:7001)
- `categoryId` - **MANuel set edilmeli**

### Customer (Default User)
- `accessToken` - Customer access token
- `refreshToken` - Customer refresh token
- `userId` - Customer user ID
- `userEmail` - Customer email
- `userRole` - Customer role (1)
- `fullName` - Customer full name

### MerchantOwner
- `merchantOwnerToken` - MerchantOwner access token
- `merchantOwnerId` - MerchantOwner user ID

### Courier
- `courierToken` - Courier access token
- `courierId` - Courier user ID

### Admin
- `adminToken` - Admin access token
- `adminId` - Admin user ID

### Entities
- `merchantId` - Created merchant ID
- `productId` - Created product ID

---

## 📊 Test Scripts

Collection, her request'te otomatik testler çalıştırır:

### Register Test Script
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.environment.set('accessToken', response.accessToken);
    pm.environment.set('userId', response.userId);
    pm.environment.set('userRole', response.role);
    
    pm.test('Registration successful', () => {
        pm.expect(response.role).to.be.a('number');
        pm.expect(response.userId).to.be.a('string');
    });
}
```

### Create Merchant Test Script
```javascript
if (pm.response.code === 200) {
    const response = pm.response.json();
    pm.environment.set('merchantId', response.id);
    
    pm.test('Merchant has owner', () => {
        pm.expect(response.ownerId).to.be.a('string');
        pm.expect(response.ownerName).to.be.a('string');
    });
} else if (pm.response.code === 403) {
    console.log('❌ Forbidden: Check user role');
}
```

---

## 🎯 Kullanım İpuçları

### 1. Doğru Token'ı Kullan
```
Customer işlemleri      → {{accessToken}}
MerchantOwner işlemleri → {{merchantOwnerToken}}
Courier işlemleri       → {{courierToken}}
Admin işlemleri         → {{adminToken}}
```

### 2. Category ID'yi Ayarla
İlk kullanımda bir category oluştur ve ID'sini kaydet:
```http
POST /api/v1/categories
{
  "name": "Market",
  "description": "Günlük ihtiyaç ürünleri",
  "displayOrder": 1
}

Response: { "id": "category-guid" }

# Postman'de manuel set et:
categoryId = category-guid
```

### 3. Console Log'ları Takip Et
Her request'te console'da detaylı bilgi gösterilir:
```
✅ Customer registered - Role: 1 - UserId: abc-123
✅ Logged in as: MerchantOwner - UserId: def-456
✅ Merchant created - OwnerId: def-456 - OwnerName: Mehmet Çelik
❌ Forbidden: Only Admin can delete merchants
```

### 4. Test Scenarios'ı Çalıştır
"📋 Role Test Scenarios" folder'ındaki testler otomatik olarak yetki kontrollerini test eder.

---

## ❌ Sık Karşılaşılan Hatalar

### 1. 403 Forbidden
**Sebep:** Yetkisiz erişim  
**Çözüm:** Doğru role sahip kullanıcıyla login olun

```bash
# Merchant oluşturmak için:
Register → MerchantOwner (role: 2) veya Admin (role: 4)

# Merchant silmek için:
Register → Admin (role: 4)
```

### 2. NOT_FOUND_CATEGORY
**Sebep:** `categoryId` değişkeni set edilmemiş  
**Çözüm:** Önce bir category oluşturun ve ID'sini kaydedin

### 3. FORBIDDEN_NOT_OWNER
**Sebep:** Başkasının merchant'ını güncellemeye çalışıyorsunuz  
**Çözüm:** Sadece kendi merchant'ınızı güncelleyebilirsiniz (veya Admin olun)

---

## 📋 Örnek Test Akışı

### Komple Test Sequence
```
1. ✅ Register Admin
2. ✅ Register MerchantOwner  
3. ✅ Register Customer
4. ✅ Create Category (Admin token)
5. ✅ Create Merchant (MerchantOwner token)
6. ❌ Try Create Merchant (Customer token) → 403 Forbidden
7. ✅ Update Merchant (MerchantOwner token)
8. ❌ Try Update Merchant (Customer token) → 403 Forbidden
9. ✅ Delete Merchant (Admin token)
10. ❌ Try Delete Merchant (Customer token) → 403 Forbidden
```

---

## 🎉 Özet

✅ **Yeni Özellikler:**
- 4 farklı role ile kayıt olma
- Role-based endpoint authorization
- Otomatik token yönetimi
- Owner bilgisi merchant response'larında
- Test scenarios

✅ **Otomatik İşlemler:**
- Token'lar otomatik kaydediliyor
- UserId, role, email environment'a set ediliyor
- Merchant ID otomatik kaydediliyor
- Test sonuçları console'da gösteriliyor

---

**Sprint 1 Postman Collection hazır! 🚀**

İyi testler! 🎯

