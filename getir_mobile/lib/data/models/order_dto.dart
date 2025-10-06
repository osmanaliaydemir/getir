import '../../../domain/entities/order.dart';

class OrderDto {
  final String id;
  final String userId;
  final String merchantId;
  final String merchantName;
  final String? merchantLogoUrl;
  final String deliveryAddressId;
  final String deliveryAddress;
  final double deliveryLatitude;
  final double deliveryLongitude;
  final String status;
  final String paymentStatus;
  final String paymentMethod;
  final double subtotal;
  final double deliveryFee;
  final double discountAmount;
  final double totalAmount;
  final String? couponCode;
  final String? notes;
  final DateTime estimatedDeliveryTime;
  final DateTime createdAt;
  final DateTime updatedAt;
  final List<OrderItemDto> items;
  final List<OrderStatusHistoryDto> statusHistory;

  const OrderDto({
    required this.id,
    required this.userId,
    required this.merchantId,
    required this.merchantName,
    this.merchantLogoUrl,
    required this.deliveryAddressId,
    required this.deliveryAddress,
    required this.deliveryLatitude,
    required this.deliveryLongitude,
    required this.status,
    required this.paymentStatus,
    required this.paymentMethod,
    required this.subtotal,
    required this.deliveryFee,
    required this.discountAmount,
    required this.totalAmount,
    this.couponCode,
    this.notes,
    required this.estimatedDeliveryTime,
    required this.createdAt,
    required this.updatedAt,
    required this.items,
    required this.statusHistory,
  });

  factory OrderDto.fromJson(Map<String, dynamic> json) {
    return OrderDto(
      id: (json['id'] ?? '').toString(),
      userId: (json['userId'] ?? '').toString(),
      merchantId: (json['merchantId'] ?? '').toString(),
      merchantName: (json['merchantName'] ?? '').toString(),
      merchantLogoUrl: json['merchantLogoUrl'] as String?,
      deliveryAddressId: (json['deliveryAddressId'] ?? '').toString(),
      deliveryAddress: (json['deliveryAddress'] ?? '').toString(),
      deliveryLatitude: json['deliveryLatitude'] is num
          ? (json['deliveryLatitude'] as num).toDouble()
          : 0.0,
      deliveryLongitude: json['deliveryLongitude'] is num
          ? (json['deliveryLongitude'] as num).toDouble()
          : 0.0,
      status: (json['status'] ?? 'pending').toString(),
      paymentStatus: (json['paymentStatus'] ?? 'pending').toString(),
      paymentMethod: (json['paymentMethod'] ?? 'cash').toString(),
      subtotal: json['subtotal'] is num
          ? (json['subtotal'] as num).toDouble()
          : 0.0,
      deliveryFee: json['deliveryFee'] is num
          ? (json['deliveryFee'] as num).toDouble()
          : 0.0,
      discountAmount: json['discountAmount'] is num
          ? (json['discountAmount'] as num).toDouble()
          : 0.0,
      totalAmount: json['totalAmount'] is num
          ? (json['totalAmount'] as num).toDouble()
          : 0.0,
      couponCode: json['couponCode'] as String?,
      notes: json['notes'] as String?,
      estimatedDeliveryTime: DateTime.parse(
        (json['estimatedDeliveryTime'] ?? DateTime.now().toIso8601String())
            .toString(),
      ),
      createdAt: DateTime.parse(
        (json['createdAt'] ?? DateTime.now().toIso8601String()).toString(),
      ),
      updatedAt: DateTime.parse(
        (json['updatedAt'] ?? DateTime.now().toIso8601String()).toString(),
      ),
      items:
          (json['items'] as List<dynamic>?)
              ?.map((e) => OrderItemDto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const <OrderItemDto>[],
      statusHistory:
          (json['statusHistory'] as List<dynamic>?)
              ?.map(
                (e) =>
                    OrderStatusHistoryDto.fromJson(e as Map<String, dynamic>),
              )
              .toList() ??
          const <OrderStatusHistoryDto>[],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'merchantId': merchantId,
      'merchantName': merchantName,
      'merchantLogoUrl': merchantLogoUrl,
      'deliveryAddressId': deliveryAddressId,
      'deliveryAddress': deliveryAddress,
      'deliveryLatitude': deliveryLatitude,
      'deliveryLongitude': deliveryLongitude,
      'status': status,
      'paymentStatus': paymentStatus,
      'paymentMethod': paymentMethod,
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'discountAmount': discountAmount,
      'totalAmount': totalAmount,
      'couponCode': couponCode,
      'notes': notes,
      'estimatedDeliveryTime': estimatedDeliveryTime.toIso8601String(),
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'items': items.map((e) => e.toJson()).toList(),
      'statusHistory': statusHistory.map((e) => e.toJson()).toList(),
    };
  }

  Order toDomain() {
    return Order(
      id: id,
      userId: userId,
      merchantId: merchantId,
      merchantName: merchantName,
      merchantLogoUrl: merchantLogoUrl,
      deliveryAddressId: deliveryAddressId,
      deliveryAddress: deliveryAddress,
      deliveryLatitude: deliveryLatitude,
      deliveryLongitude: deliveryLongitude,
      status: OrderStatus.fromString(status),
      paymentStatus: PaymentStatus.fromString(paymentStatus),
      paymentMethod: PaymentMethod.fromString(paymentMethod),
      subtotal: subtotal,
      deliveryFee: deliveryFee,
      discountAmount: discountAmount,
      totalAmount: totalAmount,
      couponCode: couponCode,
      notes: notes,
      estimatedDeliveryTime: estimatedDeliveryTime,
      createdAt: createdAt,
      updatedAt: updatedAt,
      items: items.map((e) => e.toDomain()).toList(),
      statusHistory: statusHistory.map((e) => e.toDomain()).toList(),
    );
  }
}

class OrderItemDto {
  final String id;
  final String productId;
  final String productName;
  final String? productImageUrl;
  final double unitPrice;
  final int quantity;
  final double totalPrice;
  final String? selectedVariantId;
  final String? selectedVariantName;
  final List<String> selectedOptionIds;
  final List<String> selectedOptionNames;

  const OrderItemDto({
    required this.id,
    required this.productId,
    required this.productName,
    this.productImageUrl,
    required this.unitPrice,
    required this.quantity,
    required this.totalPrice,
    this.selectedVariantId,
    this.selectedVariantName,
    required this.selectedOptionIds,
    required this.selectedOptionNames,
  });

  factory OrderItemDto.fromJson(Map<String, dynamic> json) {
    return OrderItemDto(
      id: (json['id'] ?? '').toString(),
      productId: (json['productId'] ?? '').toString(),
      productName: (json['productName'] ?? '').toString(),
      productImageUrl: json['productImageUrl'] as String?,
      unitPrice: json['unitPrice'] is num
          ? (json['unitPrice'] as num).toDouble()
          : 0.0,
      quantity: json['quantity'] is int ? json['quantity'] as int : 0,
      totalPrice: json['totalPrice'] is num
          ? (json['totalPrice'] as num).toDouble()
          : 0.0,
      selectedVariantId: json['selectedVariantId'] as String?,
      selectedVariantName: json['selectedVariantName'] as String?,
      selectedOptionIds:
          (json['selectedOptionIds'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          const <String>[],
      selectedOptionNames:
          (json['selectedOptionNames'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          const <String>[],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'productId': productId,
      'productName': productName,
      'productImageUrl': productImageUrl,
      'unitPrice': unitPrice,
      'quantity': quantity,
      'totalPrice': totalPrice,
      'selectedVariantId': selectedVariantId,
      'selectedVariantName': selectedVariantName,
      'selectedOptionIds': selectedOptionIds,
      'selectedOptionNames': selectedOptionNames,
    };
  }

  OrderItem toDomain() {
    return OrderItem(
      id: id,
      productId: productId,
      productName: productName,
      productImageUrl: productImageUrl,
      unitPrice: unitPrice,
      quantity: quantity,
      totalPrice: totalPrice,
      selectedVariantId: selectedVariantId,
      selectedVariantName: selectedVariantName,
      selectedOptionIds: selectedOptionIds,
      selectedOptionNames: selectedOptionNames,
    );
  }
}

class OrderStatusHistoryDto {
  final String id;
  final String orderId;
  final String status;
  final String? notes;
  final DateTime timestamp;

  const OrderStatusHistoryDto({
    required this.id,
    required this.orderId,
    required this.status,
    this.notes,
    required this.timestamp,
  });

  factory OrderStatusHistoryDto.fromJson(Map<String, dynamic> json) {
    return OrderStatusHistoryDto(
      id: (json['id'] ?? '').toString(),
      orderId: (json['orderId'] ?? '').toString(),
      status: (json['status'] ?? 'pending').toString(),
      notes: json['notes'] as String?,
      timestamp: DateTime.parse(
        (json['timestamp'] ?? DateTime.now().toIso8601String()).toString(),
      ),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderId': orderId,
      'status': status,
      'notes': notes,
      'timestamp': timestamp.toIso8601String(),
    };
  }

  OrderStatusHistory toDomain() {
    return OrderStatusHistory(
      id: id,
      orderId: orderId,
      status: OrderStatus.fromString(status),
      notes: notes,
      timestamp: timestamp,
    );
  }
}
