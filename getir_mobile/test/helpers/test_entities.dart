/// Test helper for creating test entities with correct fields
library;

import 'package:getir_mobile/domain/entities/user_entity.dart';
import 'package:getir_mobile/domain/entities/product.dart';
import 'package:getir_mobile/domain/entities/merchant.dart';
import 'package:getir_mobile/data/models/auth_models.dart';

/// Helper class for creating test entities
class TestEntities {
  /// Create test user entity with all required fields
  static UserEntity createTestUser({
    String id = '1',
    String email = 'test@getir.com',
    String firstName = 'Test',
    String lastName = 'User',
    String? phoneNumber = '+905551234567',
    String role = 'Customer',
    bool isEmailVerified = true,
    bool isActive = true,
  }) {
    return UserEntity(
      id: id,
      email: email,
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber,
      role: role,
      isEmailVerified: isEmailVerified,
      isActive: isActive,
      createdAt: DateTime(2024, 1, 1),
      updatedAt: DateTime(2024, 1, 1),
      lastLoginAt: DateTime(2024, 1, 1),
    );
  }

  /// Create test auth response
  static AuthResponse createTestAuthResponse({
    String accessToken = 'test_access_token',
    String refreshToken = 'test_refresh_token',
    String userId = '1',
    String email = 'test@getir.com',
    String fullName = 'Test User',
    String role = 'Customer',
  }) {
    return AuthResponse(
      accessToken: accessToken,
      refreshToken: refreshToken,
      expiresAt: DateTime.now().add(const Duration(hours: 1)),
      userId: userId,
      email: email,
      fullName: fullName,
      role: role,
    );
  }

  /// Create test product
  static Product createTestProduct({
    String id = 'product_1',
    String name = 'Test Product',
    String description = 'Test Description',
    double price = 99.99,
    String imageUrl = 'https://example.com/image.jpg',
    String category = 'Electronics',
    String merchantId = 'merchant_1',
    String merchantName = 'Test Merchant',
    bool isAvailable = true,
    int stockQuantity = 100,
    double rating = 4.5,
    int reviewCount = 50,
    String unit = 'adet',
    List<String> tags = const ['electronics'],
  }) {
    return Product(
      id: id,
      name: name,
      description: description,
      price: price,
      imageUrl: imageUrl,
      category: category,
      merchantId: merchantId,
      merchantName: merchantName,
      isAvailable: isAvailable,
      stockQuantity: stockQuantity,
      variants: const [],
      options: const [],
      rating: rating,
      reviewCount: reviewCount,
      unit: unit,
      tags: tags,
      nutritionalInfo: const {},
      brand: 'Test Brand',
      barcode: '1234567890',
    );
  }

  /// Create test merchant
  static Merchant createTestMerchant({
    String id = 'merchant_1',
    String name = 'Test Restaurant',
    String description = 'Best restaurant in town',
    String logoUrl = 'https://example.com/logo.jpg',
    String coverImageUrl = 'https://example.com/cover.jpg',
    double rating = 4.5,
    int reviewCount = 150,
    double deliveryFee = 5.0,
    int estimatedDeliveryTime = 30,
    double distance = 2.5,
    bool isOpen = true,
    String address = '123 Main St',
    String phoneNumber = '+905551234567',
    List<String> categories = const ['Restaurant'],
    double minimumOrderAmount = 20.0,
  }) {
    return Merchant(
      id: id,
      name: name,
      description: description,
      logoUrl: logoUrl,
      coverImageUrl: coverImageUrl,
      rating: rating,
      reviewCount: reviewCount,
      deliveryFee: deliveryFee,
      estimatedDeliveryTime: estimatedDeliveryTime,
      distance: distance,
      isOpen: isOpen,
      address: address,
      phoneNumber: phoneNumber,
      categories: categories,
      workingHours: const {},
      minimumOrderAmount: minimumOrderAmount,
      isDeliveryAvailable: true,
      isPickupAvailable: true,
      categoryType: null,
    );
  }
}
