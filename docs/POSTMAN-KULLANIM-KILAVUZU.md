# Getir API - Postman Kullanım Kılavuzu

Bu dokümanda Getir API'si için hazırlanan Postman koleksiyonunun nasıl kullanılacağı açıklanmaktadır.

## 📁 Dosyalar

- `Getir-API-Complete.postman_collection.json` - Ana API koleksiyonu
- `Getir-API-Environment.postman_environment.json` - Ortam değişkenleri
- `POSTMAN-KULLANIM-KILAVUZU.md` - Bu kullanım kılavuzu

## 🚀 Kurulum

### 1. Postman'i İndirin
[Postman](https://www.postman.com/downloads/) uygulamasını indirip kurun.

### 2. Koleksiyonu İçe Aktarın
1. Postman'i açın
2. "Import" butonuna tıklayın
3. `Getir-API-Complete.postman_collection.json` dosyasını seçin
4. "Import" butonuna tıklayın

### 3. Environment'ı İçe Aktarın
1. "Import" butonuna tıklayın
2. `Getir-API-Environment.postman_environment.json` dosyasını seçin
3. "Import" butonuna tıklayın
4. Sağ üst köşeden "Getir API - Geliştirme Ortamı" environment'ını seçin

## 🔧 Yapılandırma

### Environment Değişkenleri
Aşağıdaki değişkenleri environment'ta düzenleyin:

- `base_url`: API'nin base URL'i (varsayılan: https://localhost:7001)
- `access_token`: Kimlik doğrulama token'ı (login sonrası otomatik doldurulacak)
- `refresh_token`: Yenileme token'ı (login sonrası otomatik doldurulacak)

## 📋 API Kategorileri

### 🔐 Kimlik Doğrulama
- Kullanıcı kaydı
- Kullanıcı girişi
- Token yenileme
- Çıkış yapma

### 👤 Kullanıcı Yönetimi
- Adres listeleme
- Yeni adres ekleme
- Adres güncelleme
- Adres silme
- Varsayılan adres ayarlama

### 🏪 Mağaza Yönetimi
- Mağaza listeleme
- Mağaza detayı
- Yeni mağaza oluşturma
- Mağaza güncelleme
- Kategoriye göre mağaza listeleme

### 🛍️ Ürün Yönetimi
- Mağaza ürünleri
- Ürün detayı
- Yeni ürün oluşturma
- Ürün güncelleme
- Ürün silme

### 🛒 Sepet Yönetimi
- Sepeti getirme
- Sepete ürün ekleme
- Sepet öğesi güncelleme
- Sepet öğesi silme
- Sepeti temizleme

### 📦 Sipariş Yönetimi
- Yeni sipariş oluşturma
- Sipariş detayı
- Kullanıcı siparişleri

### 💳 Ödeme Yönetimi
- Ödeme oluşturma
- Ödeme detayı
- Sipariş ödemeleri
- Nakit ödeme toplama (kurye)
- Nakit ödeme başarısızlığı (kurye)

### 🚚 Kurye Yönetimi
- Kurye dashboard
- Kurye istatistikleri
- Kurye kazançları
- Atanan siparişler
- Sipariş kabul etme
- Teslimat başlatma/tamamlama
- Konum güncelleme
- Müsaitlik durumu ayarlama

### 🔍 Arama
- Ürün arama
- Mağaza arama

### 🔔 Bildirim Yönetimi
- Bildirimleri getirme
- Bildirimleri okundu olarak işaretleme
- Bildirim tercihlerini getirme/güncelleme

## 🔑 Kimlik Doğrulama

### 1. Kullanıcı Kaydı
```json
{
  "email": "test@example.com",
  "password": "Test123!",
  "firstName": "Test",
  "lastName": "User",
  "phoneNumber": "+905551234567",
  "role": "Customer"
}
```

### 2. Kullanıcı Girişi
```json
{
  "email": "test@example.com",
  "password": "Test123!"
}
```

### 3. Token Kullanımı
Giriş yaptıktan sonra `access_token` otomatik olarak environment'a kaydedilir ve diğer isteklerde kullanılır.

## 📝 Örnek İş Akışı

### 1. Kullanıcı Kaydı ve Girişi
1. "Kullanıcı Kaydı" endpoint'ini çalıştırın
2. "Kullanıcı Girişi" endpoint'ini çalıştırın
3. Token otomatik olarak environment'a kaydedilir

### 2. Adres Yönetimi
1. "Adresleri Listele" ile mevcut adresleri görün
2. "Yeni Adres Ekle" ile yeni adres ekleyin
3. "Varsayılan Adres Ayarla" ile varsayılan adresi belirleyin

### 3. Mağaza ve Ürün İnceleme
1. "Mağaza Listesi" ile mağazaları görün
2. "Mağaza Detayı" ile belirli mağazayı inceleyin
3. "Mağaza Ürünleri" ile ürünleri listeleyin

### 4. Sipariş Oluşturma
1. "Sepete Ürün Ekle" ile sepeti doldurun
2. "Sepeti Getir" ile sepeti kontrol edin
3. "Yeni Sipariş Oluştur" ile siparişi oluşturun

### 5. Ödeme İşlemi
1. "Ödeme Oluştur" ile ödeme işlemini başlatın
2. "Ödeme Detayı" ile ödeme durumunu kontrol edin

## 🛠️ Özel Ayarlar

### Test Verileri
Koleksiyonda kullanılan test verileri:
- Email: test@example.com
- Telefon: +905551234567
- Adres: Atatürk Caddesi No:123 Kadıköy/İstanbul
- Koordinatlar: 40.9884, 29.0260

### Hata Kodları
API'den dönen yaygın hata kodları:
- 400: Bad Request (Geçersiz veri)
- 401: Unauthorized (Kimlik doğrulama gerekli)
- 403: Forbidden (Yetki yok)
- 404: Not Found (Kaynak bulunamadı)
- 500: Internal Server Error (Sunucu hatası)

## 📞 Destek

API ile ilgili sorularınız için:
- GitHub Issues: [Proje Repository'si]
- Email: [Destek Email'i]
- Dokümantasyon: [API Dokümantasyon Linki]

## 🔄 Güncellemeler

Bu koleksiyon düzenli olarak güncellenmektedir. En son sürümü almak için proje repository'sini takip edin.

---

**Not**: Bu koleksiyon geliştirme ortamı için hazırlanmıştır. Production ortamında kullanmadan önce gerekli güvenlik kontrollerini yapın.
