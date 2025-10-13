# Merchant Profil Yönetimi - Merchant Profile Management

## 🎯 Overview

Merchant'ların kendi işletme bilgilerini **tam kontrol** ile yönetebilecekleri kapsamlı profil yönetim sistemi. Logo, çalışma saatleri, teslimat ayarları ve daha fazlası!

---

## ✅ Tamamlanan Özellikler

### 1. **Profil Düzenleme** (`/Merchant/Edit`)
- ✅ **Temel Bilgiler:**
  - Mağaza adı
  - Açıklama
  - Telefon numarası
  - E-posta adresi
  - Tam adres
  - GPS koordinatları (Latitude/Longitude)

- ✅ **Teslimat Ayarları:**
  - Minimum sipariş tutarı (₺)
  - Teslimat ücreti (₺)
  - Ortalama teslimat süresi (dakika)

- ✅ **Görseller:**
  - Logo URL
  - Kapak görseli URL
  - Image preview

- ✅ **Durum Kontrolleri:**
  - IsActive (Mağaza aktif mi?)
  - IsBusy (Yoğun mu?)

### 2. **Çalışma Saatleri** (`/Merchant/WorkingHours`)
- ✅ **7 Gün Takvim:**
  - Her gün için açılış/kapanış saati
  - 24 saat açık seçeneği
  - Kapalı gün seçeneği
  
- ✅ **Hızlı Şablonlar:**
  - Hafta içi 09:00-18:00
  - Perakende 10:00-22:00
  - 7/24 açık
  
- ✅ **Toplu İşlem:**
  - Pazartesiyi tüm günlere uygula

### 3. **Ayarlar Merkezi** (`/Merchant/Settings`)
- ✅ **Hızlı Erişim Kartları:**
  - Profil bilgileri
  - Çalışma saatleri
  - Kategoriler
  - Ürünler
  
- ✅ **Bildirim Tercihleri:**
  - Yeni sipariş bildirimleri (on/off)
  - Bildirim sesi (on/off)
  - E-posta bildirimleri
  - SMS bildirimleri
  - localStorage ile kalıcı kayıt
  
- ✅ **Hesap Bilgileri:**
  - Kullanıcı adı
  - E-posta
  - Şifre değiştir (placeholder)
  
- ✅ **Sistem Bilgisi:**
  - Portal versiyonu
  - Son güncelleme tarihi
  - Durum (Online/Offline)

---

## 🏗️ Teknik İmplementasyon

### **MerchantController Actions**

```csharp
public class MerchantController : Controller
{
    // GET: /Merchant/Profile
    public IActionResult Profile()
    {
        // Redirect to Edit with merchantId
    }
    
    // GET: /Merchant/Edit/{id}
    public IActionResult Edit(Guid id)
    {
        // Security check: can only edit own merchant
        // Load merchant data (currently mock)
        // Return view with form
    }
    
    // POST: /Merchant/Edit/{id}
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateMerchantRequest model)
    {
        // Validate
        // Call API to update merchant
        // Return with success/error message
    }
    
    // GET: /Merchant/WorkingHours
    public IActionResult WorkingHours()
    {
        // Load working hours (currently mock)
        // Return 7-day schedule
    }
    
    // POST: /Merchant/UpdateWorkingHours
    [ValidateAntiForgeryToken]
    public IActionResult UpdateWorkingHours(List<UpdateWorkingHoursRequest> workingHours)
    {
        // Call API to update working hours
        // Return success message
    }
    
    // GET: /Merchant/Settings
    public IActionResult Settings()
    {
        // Show settings page
    }
}
```

### **Data Models**

```csharp
// Merchant Update
public class UpdateMerchantRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Address { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int AverageDeliveryTime { get; set; }
    public bool IsActive { get; set; }
    public bool IsBusy { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
}

// Working Hours
public class WorkingHoursResponse
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public bool IsOpen24Hours { get; set; }
}
```

---

## 🎨 UI Components

### **1. Profile Edit Page**

#### **Sections:**
```
┌─────────────────────────────────────┐
│ 📝 Temel Bilgiler                   │
│   - Mağaza Adı                      │
│   - Açıklama                        │
│   - Telefon & E-posta               │
│   - Adres                           │
│   - GPS Koordinatları               │
├─────────────────────────────────────┤
│ 🚚 Teslimat Ayarları                │
│   - Min. Sipariş Tutarı             │
│   - Teslimat Ücreti                 │
│   - Ortalama Teslimat Süresi        │
├─────────────────────────────────────┤
│ 🖼️ Görseller                        │
│   - Logo URL + Preview              │
│   - Kapak Görseli URL + Preview     │
└─────────────────────────────────────┘
```

#### **Sidebar:**
```
┌─────────────────────┐
│ 🔘 Durum            │
│   - Aktif           │
│   - Yoğun           │
├─────────────────────┤
│ 💾 Kaydet Butonu    │
│ ❌ İptal Butonu     │
├─────────────────────┤
│ 💡 İpuçları         │
└─────────────────────┘
```

### **2. Working Hours Page**

#### **Layout:**
```
┌──────────┬──────────┬──────────┬──────┬────────┐
│ Gün      │ Açılış   │ Kapanış  │ 24h  │ Kapalı │
├──────────┼──────────┼──────────┼──────┼────────┤
│ Pazartesi│ [09:00]  │ [18:00]  │ [ ]  │ [ ]    │
│ Salı     │ [09:00]  │ [18:00]  │ [ ]  │ [ ]    │
│ ...      │ ...      │ ...      │ ...  │ ...    │
└──────────┴──────────┴──────────┴──────┴────────┘

Buttons:
[📋 Pazartesiyi Tüm Günlere Uygula] [💾 Kaydet]
```

#### **Quick Templates:**
```
📅 Hızlı Şablonlar:
├─ Hafta İçi 09:00-18:00   (Mon-Fri open, Sat-Sun closed)
├─ Perakende 10:00-22:00   (All days open)
└─ 7/24 Açık               (24 hours all week)
```

### **3. Settings Page**

#### **Quick Actions:**
```
┌───────────────┐  ┌───────────────┐
│ 🏪 Profil     │  │ 🕐 Çalışma    │
│    Bilgileri  │  │    Saatleri   │
└───────────────┘  └───────────────┘

┌───────────────┐  ┌───────────────┐
│ 🏷️ Kategoriler │  │ 📦 Ürünler    │
└───────────────┘  └───────────────┘
```

#### **Notification Settings:**
```
🔔 Bildirim Tercihleri:
├─ ✅ Yeni Sipariş Bildirimleri
├─ ✅ Bildirim Sesi
├─ ☐ E-posta Bildirimleri
└─ ☐ SMS Bildirimleri
```

---

## 🔧 Features in Detail

### **1. GPS Koordinat Yönetimi**

**Neden Önemli:**
- Müşteri-mağaza arası mesafe hesaplama
- Teslimat zone belirleme
- Harita üzerinde gösterim

**Nasıl Kullanılır:**
```
1. Google Maps'te mağazanızı bulun
2. Konuma sağ tıklayın
3. Koordinatları kopyalayın
4. Form'a yapıştırın
   - Latitude: 41.008238
   - Longitude: 28.978359
```

### **2. Çalışma Saatleri Templates**

**Template 1: Hafta İçi (Weekdays)**
```javascript
Monday-Friday: 09:00 - 18:00 (Open)
Saturday-Sunday: Closed
```

**Template 2: Perakende (Retail)**
```javascript
All days: 10:00 - 22:00 (Open)
```

**Template 3: 7/24**
```javascript
All days: 00:00 - 23:59 (24 Hours)
```

### **3. Working Hours Logic**

```javascript
function toggle24Hours(checkbox, day) {
    if (checked) {
        openTime = "00:00";
        closeTime = "23:59";
        disableTimeInputs();
        uncheckClosed();
    }
}

function toggleClosed(checkbox, day) {
    if (checked) {
        openTime = "00:00";
        closeTime = "00:00";
        disableTimeInputs();
        uncheck24Hours();
    }
}
```

**Mutual Exclusion:**
- ✅ "24 Saat" seçilirse → "Kapalı" disable
- ✅ "Kapalı" seçilirse → "24 Saat" disable
- ✅ Time inputs disabled when special mode active

### **4. Notification Preferences**

**localStorage Kullanımı:**
```javascript
// Save
localStorage.setItem('merchant_notif-sound', 'true');

// Load
const soundEnabled = localStorage.getItem('merchant_notif-sound') === 'true';

// Per device, per browser
// User değiştirince reset olmaz
```

---

## 🔐 Security & Validation

### **Authorization:**
```csharp
[Authorize] // Must be logged in
public class MerchantController : Controller
{
    // Can only edit OWN merchant
    if (id != sessionMerchantId)
    {
        return Forbidden();
    }
}
```

### **Validation Rules:**
```csharp
[Required]
[StringLength(200)]
public string Name { get; set; }

[Required]
[Phone]
public string PhoneNumber { get; set; }

[EmailAddress]
public string? Email { get; set; }

[Range(0, 999999)]
public decimal MinimumOrderAmount { get; set; }

[Range(0, 999999)]
public decimal DeliveryFee { get; set; }

[Range(0, 1440)] // Max 24 hours in minutes
public int AverageDeliveryTime { get; set; }
```

---

## 📊 User Workflows

### **Workflow 1: Profil Güncelleme**

```
1. Sidebar → "Ayarlar" → "Profil Bilgileri"
   OR
   Settings sayfası → "Profil Bilgileri" kartı

2. Form gösterilir (mevcut değerler dolu)

3. Değişiklik yap:
   - Mağaza adını güncelle
   - Telefonu güncelle
   - Teslimat ücretini değiştir
   - Logo URL ekle

4. "Değişiklikleri Kaydet" tıkla

5. API'ye gönderilir
   ↓
   Success: ✅ "Profil başarıyla güncellendi"
   Error: ❌ "Profil güncellenirken hata oluştu"

6. Aynı sayfada kal (değişiklikler kayıtlı)
```

### **Workflow 2: Çalışma Saatleri Ayarlama**

```
1. "Ayarlar" → "Çalışma Saatleri" kartı

2. 7 günlük tablo gösterilir

3. Option A: Manuel ayarlama
   - Her gün için ayrı ayrı
   - Açılış: [09:00]
   - Kapanış: [18:00]
   - 24 Saat: [ ]
   - Kapalı: [ ]

4. Option B: Template kullan
   - "Hafta İçi 09:00-18:00" → Click
   - Otomatik doldurulur

5. Option C: Pazartesiyi kopyala
   - Pazartesi'yi ayarla
   - "Tüm Günlere Uygula" → Click
   - Tüm günler aynı olur

6. "Çalışma Saatlerini Kaydet"

7. ✅ Success toast
```

### **Workflow 3: Bildirim Tercihleri**

```
1. "Ayarlar" sayfası

2. "Bildirim Tercihleri" bölümü

3. Toggle switches:
   [✅] Yeni Sipariş Bildirimleri
   [✅] Bildirim Sesi
   [ ] E-posta Bildirimleri
   [ ] SMS Bildirimleri

4. Değişiklik yap → Otomatik kaydedilir
   ↓
   localStorage'a yazılır
   ↓
   ✅ "Bildirim tercihleri kaydedildi" toast
```

---

## 🎨 UI/UX Features

### **Profile Page Design**

**Layout:**
```
┌─────────────────────────┬─────────────┐
│ Temel Bilgiler (Form)   │ Durum Panel │
├─────────────────────────┤             │
│ Teslimat Ayarları       │ Kaydet      │
├─────────────────────────┤             │
│ Görseller + Preview     │ İpuçları    │
└─────────────────────────┴─────────────┘
```

**Color Coding:**
```
Form Groups: White cards with shadow
Alerts: Info blue (#e3f2fd)
Submit: Getir purple (#5D3EBC)
Cancel: Secondary gray (#6c757d)
```

### **Working Hours Visual**

```
┌──────────────────────────────────────┐
│ Pazartesi  [09:00] [18:00] [24h] [X] │ ← Row border, hover effect
│ Salı       [09:00] [18:00] [24h] [X] │
│ ...                                   │
└──────────────────────────────────────┘
```

**Interactive States:**
- Default: Light gray background
- Hover: Border purple
- 24h enabled: Time inputs disabled
- Closed: Time inputs disabled, gray out

### **Settings Quick Actions**

```
┌────────────────────────────┐
│  [Icon] Profil Bilgileri   │ ← Hover: Purple border
│  İsim, adres, iletişim     │   Transform: translateY(-2px)
│                      →     │   Shadow: 0 4px 12px
└────────────────────────────┘
```

---

## 📡 API Integration

### **Endpoints Used:**

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/merchant/{id}` | Get merchant details |
| PUT | `/api/v1/merchant/{id}` | Update merchant |
| GET | `/api/v1/workinghours/merchant/{id}` | Get working hours |
| POST | `/api/v1/workinghours/bulk-upsert` | Update working hours |

### **Request Example:**

```json
PUT /api/v1/merchant/{id}
{
  "name": "Migros MMM",
  "description": "Kaliteli ve uygun fiyatlı alışveriş",
  "address": "Atatürk Cad. No:123, Kadıköy, İstanbul",
  "latitude": 40.9897,
  "longitude": 29.0254,
  "phoneNumber": "+90 555 123 4567",
  "email": "info@migros.com",
  "minimumOrderAmount": 50.00,
  "deliveryFee": 9.99,
  "averageDeliveryTime": 30,
  "isActive": true,
  "isBusy": false,
  "logoUrl": "https://cdn.getir.com/logos/migros.png",
  "coverImageUrl": "https://cdn.getir.com/covers/migros-cover.jpg"
}
```

---

## 🧪 Test Scenarios

### **Test 1: Profil Güncelleme**
```
Given: Merchant logged in
And: MerchantId in session

When: Navigate to /Merchant/Edit/{merchantId}
Then: Form displayed with current values

When: Change "Name" to "Yeni Mağaza Adı"
And: Click "Kaydet"
Then: API called with PUT request
And: Success toast shown
And: Form values updated
```

### **Test 2: Çalışma Saatleri - Template**
```
Given: Working hours page open

When: Click "Hafta İçi 09:00-18:00"
Then:
  - Monday-Friday: 09:00 - 18:00
  - Saturday-Sunday: Closed (checked)
  - Success toast shown
```

### **Test 3: 24 Saat Toggle**
```
Given: Working hours for Monday shown

When: Check "24 Saat" checkbox
Then:
  - Open time: 00:00 (disabled)
  - Close time: 23:59 (disabled)
  - "Kapalı" unchecked and disabled
```

### **Test 4: Notification Preference**
```
Given: Settings page open

When: Toggle "Bildirim Sesi" OFF
Then:
  - localStorage updated
  - Toast: "Bildirim tercihleri kaydedildi"
  
When: Reload page
Then:
  - "Bildirim Sesi" still OFF
  - Preference persisted
```

---

## 🐛 Known Limitations

### **Current Limitations:**

1. **Mock Data:**
   - ⚠️ Profil düzenleme şu an demo data gösteriyor
   - ⚠️ Gerçek API entegrasyonu gerekli
   - **Fix:** Backend'den merchant fetch et

2. **File Upload:**
   - ⚠️ Sadece URL girişi var
   - ⚠️ Direkt file upload yok
   - **Fix:** File upload service entegre et

3. **Working Hours API:**
   - ⚠️ Frontend hazır, backend call eksik
   - **Fix:** WorkingHoursService API call ekle

4. **Notification Settings:**
   - ⚠️ Sadece localStorage (local)
   - ⚠️ Backend'e kaydedilmiyor
   - **Fix:** Backend'e notification preference API'si

---

## 🚀 Future Enhancements

### **Phase 1: API Integration** (1-2 saat)
```csharp
// MerchantController.Edit (GET)
var merchant = await _merchantService.GetMerchantByIdAsync(id);
var model = MapToUpdateRequest(merchant);

// MerchantController.WorkingHours (GET)
var workingHours = await _workingHoursService.GetByMerchantIdAsync(merchantId);

// MerchantController.UpdateWorkingHours (POST)
await _workingHoursService.BulkUpdateAsync(merchantId, workingHours);
```

### **Phase 2: File Upload** (2-3 saat)
```html
<input type="file" id="logo-upload" accept="image/*" />
<button onclick="uploadLogo()">Upload</button>

<script>
async function uploadLogo() {
    const formData = new FormData();
    formData.append('file', fileInput.files[0]);
    
    const response = await fetch('/api/v1/fileupload', {
        method: 'POST',
        body: formData
    });
    
    const result = await response.json();
    document.getElementById('LogoUrl').value = result.url;
}
</script>
```

### **Phase 3: Advanced Features** (3-4 saat)
- Image cropping (before upload)
- Image compression
- Drag & drop upload
- Multiple image gallery
- CDN integration

---

## 💡 Best Practices

### **1. Form Validation**
```csharp
✅ Client-side: jQuery Validation (immediate feedback)
✅ Server-side: Model validation (security)
✅ Business rules: Backend validation
```

### **2. User Experience**
```
✅ Auto-fill with current data
✅ Image preview before save
✅ Clear error messages
✅ Success feedback (toast)
✅ Help text & tooltips
```

### **3. Data Management**
```
✅ Session-based merchant ID
✅ Security check (can't edit others)
✅ AntiForgeryToken on POST
✅ ModelState validation
```

---

## 📱 Mobile Responsive

### **Breakpoints:**

**Desktop (>768px):**
- 2-column layout (8-4 split)
- Full sidebar visible
- Large form inputs

**Tablet (768px):**
- 2-column maintained
- Sidebar collapsible
- Medium form inputs

**Mobile (<576px):**
- Single column
- Stacked cards
- Full-width buttons
- Touch-friendly inputs

---

## 🎯 Integration Points

### **1. Dashboard Link:**
```
Dashboard → User Avatar → "Profil Ayarları"
(Not implemented yet, add to user menu)
```

### **2. Sidebar Link:**
```
Sidebar → "Ayarlar" → /Merchant/Settings
✅ Already implemented
```

### **3. Product Create:**
```
Products → Create → "Logo URL"
← Auto-filled from merchant.LogoUrl
(Enhancement opportunity)
```

---

## 📚 Documentation Files

Created:
- ✅ `MERCHANT-PROFILE-MANAGEMENT.md` (This file)
- ✅ Updated `README.md`
- ✅ Updated `TODO.md`

---

**✨ Merchant Profil Yönetimi: TAMAMLANDI!**

Merchant'lar artık:
- ✅ Profil bilgilerini düzenleyebilir
- ✅ Çalışma saatlerini ayarlayabilir
- ✅ Teslimat ayarlarını değiştirebilir
- ✅ Logo ve kapak görseli ekleyebilir
- ✅ Bildirim tercihlerini yönetebilir
- ✅ Hızlı şablonlar kullanabilir

**Backend entegrasyonu sonrası %100 functional!**

