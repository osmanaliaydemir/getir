# Getir Merchant Portal - Quick Start Guide

## 🚀 Hızlı Başlangıç (5 Dakika)

### **1. API'yi Başlat**
```bash
cd src/WebApi
dotnet run
```
✅ API: `https://localhost:7001`

### **2. Portal'ı Başlat**
```bash
cd src/MerchantPortal
dotnet run
```
✅ Portal: `https://localhost:5001` (veya konsoldaki port)

### **3. Giriş Yap**
```
URL: https://localhost:5001
Email: merchant@example.com (MerchantOwner rolü gerekli)
Password: (API'deki test merchant şifresi)
```

### **4. Dashboard Aç**
✅ Otomatik yönlendirileceksin → `/Dashboard`

---

## 📁 **Proje Yapısı (Özet)**

```
MerchantPortal/
├── 📂 Controllers/       → 6 controller
├── 📂 Services/          → 9 servis (API client)
├── 📂 Models/            → DTOs
├── 📂 Views/             → 15+ Razor view
└── 📂 wwwroot/           → Static files (CSS, JS)
```

---

## 🎯 **Ana Özellikler**

### **1. Dashboard** 📊
- Günlük/Haftalık/Aylık ciro
- Bekleyen siparişler
- Aktif ürünler
- Son siparişler
- Popüler ürünler
- **Real-time SignalR notifications** 🔔

### **2. Ürünler** 📦
- Liste (sayfalama)
- Yeni ürün ekle
- Düzenle
- Sil
- Kategori seç
- Stok yönet

### **3. Siparişler** 🛒
- Liste (filtreleme)
- Detay görüntüle
- Durum güncelle
- Timeline takip
- **Real-time updates** 🔔

### **4. Kategoriler** 🏷️
- Hierarchical tree view
- Ana/alt kategori
- CRUD işlemleri
- Expand/collapse

### **5. Profil** 🏪
- Mağaza bilgileri
- Çalışma saatleri
- Teslimat ayarları
- Logo/kapak görseli
- Bildirim tercihleri

---

## 🔔 **SignalR Real-time**

### **Dashboard'da:**
- ✅ Yeni sipariş → Toast + Sound + Tab flash
- ✅ Durum değişikliği → Toast
- ✅ Otomatik metric update

### **Order Details'da:**
- ✅ Status change → Toast + Reload

### **Connection Status:**
- 🟢 Connected
- 🟡 Connecting
- 🔴 Disconnected

---

## 🎨 **UI Highlights**

- **Getir mor** (#5D3EBC) brand color
- **Responsive** sidebar
- **Modern** Bootstrap 5
- **Icons** Font Awesome 6
- **Animations** smooth transitions
- **Mobile** friendly

---

## 📝 **Sayfa Listesi**

### **Navigation Flow:**
```
/Auth/Login
  ↓
/Dashboard (Ana Sayfa)
  ├→ /Products (Ürünler)
  │   ├→ /Products/Create
  │   └→ /Products/Edit/{id}
  │
  ├→ /Orders (Siparişler)
  │   ├→ /Orders/Pending
  │   └→ /Orders/Details/{id}
  │
  ├→ /Categories (Kategoriler)
  │   ├→ /Categories/Create
  │   └→ /Categories/Edit/{id}
  │
  └→ /Merchant/Settings (Ayarlar)
      ├→ /Merchant/Edit/{id} (Profil)
      └→ /Merchant/WorkingHours (Çalışma Saatleri)
```

---

## ⚡ **Hızlı Aksiyonlar**

### **Yeni Ürün Ekle:**
```
Sidebar → Ürünler → "Yeni Ürün Ekle" 
→ Form doldur → Kaydet
```

### **Sipariş Durumu Güncelle:**
```
Sidebar → Siparişler → Sipariş seç 
→ "Detay" → Durum butonu → Kaydet
```

### **Kategori Oluştur:**
```
Sidebar → Kategoriler → "Yeni Kategori Ekle"
→ Parent seç (opsiyonel) → Kaydet
```

### **Çalışma Saatleri:**
```
Sidebar → Ayarlar → "Çalışma Saatleri"
→ Şablon seç VEYA Manuel ayarla → Kaydet
```

---

## 🐛 **Sorun Giderme**

### **Problem: Giriş yapamıyorum**
```
Çözüm:
1. API çalışıyor mu? (https://localhost:7001)
2. Merchant hesabı var mı?
3. MerchantOwner veya Admin rolü var mı?
4. Browser console'da hata var mı?
```

### **Problem: SignalR bağlanmıyor**
```
Çözüm:
1. API'de SignalR hubs çalışıyor mu?
2. CORS ayarları doğru mu?
3. JWT token geçerli mi?
4. F12 → Console → Error mesajları
```

### **Problem: Ürün/Kategori gösterilmiyor**
```
Çözüm:
1. API'de veri var mı?
2. MerchantId doğru mu?
3. API response success mi?
4. Network tab'de 200 OK dönüyor mu?
```

---

## 📞 **Destek**

### **Loglara Bak:**
```bash
# API Logs
cd src/WebApi
dotnet run
# Console'da loglara bak

# Portal Logs
cd src/MerchantPortal
dotnet run
# Console'da loglara bak
```

### **Browser Console:**
```
F12 → Console
- SignalR connection messages
- API call responses
- JavaScript errors
```

---

## 🎯 **Sonraki Adımlar**

1. **Backend SignalR event'lerini ekle** (CRITICAL)
2. **Payment module oluştur**
3. **Production'a deploy et**
4. **Gerçek merchant'larla test et**

---

**Kolay gelsin!** 🚀

Bu guide'ı takip ederek 5 dakikada portal'ı çalıştırabilirsin!

