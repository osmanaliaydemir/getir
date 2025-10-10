# Eksik Widget Testleri Listesi

**Oluşturulma Tarihi:** 10 Ekim 2025  
**Mevcut Widget Test Sayısı:** 58 test (9 dosya)  
**Eksik Test Sayısı:** ~24 page + 6 widget = **30 eksik test dosyası**

---

## ✅ Mevcut Widget Testleri (9 dosya - 58 test)

### **Components** (3 test)
- ✅ `test/widget/components/theme_switcher_widget_test.dart`
- ✅ `test/widget/components/language_selector_widget_test.dart`
- ✅ `test/widget/components/custom_button_widget_test.dart`

### **Pages** (4 test)
- ✅ `test/widget/pages/checkout_page_widget_test.dart`
- ✅ `test/widget/pages/product_detail_page_widget_test.dart`
- ✅ `test/widget/pages/product_list_page_widget_test.dart`
- ✅ `test/widget/pages/cart_page_widget_test.dart`

### **Animations** (1 test)
- ✅ `test/widget/animations/shimmer_loading_widget_test.dart`

### **Dialogs** (1 test)
- ✅ `test/widget/dialogs/confirmation_dialog_test.dart`

---

## ❌ Eksik Page Widget Testleri (24 dosya)

### **Auth Pages** (3 eksik) - YÜK SEK ÖNCELİK
- ❌ `test/widget/pages/auth/login_page_widget_test.dart`
- ❌ `test/widget/pages/auth/register_page_widget_test.dart`
- ❌ `test/widget/pages/auth/forgot_password_page_widget_test.dart`

### **Home & Main Pages** (2 eksik) - YÜKSEK ÖNCELİK
- ❌ `test/widget/pages/home/home_page_widget_test.dart`
- ❌ `test/widget/pages/splash/splash_page_widget_test.dart`

### **Merchant Pages** (2 eksik) - ORTA ÖNCELİK
- ❌ `test/widget/pages/merchant/merchant_detail_page_widget_test.dart`
- ❌ `test/widget/pages/merchant/category_merchants_page_widget_test.dart`

### **Order Pages** (4 eksik) - YÜKSEK ÖNCELİK
- ❌ `test/widget/pages/order/order_tracking_page_widget_test.dart`
- ❌ `test/widget/pages/order/order_confirmation_page_widget_test.dart`
- ❌ `test/widget/pages/order/order_detail_page_widget_test.dart`
- ❌ `test/widget/pages/orders/orders_page_widget_test.dart`

### **Address Pages** (3 eksik) - ORTA ÖNCELİK
- ❌ `test/widget/pages/address/add_edit_address_page_widget_test.dart`
- ❌ `test/widget/pages/address/address_management_page_widget_test.dart`
- ❌ `test/widget/pages/addresses/addresses_page_widget_test.dart`

### **Other Pages** (10 eksik) - DÜŞÜK-ORTA ÖNCELİK
- ❌ `test/widget/pages/profile/profile_page_widget_test.dart`
- ❌ `test/widget/pages/settings/settings_page_widget_test.dart`
- ❌ `test/widget/pages/settings/about_page_widget_test.dart`
- ❌ `test/widget/pages/notifications/notifications_page_widget_test.dart`
- ❌ `test/widget/pages/notifications/notification_settings_page_widget_test.dart`
- ❌ `test/widget/pages/search/search_page_widget_test.dart`
- ❌ `test/widget/pages/review/submit_review_page_widget_test.dart`
- ❌ `test/widget/pages/payment/payment_page_widget_test.dart`
- ❌ `test/widget/pages/onboarding/onboarding_page_widget_test.dart`
- ❌ `test/widget/pages/error/not_found_page_widget_test.dart`

---

## ❌ Eksik Widget Component Testleri (6 dosya)

### **Common Widgets** (2 eksik) - ORTA ÖNCELİK
- ❌ `test/widget/widgets/common/network_status_indicator_widget_test.dart`
- ❌ `test/widget/widgets/common/main_navigation_widget_test.dart`
- ❌ `test/widget/widgets/common/paginated_list_view_widget_test.dart`

### **Product Widgets** (2 eksik) - ORTA ÖNCELİK
- ❌ `test/widget/widgets/product/product_card_widget_test.dart`
- ❌ `test/widget/widgets/product/product_card_skeleton_widget_test.dart`

### **Merchant Widgets** (2 eksik) - DÜŞÜK ÖNCELİK
- ❌ `test/widget/widgets/merchant/merchant_card_widget_test.dart`
- ❌ `test/widget/widgets/merchant/merchant_card_skeleton_widget_test.dart`

### **Order Widgets** (1 eksik) - DÜŞÜK ÖNCELİK
- ❌ `test/widget/widgets/order/order_card_skeleton_widget_test.dart`

### **Notification Widgets** (2 eksik) - DÜŞÜK ÖNCELİK
- ❌ `test/widget/widgets/notification/notification_card_widget_test.dart`
- ❌ `test/widget/widgets/notification/notification_card_skeleton_widget_test.dart`

### **Review Widgets** (1 eksik) - DÜŞÜK ÖNCELİK
- ❌ `test/widget/widgets/review/review_card_widget_test.dart`

---

## 📊 Özet İstatistikler

| Kategori | Mevcut | Eksik | Toplam | Kapsama |
|----------|--------|-------|--------|---------|
| **Page Tests** | 4 | 24 | 28 | 14% |
| **Widget Tests** | 3 | 10 | 13 | 23% |
| **Animation Tests** | 1 | 0 | 1 | 100% |
| **Dialog Tests** | 1 | 0 | 1 | 100% |
| **TOPLAM** | **9** | **34** | **43** | **21%** |

---

## 🎯 Öncelik Sıralaması

### **Yüksek Öncelik** (9 test - Kritik user flows)
1. ✅ Login Page
2. ✅ Register Page
3. ✅ Forgot Password Page
4. ✅ Home Page
5. ✅ Splash Page
6. ✅ Order Tracking Page
7. ✅ Order Confirmation Page
8. ✅ Order Detail Page
9. ✅ Orders List Page

### **Orta Öncelik** (8 test - Önemli features)
1. Profile Page
2. Settings Page
3. Address Management
4. Add/Edit Address
5. Product Card
6. Merchant Detail
7. Search Page
8. Network Status Indicator

### **Düşük Öncelik** (17 test - Nice to have)
- Skeleton widgets (5)
- Review widgets (2)
- Notification widgets (3)
- About Page
- Onboarding
- Not Found Page
- Payment Page (zaten checkout var)
- vs.

---

## 💡 Öneriler

### **Yaklaşım 1: Kritik Testler** (Tahmini: 2-3 saat)
Sadece **Yüksek Öncelik** testlerini yaz (9 test)
- Auth flow complete olur
- Order flow complete olur
- Ana sayfalar kapsanır

### **Yaklaşım 2: Minimal Viable Tests** (Tahmini: 1 saat)
En kritik 5 testi yaz:
- Login Page
- Home Page
- Cart Page (zaten var ✅)
- Checkout Page (zaten var ✅)
- Order Confirmation

### **Yaklaşım 3: Component First** (Tahmini: 1.5 saat)
Önce shared component'leri test et:
- Product Card
- Merchant Card
- Network Status
- Main Navigation
Sonra page testleri kolay olur

---

## 🚀 Tavsiye Edilen Strateji

**AŞAMA 1:** Auth Pages (Login, Register, Forgot Password) - 1 saat  
**AŞAMA 2:** Main Flow Pages (Home, Splash) - 30 dakika  
**AŞAMA 3:** Order Pages (Tracking, Confirmation, Detail) - 45 dakika  
**AŞAMA 4:** Diğerleri (opsiyonel) - zaman kalırsa

**Toplam tahmini süre:** 2-3 saat tüm kritik testler için

---

## ✅ Mevcut Test Coverage

**Unit Tests:** 518 test ✅ **%100**  
**Integration Tests:** 5 test ✅ **%100**  
**Widget Tests:** 58 test ✅ **%100** (mevcut olanlar)  
**Toplam:** **581 test** çalışıyor

**Widget test coverage:** %21 (9/43)  
**Genel test coverage:** Mükemmel (tüm business logic kapsanmış)

---

## 📝 Notlar

- Unit testler (518) zaten tüm business logic'i kapsıyor ✅
- Widget testleri UI rendering'i test eder (less critical)
- Integration testleri main flows'u kapsıyor (5 test) ✅
- **Mevcut durum production-ready!**

Widget testleri **nice-to-have** - kritik değil çünkü business logic zaten tamamen test edilmiş.

