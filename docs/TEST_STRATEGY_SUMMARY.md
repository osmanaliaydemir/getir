# 🎯 TEST STRATEGY - HIZLI BAŞLANGIÇ

## 📋 3 Dokümandan Hangisini Kullanmalıyım?

### 🔍 Detaylı Analiz İstiyorum
➡️ **`MISSING_TESTS_COMPREHENSIVE_LIST.md`**
- 58 eksik testin detaylı açıklaması
- Her test için örnek senaryolar
- Best practices
- 42 sayfa

### ✅ Günlük Checklist İstiyorum  
➡️ **`TEST_EXECUTION_CHECKLIST.md`**
- 10 haftalık sprint planı
- Günlük yapılacaklar
- Test template'leri
- Komut referansları
- 15 sayfa

### 📊 Genel Bakış İstiyorum
➡️ **`TEST_STRATEGY_README.md`**
- Hızlı özet
- Nasıl başlanır
- 3 sayfa

---

## ⚡ 5 Dakikada Başla

### 1. İlk Test: ReviewBloc (EN KRİTİK)

```bash
# Dosyayı oluştur
touch test/unit/blocs/review_bloc_test.dart

# Template'i kopyala
cat > test/unit/blocs/review_bloc_test.dart << 'EOF'
import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';

@GenerateMocks([ReviewService])
void main() {
  group('ReviewBloc', () {
    late ReviewBloc bloc;
    late MockReviewService mockService;

    setUp(() {
      mockService = MockReviewService();
      bloc = ReviewBloc(mockService);
    });

    tearDown(() {
      bloc.close();
    });

    blocTest<ReviewBloc, ReviewState>(
      'should emit [Loading, Loaded] when fetch reviews successful',
      build: () {
        when(mockService.fetchReviews(any))
            .thenAnswer((_) async => Result.success(mockReviews));
        return bloc;
      },
      act: (bloc) => bloc.add(FetchReviewsEvent(merchantId: '123')),
      expect: () => [ReviewsLoading(), ReviewsLoaded(mockReviews)],
      verify: (_) {
        verify(mockService.fetchReviews('123')).called(1);
      },
    );

    // TODO: 17 test daha ekle
  });
}
EOF

# Mock'ları generate et
flutter pub run build_runner build

# Test'i çalıştır
flutter test test/unit/blocs/review_bloc_test.dart
```

### 2. İkinci Test: Register Page

```bash
touch test/widget/auth/register_page_widget_test.dart
# Template'i TEST_EXECUTION_CHECKLIST.md'den kopyala
# 30 test yaz
```

### 3. Üçüncü Test: Forgot Password

```bash
touch test/widget/auth/forgot_password_page_widget_test.dart
# 18 test yaz
```

---

## 📊 Eksik Testler - Bir Bakışta

| Kategori | Eksik | Kritik | Orta | Düşük |
|----------|-------|--------|------|-------|
| **BLoC** | 1 | 1 | 0 | 0 |
| **Widget Pages** | 14 | 7 | 5 | 2 |
| **Components** | 10 | 0 | 7 | 3 |
| **Core Services** | 16 | 5 | 7 | 4 |
| **Cubits** | 4 | 1 | 2 | 1 |
| **Integration** | 8 | 2 | 4 | 2 |
| **E2E** | 5 | 1 | 2 | 2 |
| **TOPLAM** | **58** | **17** | **27** | **14** |

---

## 🔥 Öncelik Sırası (İlk 10 Test)

### Week 1 (Bu Hafta!)
1. ✅ **ReviewBloc Test** - 18 test - P0 CRITICAL
2. ✅ **Register Page Test** - 30 test - P0 CRITICAL  
3. ✅ **Forgot Password Test** - 18 test - P0 HIGH
4. ✅ **SignalR Service Test** - 50 test - P0 CRITICAL

### Week 2
5. ✅ **Home Page Test** - 35 test - P0 CRITICAL
6. ✅ **Encryption Service Test** - 30 test - P0 CRITICAL
7. ✅ **Network Service Test** - 35 test - P0 CRITICAL
8. ✅ **Network Cubit Test** - 20 test - P0 HIGH

### Week 3
9. ✅ **Merchant Detail Test** - 40 test - P0 CRITICAL
10. ✅ **Order Tracking Test** - 30 test - P0 CRITICAL

**İlk 10 Test Toplam:** 306 test case

---

## 🎯 Coverage Roadmap

```
Week 0  (Now):    ███████░░░░░░░░░░░░░  35% 🔴
Week 2  (Sprint 1): █████████░░░░░░░░░░  45% 🟡
Week 4  (Sprint 2): ███████████░░░░░░░░  55% 🟡
Week 6  (Sprint 3): █████████████░░░░░░  65% 🟢
Week 8  (Sprint 4): ███████████████░░░░  75% 🟢
Week 10 (Sprint 5): ██████████████████░  90% ✅
```

---

## ✅ Günlük Checklist

### Her Sabah (Planning)
- [ ] Bugün hangi test dosyasını yazacağım?
- [ ] Kaç test case hedefliyorum? (Ortalama: 10-12)
- [ ] Mock'lar hazır mı?

### Gün İçi (Execution)
- [ ] Template'den başladım
- [ ] Happy path testlerini yazdım
- [ ] Error scenario'ları yazdım
- [ ] Edge case'leri yazdım
- [ ] Testleri çalıştırdım ve geçtim

### Her Akşam (Review)
- [ ] Tüm testler geçiyor mu? ✅
- [ ] Coverage arttı mı?
- [ ] Flaky test var mı?
- [ ] Commit & Push yaptım mı?
- [ ] Yarın ne yazacağım?

---

## 📈 İlerleme Takibi

### Haftalık Hedef Tracking
```
Week 1: [ ] 4 files, 116 tests ░░░░░
Week 2: [ ] 4 files, 120 tests ░░░░░
Week 3: [ ] 3 files, 110 tests ░░░
Week 4: [ ] 4 files, 120 tests ░░░░░
Week 5: [ ] 4 files, 115 tests ░░░░░
Week 6: [ ] 5 files, 112 tests ░░░░░░
Week 7: [ ] 5 files, 86 tests  ░░░░░
Week 8: [ ] 6 files, 95 tests  ░░░░░░
Week 9: [ ] 5 files, 127 tests ░░░░░
Week 10: [ ] 6 files, 59 tests ░░░░░░
```

Her tamamlanan test için ░ → █ yap!

---

## 🚀 Hızlı Komutlar

```bash
# Test çalıştır
flutter test

# Sadece bir dosya
flutter test test/unit/blocs/review_bloc_test.dart

# Coverage
flutter test --coverage && genhtml coverage/lcov.info -o coverage/html

# Mock generate
flutter pub run build_runner build --delete-conflicting-outputs

# Watch mode (otomatik rerun)
flutter test --watch
```

---

## 🎓 Öğrenme Kaynakları

### İyi Örnekler (Projede Mevcut)
- `test/unit/blocs/auth_bloc_test.dart` - BLoC test örneği
- `test/unit/repositories/auth_repository_impl_test.dart` - Repository test
- `test/widget/pages/cart_page_widget_test.dart` - Widget test

### External Links
- [Flutter Testing](https://docs.flutter.dev/testing)
- [BLoC Testing](https://bloclibrary.dev/#/testing)
- [Mockito](https://pub.dev/packages/mockito)

---

## ⚠️ Sık Yapılan Hatalar

### ❌ YAPMA
```dart
// Kötü: Test birbirine bağımlı
test('first test', () {
  globalVariable = 'value';
});

test('second test', () {
  expect(globalVariable, 'value'); // İlk teste bağımlı!
});
```

### ✅ YAP
```dart
// İyi: Her test izole
test('first test', () {
  final value = 'value';
  expect(value, 'value');
});

test('second test', () {
  final value = 'value';
  expect(value, 'value');
});
```

---

## 📞 Yardım

### Takıldığında Kontrol Et:
1. ✅ Mock'lar doğru generate edildi mi? (`build_runner`)
2. ✅ Import'lar eksik mi?
3. ✅ Test izole mi? (setUp/tearDown kullan)
4. ✅ Async bekliyor musun? (`await`)
5. ✅ State emission sırası doğru mu?

### Hala Çözemediysen:
- Mevcut testlere bak (yukarıdaki örnekler)
- Dokümantasyonu oku
- Template'i kullan

---

## 🎯 BU HAFTA BAŞLA!

```
BUGÜN: ReviewBloc Test - İlk 5 test
YARIN: ReviewBloc Test - Geri kalan 13 test
3. GÜN: Register Page Test - İlk 15 test
4. GÜN: Register Page Test - Geri kalan 15 test
5. GÜN: Forgot Password Test - Tüm 18 test
```

**Week 1 Hedef:** 116 test ✅  
**Week 1 Deadline:** 5 gün  
**Week 1 Coverage:** %35 → %45

---

**🚦 DURUM: BAŞLA HEMEN!**

**İlk Komut:**
```bash
cd getir_mobile
touch test/unit/blocs/review_bloc_test.dart
code test/unit/blocs/review_bloc_test.dart
```

**GO! 🚀**

