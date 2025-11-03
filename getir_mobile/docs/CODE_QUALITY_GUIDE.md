# 📊 Code Quality Guide

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Kod Kalitesi Standartları

---

## 🎯 Genel Prensipler

### SOLID Principles

- **S**ingle Responsibility: Her class/module tek bir sorumluluğa sahip olmalı
- **O**pen/Closed: Extension için açık, modification için kapalı
- **L**iskov Substitution: Alt sınıflar üst sınıfların yerine kullanılabilmeli
- **I**nterface Segregation: Interface'ler küçük ve spesifik olmalı
- **D**ependency Inversion: Yüksek seviye modüller düşük seviye modüllere bağımlı olmamalı

### DRY (Don't Repeat Yourself)

Kod tekrarlarından kaçın.

### KISS (Keep It Simple, Stupid)

Basit çözümler karmaşık çözümlerden iyidir.

---

## 📝 Naming Conventions

### Files
```
✅ lib/services/auth_service.dart
✅ lib/pages/home/home_page.dart
✅ lib/bloc/cart/cart_bloc.dart

❌ lib/AuthService.dart
❌ lib/homePage.dart
```

### Classes
```dart
✅ class AuthService {}
✅ class HomePage extends StatelessWidget {}
✅ class CartBloc extends Bloc<CartEvent, CartState> {}

❌ class authService {}
❌ class homePage {}
```

### Variables
```dart
✅ String userName = 'John';
✅ bool isLoggedIn = false;
✅ List<Product> products = [];

❌ String userName2
❌ bool flag
❌ List<Product> lst
```

---

## 🧪 Testing Standards

### Unit Tests

Her service için:
```dart
✅ Happy path test
✅ Error handling test
✅ Edge cases test
✅ Null safety test
```

### Widget Tests

Önemli widget'lar için:
```dart
✅ Rendering test
✅ User interaction test
✅ State update test
```

### Coverage Target

**Minimum:** %60

---

## 🔍 Code Review Checklist

- [ ] Linter warnings yok
- [ ] Tests passing
- [ ] Coverage maintained
- [ ] Documentation updated
- [ ] No hard-coded values
- [ ] Error handling proper
- [ ] Performance considered

---

**Hazırlayan:** Tech Lead  
**Tarih:** 2 Kasım 2025

