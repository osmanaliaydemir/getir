# 🧪 Test Execution Guide

**Date:** 8 Ekim 2025  
**Total Tests:** ~282 test cases

---

## 🚀 Quick Start

### Step 1: Generate Mocks (Required First)
```powershell
cd getir_mobile
dart run build_runner build --delete-conflicting-outputs
```

⏱️ **Duration:** 2-3 minutes

---

### Step 2: Run All Tests
```powershell
flutter test
```

⏱️ **Duration:** 1-2 minutes

---

### Step 3: Run Tests with Coverage
```powershell
flutter test --coverage
```

⏱️ **Duration:** 2-3 minutes

---

### Step 4: Generate HTML Coverage Report
```powershell
# Windows PowerShell
.\run_tests_with_coverage.ps1

# Linux/Mac
./run_tests_with_coverage.sh
```

---

## 📋 Test Files

### Repository Tests (test/unit/repositories/)
```
✅ auth_repository_impl_test.dart        (50+ tests)
✅ cart_repository_impl_test.dart        (40+ tests)
✅ product_repository_impl_test.dart     (45+ tests)
✅ merchant_repository_impl_test.dart    (35+ tests)
```

### Datasource Tests (test/unit/datasources/)
```
✅ auth_datasource_impl_test.dart        (30+ tests)
```

### BLoC Tests (test/unit/blocs/)
```
✅ auth_bloc_test.dart                   (existing)
✅ cart_bloc_test.dart                   (30+ tests)
```

### Widget Tests (test/widget/)
```
✅ auth/login_page_widget_test.dart      (20+ tests)
✅ pages/product_list_page_widget_test.dart (20+ tests)
```

### Integration Tests (test/integration/)
```
✅ auth_flow_test.dart                   (12+ tests)
```

---

## 📊 Expected Coverage

```
Repository Layer:   ~60-70%
Datasource Layer:   ~40-50%
BLoC Layer:         ~35-45%
Widget Layer:       ~20-30%
Integration:        ~25-35%
────────────────────────────
Overall Target:     ~40-50%
```

---

## ✅ Execution Checklist

- [ ] Generate mocks with build_runner
- [ ] Run all tests (`flutter test`)
- [ ] Verify all tests pass
- [ ] Generate coverage report
- [ ] Review coverage HTML
- [ ] Fix any failing tests
- [ ] Update documentation

---

**Ready to execute!** 🎯
