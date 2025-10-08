# 🔌 Backend API Mapping - Flutter ↔ .NET

**Date:** 8 Ekim 2025  
**Backend URL:** http://ajilgo.runasp.net  
**Status:** ✅ Backend YOK, endpoint'ler VAR!

---

## 🎯 Ana Sorun

**Flutter test dosyaları YANLIŞ entity field'ları kullanıyor!**

### Gerçek Entity Fields:

#### UserEntity (DOĞRU)
```dart
class UserEntity {
  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final String? phoneNumber;
  final String role;
  final bool isEmailVerified;
  final bool isActive;           // ✅ VAR
  final DateTime createdAt;
  final DateTime? updatedAt;
  final DateTime? lastLoginAt;
  
  // ❌ YOK:
  // - profileImageUrl
  // - isPhoneVerified
}
```

#### Product (DOĞRU)
```dart
class Product {
  final String id;
  final String name;
  final double price;
  final int stockQuantity;       // ✅ "stock" değil "stockQuantity"
  final List<ProductVariant> variants;  // ✅ Required
  final List<ProductOption> options;    // ✅ Required
  final String unit;             // ✅ Required (kg, adet, etc.)
  final String brand;            // ✅ Required
  final String barcode;          // ✅ Required
  final Map<String, dynamic> nutritionalInfo;  // ✅ Required
  
  // ❌ YOK:
  // - createdAt
  // - updatedAt
  // - stock (stockQuantity kullan)
}
```

#### Merchant (DOĞRU)
```dart
class Merchant {
  final String id;
  final String name;
  final String coverImageUrl;    // ✅ "bannerUrl" değil
  final String phoneNumber;      // ✅ "phone" değil
  final List<String> categories; // ✅ Array, tek string değil
  final Map<String, String> workingHours;  // ✅ Required
  final double minimumOrderAmount;  // ✅ "minimumOrder" değil
  
  // ❌ YOK:
  // - category (tek string)
  // - bannerUrl
  // - phone
  // - email
  // - tags
  // - isFeatured
  // - createdAt/updatedAt
}
```

#### AuthResponse (DOĞRU)
```dart
class AuthResponse {
  final String accessToken;
  final String refreshToken;
  final DateTime expiresAt;
  final String role;
  final String userId;
  final String email;
  final String fullName;
  
  // ❌ YOK:
  // - user (UserModel yok, sadece basic fields var)
}
```

---

## 🔧 ÇÖZ
