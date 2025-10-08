# 🧪 Test Infrastructure

## 📁 Klasör Yapısı

```
test/
├── unit/                    # Unit testler
│   ├── usecases/           # Use case testleri
│   ├── repositories/       # Repository testleri
│   ├── blocs/             # BLoC testleri
│   └── services/          # Service testleri
├── widget/                 # Widget testleri
├── integration/            # Integration testleri
├── helpers/                # Test helper'ları
│   └── mock_data.dart     # Mock data fixtures
└── README.md

```

## 🚀 Testleri Çalıştırma

### Tüm Testleri Çalıştır
```bash
flutter test
```

### Coverage ile Çalıştır
```bash
# PowerShell
.\run_tests_with_coverage.ps1

# Bash/Linux
./run_tests_with_coverage.sh
```

### Belirli Bir Testi Çalıştır
```bash
flutter test test/unit/usecases/login_usecase_test.dart
```

## 📝 Test Yazma

### 1. Mock Sınıfları Generate Etme

Test yazmadan önce mock sınıflarını generate edin:

```bash
dart run build_runner build --delete-conflicting-outputs
```

### 2. Use Case Test Örneği

```dart
import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:getir_mobile/domain/repositories/auth_repository.dart';
import 'package:getir_mobile/domain/usecases/auth_usecases.dart';

@GenerateMocks([AuthRepository])
import 'my_test.mocks.dart';

void main() {
  late MyUseCase useCase;
  late MockAuthRepository mockRepository;

  setUp(() {
    mockRepository = MockAuthRepository();
    useCase = MyUseCase(mockRepository);
  });

  test('should return expected result', () async {
    // Arrange
    when(mockRepository.method()).thenAnswer((_) async => expectedResult);

    // Act
    final result = await useCase();

    // Assert
    expect(result, equals(expectedResult));
    verify(mockRepository.method()).called(1);
  });
}
```

## 📊 Mevcut Coverage

Şu anda yazılan testler:
- ✅ Login Use Case (9 test)
- ✅ Register Use Case (8 test)

**Toplam:** 17 test

## 🎯 Hedef

- Minimum %60 code coverage
- Kritik business logic testleri
- BLoC testleri
- Integration testleri

## 🔧 Mock Data

`helpers/mock_data.dart` dosyasında test için kullanılacak mock datalar bulunur:

```dart
final testUser = MockData.testUser;
final testMerchant = MockData.testMerchant;
final testProduct = MockData.testProduct;
final testCart = MockData.testCart;
```

## 📚 Test Best Practices

1. **AAA Pattern:** Arrange, Act, Assert
2. **Descriptive Names:** Test adları açıklayıcı olmalı
3. **Single Responsibility:** Her test tek bir şeyi test etmeli
4. **Independent:** Testler birbirinden bağımsız olmalı
5. **Mocking:** Dış bağımlılıkları mock'layın

## 🐛 Debugging

### Test Hatası Aldığınızda

1. Mock'ların doğru setup edildiğini kontrol edin
2. `when()` ile beklenen davranışları tanımlayın
3. `verify()` ile method çağrılarını doğrulayın
4. `-n` flag ile verbose output alın:
   ```bash
   flutter test -n "test name"
   ```

## 🔗 Kaynaklar

- [Flutter Testing Documentation](https://docs.flutter.dev/testing)
- [Mockito Documentation](https://pub.dev/packages/mockito)
- [Bloc Test](https://pub.dev/packages/bloc_test)
