# Merchant Portal – API Entegrasyon Yol Haritası

Bu plan `analysis/endpoint-analysis.json` çıktısındaki eksik Portal entegrasyonlarına göre hazırlanmıştır. Sprintler 1 haftalık varsayılmıştır.

## Sprint 1 – Yüksek Öncelik (Temel Operasyonlar)

- [x] **Merchant Belge Yönetimi**
  - `GET api/MerchantDocument` → Belge listesi sayfası
  - `GET api/MerchantDocument/{id}` & `/download` → Detay + indirme
  - `GET api/MerchantDocument/required-types` → Belge türleri konfigürasyonu
  - `POST api/MerchantDocument/{id}/verify` & `/bulk-verify` → Onay/Toplu onay akışı
- [x] **Merchant CRUD Eksikleri**
  - `GET api/v1/Merchant` & `/active/by-category-type/{categoryType}`
  - `GET/DELETE api/v1/Merchant/{id}`
  - `POST api/v1/Merchant`
  - Portal UI/rol modeli ile uyumlu yetki kontrolü ekle.
- [x] **Payment – Courier/Admin Senaryoları (Seçmeli)**
  - `GET api/v1/Payment/courier/pending` & `/summary`
  - `POST api/v1/Payment/courier/{paymentId}/collect|fail`
  - Merchant’ın kurye tahsilatlarını izlemesi gerekiyorsa yeni ekran tasarla.

## Sprint 2 – Orta Öncelik (Operasyonel Genişleme)

- [x] **Merchant Dashboard Genişletme**
  - Kullanılmayan `MerchantDashboard` endpoint’lerini (ör. export/metrics) dahil et.
- [x] **Stok & Uyarı Tamamlamaları**
  - `StockManagement` ve `StockAlert` altında portalda olmayan kontrolleri bağla.
- [x] **Special Holiday & Notification Tarihçesi**
  - `GET/POST/PUT/DELETE api/v1/SpecialHoliday` → Tatil günleri UI.
  - `GET api/v1/Notification/*` → Bildirim geçmişi ekranı.

## Sprint 3 – Düşük Öncelik / Nice-to-have

- [x] **İleri Analitik & Raporlama**
  - [x] `Inventory` için envanter analitiği ve değerleme raporları
  - [x] `FileUpload` için dosya yönetimi paneli
  - [x] `Review` / `ProductReview` tarafında gelişmiş metrikler ve trend ekranları
- [x] **Uluslararasılaştırma & Rate Limit Araçları**
  - `InternationalizationController` & `RateLimitController` entegrasyonu; gerekirse sadece admin araçları için planla.
- [x] **Realtime Tracking / Courier Araçları**
  - Merchant Portal’da gerçek zamanlı kurye/sipariş takibi gerekiyorsa `RealtimeTrackingController` uçlarını kullanacak modül tasarla.

## Teknik Notlar

- Yeni entegrasyonlar için servis katmanında ilgili HTTP çağrıları açılmalı, ardından Controller/View tarafında UI güncellenmeli.
- Her sprintte eklenen endpointler için birim test + entegrasyon testleri yazılmalı.
- Gereksiz bulunan endpointleri (Portal kapsamı dışı) ayrı bir dokümana (ör. `docs/UNUSED_API_ENDPOINTS_ANALYSIS.md`) not ederek takip et.


