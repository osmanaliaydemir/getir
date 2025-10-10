# ✅ Test Execution Checklist - Sprint Planning

**Sprint Süresi:** 10 Hafta  
**Hedef Coverage:** %35 → %90  
**Toplam Eksik:** 58 test dosyası, ~1068 test case

---

## 📅 Sprint 1-2: Critical Foundation (Hafta 1-2)

### Week 1
- [ ] **ReviewBloc Test** - `test/unit/blocs/review_bloc_test.dart` (18 test)
- [ ] **Register Page Test** - `test/widget/auth/register_page_widget_test.dart` (30 test)
- [ ] **Forgot Password Test** - `test/widget/auth/forgot_password_page_widget_test.dart` (18 test)
- [ ] **SignalR Service Test** - `test/unit/core/services/signalr_service_test.dart` (50 test)

**Week 1 Target:** 4 files, 116 tests ✅

---

### Week 2
- [ ] **Home Page Test** - `test/widget/pages/home_page_widget_test.dart` (35 test)
- [ ] **Encryption Service Test** - `test/unit/core/services/encryption_service_test.dart` (30 test)
- [ ] **Network Service Test** - `test/unit/core/services/network_service_test.dart` (35 test)
- [ ] **Network Cubit Test** - `test/unit/cubits/network_cubit_test.dart` (20 test)

**Week 2 Target:** 4 files, 120 tests ✅

**Sprint 1-2 Total:** 8 files, 236 tests  
**Coverage Artışı:** %35 → %45

---

## 📅 Sprint 3-4: Critical Pages (Hafta 3-4)

### Week 3
- [ ] **Merchant Detail Test** - `test/widget/pages/merchant_detail_page_widget_test.dart` (40 test)
- [ ] **Order Tracking Test** - `test/widget/pages/order_tracking_page_widget_test.dart` (30 test)
- [ ] **Local Storage Service Test** - `test/unit/core/services/local_storage_service_test.dart` (40 test)

**Week 3 Target:** 3 files, 110 tests ✅

---

### Week 4
- [ ] **Order Detail Test** - `test/widget/pages/order_detail_page_widget_test.dart` (25 test)
- [ ] **Payment Page Test** - `test/widget/pages/payment_page_widget_test.dart` (35 test)
- [ ] **Firebase Service Test** - `test/unit/core/services/firebase_service_test.dart` (35 test)
- [ ] **Order Realtime Binder Test** - `test/unit/core/services/order_realtime_binder_test.dart` (25 test)

**Week 4 Target:** 4 files, 120 tests ✅

**Sprint 3-4 Total:** 7 files, 230 tests  
**Coverage Artışı:** %45 → %55

---

## 📅 Sprint 5-6: Services & Components (Hafta 5-6)

### Week 5: Core Services
- [ ] **Analytics Service Test** - `test/unit/core/services/analytics_service_test.dart` (30 test)
- [ ] **API Cache Service Test** - `test/unit/core/services/api_cache_service_test.dart` (35 test)
- [ ] **Logger Service Test** - `test/unit/core/services/logger_service_test.dart` (25 test)
- [ ] **Pending Actions Service Test** - `test/unit/core/services/pending_actions_service_test.dart` (25 test)

**Week 5 Target:** 4 files, 115 tests ✅

---

### Week 6: More Services & Pages
- [ ] **Reconnection Strategy Test** - `test/unit/core/services/reconnection_strategy_service_test.dart` (22 test)
- [ ] **Sync Service Test** - `test/unit/core/services/sync_service_test.dart` (20 test)
- [ ] **Search History Service Test** - `test/unit/core/services/search_history_service_test.dart` (18 test)
- [ ] **Address Management Test** - `test/widget/pages/address_management_page_widget_test.dart` (22 test)
- [ ] **Add/Edit Address Test** - `test/widget/pages/add_edit_address_page_widget_test.dart` (30 test)

**Week 6 Target:** 5 files, 112 tests ✅

**Sprint 5-6 Total:** 9 files, 227 tests  
**Coverage Artışı:** %55 → %65

---

## 📅 Sprint 7-8: UI Components & Cubits (Hafta 7-8)

### Week 7: Widget Components (Part 1)
- [ ] **Merchant Card Test** - `test/widget/components/merchant_card_widget_test.dart` (18 test)
- [ ] **Product Card Test** - `test/widget/components/product_card_widget_test.dart` (20 test)
- [ ] **Order Card Test** - `test/widget/components/order_card_widget_test.dart` (18 test)
- [ ] **Notification Card Test** - `test/widget/components/notification_card_widget_test.dart` (15 test)
- [ ] **Review Card Test** - `test/widget/components/review_card_widget_test.dart` (15 test)

**Week 7 Target:** 5 files, 86 tests ✅

---

### Week 8: Cubits & Components (Part 2)
- [ ] **Theme Cubit Test** - `test/unit/cubits/theme_cubit_test.dart` (15 test)
- [ ] **Language Cubit Test** - `test/unit/cubits/language_cubit_test.dart` (18 test)
- [ ] **Notification Badge Cubit Test** - `test/unit/cubits/notification_badge_cubit_test.dart` (12 test)
- [ ] **Main Navigation Test** - `test/widget/components/main_navigation_widget_test.dart` (20 test)
- [ ] **Network Status Indicator Test** - `test/widget/components/network_status_indicator_widget_test.dart` (12 test)
- [ ] **Paginated ListView Test** - `test/widget/components/paginated_list_view_widget_test.dart` (18 test)

**Week 8 Target:** 6 files, 95 tests ✅

**Sprint 7-8 Total:** 11 files, 181 tests  
**Coverage Artışı:** %65 → %75

---

## 📅 Sprint 9-10: Integration & E2E (Hafta 9-10)

### Week 9: Integration Tests (Part 1)
- [ ] **Checkout Flow Test** - `test/integration/checkout_flow_test.dart` (30 test)
- [ ] **Cart Management Flow Test** - `test/integration/cart_management_flow_test.dart` (25 test)
- [ ] **Search & Filter Flow Test** - `test/integration/product_search_filter_flow_test.dart` (22 test)
- [ ] **Orders Page Test** - `test/widget/pages/orders_page_widget_test.dart` (25 test)
- [ ] **Profile Page Test** - `test/widget/pages/profile_page_widget_test.dart` (25 test)

**Week 9 Target:** 5 files, 127 tests ✅

---

### Week 10: Integration Tests (Part 2) & E2E
- [ ] **Address Management Flow Test** - `test/integration/address_management_flow_test.dart` (18 test)
- [ ] **Review & Rating Flow Test** - `test/integration/review_rating_flow_test.dart` (18 test)
- [ ] **Notification Flow Test** - `test/integration/notification_flow_test.dart` (20 test)
- [ ] **Complete Order Journey E2E** - `test/e2e/complete_order_journey_test.dart` (1 flow)
- [ ] **Guest User Flow E2E** - `test/e2e/guest_user_flow_test.dart` (1 flow)
- [ ] **Offline Mode E2E** - `test/e2e/offline_mode_test.dart` (1 flow)

**Week 10 Target:** 6 files, 59 tests ✅

**Sprint 9-10 Total:** 11 files, 186 tests  
**Coverage Artışı:** %75 → %90

---

## 📊 Sprint Summary

| Sprint | Weeks | Files | Tests | Coverage |
|--------|-------|-------|-------|----------|
| Sprint 1-2 | 1-2 | 8 | 236 | %35 → %45 |
| Sprint 3-4 | 3-4 | 7 | 230 | %45 → %55 |
| Sprint 5-6 | 5-6 | 9 | 227 | %55 → %65 |
| Sprint 7-8 | 7-8 | 11 | 181 | %65 → %75 |
| Sprint 9-10 | 9-10 | 11 | 186 | %75 → %90 |
| **TOTAL** | **10** | **46** | **1060** | **%90** |

---

## 🚦 Günlük Test Hedefi

### Ortalama İş Yükü
- **Test/Gün:** ~11 test case
- **Dosya/Hafta:** ~5 test dosyası
- **Çalışma Günü:** 5 gün/hafta
- **Toplam:** 50 çalışma günü

### Zaman Tahminleri
- **Basit Test:** 15-20 dakika
- **Orta Test:** 30-40 dakika
- **Kompleks Test:** 45-60 dakika
- **Ortalama:** ~30 dakika/test

---

## 🎯 Haftalık Review Checklist

### Her Hafta Sonu Yapılacaklar:
- [ ] Bu haftanın tüm testleri tamamlandı mı?
- [ ] Tüm testler geçiyor mu? (`flutter test`)
- [ ] Coverage raporu oluşturuldu mu? (`flutter test --coverage`)
- [ ] Target coverage'a ulaşıldı mı?
- [ ] Testler CI/CD'de çalışıyor mu?
- [ ] Flaky test var mı? (Varsa düzelt)
- [ ] Test dokümantasyonu güncellendi mi?
- [ ] Code review yapıldı mı?
- [ ] Branch merge edildi mi?

---

## 🔥 Kritik Öncelikler (Bu Hafta Başla!)

### Week 1 - DAY 1 (Pazartesi)
- [ ] ReviewBloc Test setup
- [ ] Write first 5 tests for ReviewBloc
- [ ] Mock ReviewService

### Week 1 - DAY 2 (Salı)
- [ ] Complete ReviewBloc Test (13 more tests)
- [ ] Start Register Page Test setup

### Week 1 - DAY 3 (Çarşamba)
- [ ] Write 15 tests for Register Page
- [ ] Form validation tests

### Week 1 - DAY 4 (Perşembe)
- [ ] Complete Register Page Test (15 more tests)
- [ ] Start Forgot Password Test

### Week 1 - DAY 5 (Cuma)
- [ ] Complete Forgot Password Test (18 tests)
- [ ] Start SignalR Service Test setup

**Week 1 Goal:** 4 files completed, 116 tests ✅

---

## 📝 Test Template (Hızlı Başlangıç)

### BLoC Test Template
```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';

@GenerateMocks([YourService])
void main() {
  group('YourBloc', () {
    late YourBloc bloc;
    late MockYourService mockService;

    setUp(() {
      mockService = MockYourService();
      bloc = YourBloc(mockService);
    });

    tearDown(() {
      bloc.close();
    });

    blocTest<YourBloc, YourState>(
      'should emit [Loading, Success] when successful',
      build: () {
        when(mockService.method()).thenAnswer((_) async => Result.success(data));
        return bloc;
      },
      act: (bloc) => bloc.add(YourEvent()),
      expect: () => [YourLoadingState(), YourSuccessState(data)],
      verify: (_) {
        verify(mockService.method()).called(1);
      },
    );
  });
}
```

### Widget Test Template
```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:mockito/mockito.dart';

void main() {
  group('YourPage Widget Tests', () {
    late MockYourBloc mockBloc;

    setUp(() {
      mockBloc = MockYourBloc();
    });

    testWidgets('should display expected UI elements', (tester) async {
      // Arrange
      when(mockBloc.state).thenReturn(YourInitialState());
      whenListen(mockBloc, Stream.fromIterable([YourInitialState()]));

      // Act
      await tester.pumpWidget(
        MaterialApp(
          home: BlocProvider<YourBloc>.value(
            value: mockBloc,
            child: YourPage(),
          ),
        ),
      );

      // Assert
      expect(find.text('Expected Text'), findsOneWidget);
      expect(find.byType(ExpectedWidget), findsOneWidget);
    });
  });
}
```

---

## 🎓 Test Komutları (Hızlı Referans)

```bash
# Tüm testleri çalıştır
flutter test

# Coverage ile
flutter test --coverage

# Belirli dosya
flutter test test/unit/blocs/review_bloc_test.dart

# Watch mode (otomatik rerun)
flutter test --watch

# Verbose output
flutter test --verbose

# Integration tests
flutter test integration_test/

# Generate mocks
flutter pub run build_runner build --delete-conflicting-outputs

# HTML coverage report
genhtml coverage/lcov.info -o coverage/html
open coverage/html/index.html  # Mac
start coverage/html/index.html # Windows
```

---

## ✅ Sprint Completion Criteria

### Sprint Başarılı Sayılır Eğer:
- [ ] Planlanan tüm test dosyaları yazıldı
- [ ] Planlanan test case sayısına ulaşıldı
- [ ] Tüm testler geçiyor
- [ ] Hedef coverage'a ulaşıldı
- [ ] Flaky test yok
- [ ] CI/CD entegre
- [ ] Code review tamamlandı
- [ ] Dokümantasyon güncellendi

---

## 🚀 Başarı Metrikleri

### Haftalık Track
- [ ] Test dosyası sayısı hedefine ulaşıldı mı?
- [ ] Test case sayısı hedefine ulaşıldı mı?
- [ ] Coverage hedefine ulaşıldı mı?
- [ ] Tüm testler geçiyor mu?
- [ ] Test execution süresi makul mü? (<5 dk)

### Kalite Metrikleri
- **Flaky Test Oranı:** <5%
- **Test Execution Time:** <5 dakika
- **Test Pass Rate:** >95%
- **Code Coverage:** >90%
- **Test Maintainability:** Yüksek

---

**Son Güncelleme:** 9 Ekim 2025  
**Sprint Owner:** Osman Ali Aydemir  
**Next Review:** Her Cuma EOD  
**Status:** 🔴 Sprint henüz başlamadı

