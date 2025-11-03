# 🔍 Linting & Code Style Guide

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Kod Stili ve Linting Kuralları

---

## 📋 İçindekiler

- [Linting Configuration](#linting-configuration)
- [Code Style](#code-style)
- [Best Practices](#best-practices)
- [Pre-commit Hooks](#pre-commit-hooks)

---

## ⚙️ Linting Configuration

### Analysis Options

**File:** `analysis_options.yaml`

```yaml
include: package:flutter_lints/flutter.yaml

analyzer:
  exclude:
    - "**/*.g.dart"
    - "**/*.freezed.dart"
    - "**/*.mocks.dart"
  
  errors:
    invalid_annotation_target: ignore
    lines_longer_than_80_chars: ignore
    prefer_final_locals: ignore

linter:
  rules:
    - avoid_empty_else
    - avoid_print
    - avoid_relative_lib_imports
    - cancel_subscriptions
    - prefer_const_constructors
    # ... 150+ more rules
```

---

## 📝 Code Style

### Formatting

```bash
# Auto-format
dart format lib/ test/

# Check formatting
dart format --set-exit-if-changed lib/ test/
```

### Max Line Length

**80 characters** (configurable)

---

## ✅ Best Practices

### 1. Naming

```dart
✅ class AuthService
✅ String userName
✅ Future<void> fetchData()

❌ class auth_service
❌ String userName2
❌ Future<void> getData()
```

### 2. Imports

```dart
✅ import 'package:flutter/material.dart';
✅ import '../services/auth_service.dart';

❌ import '../../../../../../services/auth.dart';
```

### 3. Const Usage

```dart
✅ const Icon(Icons.home)
✅ static const String apiKey = 'key';

❌ Icon(Icons.home)
```

---

## 🔧 Pre-commit Hooks

**Location:** `.githooks/pre-commit`

```bash
# Runs automatically on git commit
1. flutter analyze
2. dart format --set-exit-if-changed
3. flutter test (optional)
```

---

**Current Status:** **0 linter warnings** ✅

**Hazırlayan:** Code Quality Team  
**Tarih:** 2 Kasım 2025

