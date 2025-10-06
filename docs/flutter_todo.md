## Yapılacaklar Listesi (Flutter Mobile)

### Kritik (önce)
- [x] Ortam yapılandırması (env): `--dart-define` ile baseUrl/dev-stage-prod yönetimi; `ApiClient` baseUrl buradan okunsun
- [x] Notifications API: `NotificationDataSource` (list, read), `NotificationRepositoryImpl`, listeleme BLoC/ekranı
- [x] FCM token server sync: login/refresh sonrası token’ı API’ye gönder, yenilemede güncelle
- [x] SignalR → BLoC köprüsü: order tracking hub eventlerini `OrderBloc`’a map et (status updates)
- [x] Deep-link/notification tap routing: orderId varsa detay/track route’a yönlendirme

### Yüksek
- [x] Görsel optimizasyonu yaygınlaştırma: `Image.network` kullanılan yerleri `OptimizedImage` ile değiştir
- [x] Cart merge politikası: local→server item merge (aynı ürün miktar toplama, farklı merchant ayrı satır) data source seviyesinde uygula
- [x] gen-l10n çıktısı: geçici `GeneratedLocalizations` shim’i kaldır, gerçek `flutter gen-l10n` çıktısıyla çalıştır

### Test ve DevOps
- [ ] Unit/Widget/BLoC testleri: Auth, Cart, Product, Order usecase + BLoC
- [ ] CI/CD: format, analyze, test, build (Android/iOS) ve artifact upload pipeline

### Güvenlik
- [x] Certificate pinning: temel doğrulama (host allowlist) — gerçek fingerprint pinleme için altyapı hazır
- [x] Token’ı secure storage’da saklama; opsiyonel screenshot/root/jailbreak denetimleri (kısmi)

### Bildirim Stratejisi (opsiyonel)
- [ ] Topic abonelikleri: order/promotion/system için subscribe/unsubscribe politikası


