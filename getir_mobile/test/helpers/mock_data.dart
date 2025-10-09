import 'package:getir_mobile/domain/entities/user_entity.dart';
import 'package:getir_mobile/domain/entities/merchant.dart';
import 'package:getir_mobile/domain/entities/product.dart';
import 'package:getir_mobile/domain/entities/cart.dart';
import 'package:getir_mobile/domain/entities/address.dart';
import 'package:getir_mobile/domain/entities/order.dart';

/// Mock data for testing
/// Provides consistent test fixtures
class MockData {
  // ==================== USER ====================

  static UserEntity get testUser => UserEntity(
    id: 'test-user-123',
    email: 'test@getir.com',
    firstName: 'Test',
    lastName: 'User',
    phoneNumber: '+905551234567',
    role: 'customer',
    isEmailVerified: true,
    isActive: true,
    createdAt: DateTime(2025, 1, 1),
    updatedAt: DateTime(2025, 1, 1),
    lastLoginAt: DateTime(2025, 10, 7),
  );

  static UserEntity get testUser2 => UserEntity(
    id: 'test-user-456',
    email: 'user2@getir.com',
    firstName: 'Another',
    lastName: 'User',
    phoneNumber: '+905559876543',
    role: 'customer',
    isEmailVerified: false,
    isActive: true,
    createdAt: DateTime(2025, 2, 1),
    updatedAt: DateTime(2025, 2, 1),
    lastLoginAt: null,
  );

  // ==================== MERCHANT ====================

  static Merchant get testMerchant => Merchant(
    id: 'merchant-123',
    name: 'Test Market',
    description: 'En taze ürünler burada',
    logoUrl: 'https://via.placeholder.com/150',
    coverImageUrl: 'https://via.placeholder.com/400',
    rating: 4.5,
    reviewCount: 120,
    deliveryFee: 9.99,
    estimatedDeliveryTime: 15,
    distance: 2.5,
    isOpen: true,
    address: 'Test Mahallesi, Test Sokak No:1',
    phoneNumber: '+902121234567',
    categories: ['Market', 'Bakkal'],
    workingHours: {'monday': '08:00-23:00', 'tuesday': '08:00-23:00'},
    minimumOrderAmount: 50.0,
    isDeliveryAvailable: true,
    isPickupAvailable: false,
  );

  // ==================== PRODUCT ====================

  static Product get testProduct => Product(
    id: 'product-123',
    merchantId: 'merchant-123',
    merchantName: 'Test Market',
    name: 'Test Ürün',
    description: 'Test açıklaması',
    price: 29.99,
    imageUrl: 'https://via.placeholder.com/200',
    category: 'İçecek',
    isAvailable: true,
    stockQuantity: 100,
    variants: [],
    options: [],
    rating: 4.2,
    reviewCount: 45,
    unit: 'adet',
    tags: ['test', 'icecek'],
    nutritionalInfo: {},
    brand: 'Test Brand',
    barcode: '1234567890',
  );

  static Product get testProduct2 => Product(
    id: 'product-456',
    merchantId: 'merchant-123',
    merchantName: 'Test Market',
    name: 'Test Ürün 2',
    description: 'Başka bir test ürünü',
    price: 19.99,
    imageUrl: 'https://via.placeholder.com/200',
    category: 'Gıda',
    isAvailable: true,
    stockQuantity: 50,
    variants: [],
    options: [],
    rating: 4.8,
    reviewCount: 200,
    unit: 'kg',
    tags: ['test', 'gida'],
    nutritionalInfo: {},
    brand: 'Test Brand',
    barcode: '0987654321',
  );

  // ==================== CART ====================

  static Cart get emptyCart => Cart(
    id: 'cart-empty',
    userId: 'test-user-123',
    items: [],
    subtotal: 0.0,
    deliveryFee: 0.0,
    total: 0.0,
    couponCode: null,
    discountAmount: 0.0,
    createdAt: now,
    updatedAt: now,
  );

  static Cart get testCart => Cart(
    id: 'cart-123',
    userId: 'test-user-123',
    items: [testCartItem],
    subtotal: 29.99,
    deliveryFee: 9.99,
    total: 39.98,
    couponCode: null,
    discountAmount: 0.0,
    createdAt: now,
    updatedAt: now,
  );

  static CartItem get testCartItem => CartItem(
    id: 'cart-item-123',
    productId: 'product-123',
    productName: 'Test Ürün',
    productImageUrl: 'https://via.placeholder.com/200',
    unitPrice: 29.99,
    quantity: 1,
    totalPrice: 29.99,
    selectedVariantId: null,
    selectedVariantName: null,
    selectedOptionIds: [],
    selectedOptionNames: [],
    merchantId: 'merchant-123',
    merchantName: 'Test Market',
    addedAt: now,
  );

  // ==================== ADDRESS ====================

  static UserAddress get testAddress => UserAddress(
    id: 'address-123',
    userId: 'test-user-123',
    title: 'Ev',
    fullAddress: 'Test Mahallesi, Test Sokak, No: 123, Kadıköy/İstanbul',
    buildingNumber: '123',
    floor: '3',
    apartment: '5',
    landmark: 'Test Market yanı',
    latitude: 40.9923,
    longitude: 29.0287,
    type: AddressType.home,
    isDefault: true,
    createdAt: DateTime(2025, 1, 1),
    updatedAt: DateTime(2025, 1, 1),
  );

  // ==================== TEST CREDENTIALS ====================

  static const String testEmail = 'test@getir.com';
  static const String testPassword = 'Test123456';
  static const String testInvalidEmail = 'invalid-email';
  static const String testShortPassword = '12345';
  static const String testEmptyString = '';

  // ==================== TEST TOKENS ====================

  static const String testAccessToken = 'test-access-token-123';
  static const String testRefreshToken = 'test-refresh-token-456';
  static const String testExpiredToken = 'expired-token-789';

  // ==================== ORDER ====================

  static Order get testOrder => Order(
    id: 'order-123',
    userId: 'test-user-123',
    merchantId: 'merchant-123',
    merchantName: 'Test Market',
    merchantLogoUrl: 'https://via.placeholder.com/150',
    deliveryAddressId: 'address-123',
    deliveryAddress: 'Test Mahallesi, Test Sokak, No: 123, Kadıköy/İstanbul',
    deliveryLatitude: 40.9923,
    deliveryLongitude: 29.0287,
    status: OrderStatus.pending,
    paymentStatus: PaymentStatus.pending,
    paymentMethod: PaymentMethod.card,
    subtotal: 29.99,
    deliveryFee: 9.99,
    discountAmount: 0.0,
    totalAmount: 39.98,
    couponCode: null,
    notes: 'Test sipariş notu',
    estimatedDeliveryTime: DateTime(2025, 10, 7, 12, 30),
    createdAt: DateTime(2025, 10, 7, 12, 0),
    updatedAt: DateTime(2025, 10, 7, 12, 0),
    items: [testOrderItem],
    statusHistory: [testOrderStatusHistory],
  );

  static OrderItem get testOrderItem => const OrderItem(
    id: 'order-item-123',
    productId: 'product-123',
    productName: 'Test Ürün',
    productImageUrl: 'https://via.placeholder.com/200',
    unitPrice: 29.99,
    quantity: 1,
    totalPrice: 29.99,
    selectedVariantId: null,
    selectedVariantName: null,
    selectedOptionIds: [],
    selectedOptionNames: [],
  );

  static OrderStatusHistory get testOrderStatusHistory => OrderStatusHistory(
    id: 'status-history-123',
    orderId: 'order-123',
    status: OrderStatus.pending,
    notes: 'Sipariş oluşturuldu',
    timestamp: DateTime(2025, 10, 7, 12, 0),
  );

  // ==================== TEST DATES ====================

  static DateTime get now => DateTime(2025, 10, 7, 12, 0);
  static DateTime get yesterday => now.subtract(const Duration(days: 1));
  static DateTime get tomorrow => now.add(const Duration(days: 1));
  static DateTime get lastWeek => now.subtract(const Duration(days: 7));
}
