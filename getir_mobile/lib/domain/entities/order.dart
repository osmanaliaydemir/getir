class Order {
  final String id;
  final String userId;
  final String merchantId;
  final String merchantName;
  final String? merchantLogoUrl;
  final String deliveryAddressId;
  final String deliveryAddress;
  final double deliveryLatitude;
  final double deliveryLongitude;
  final OrderStatus status;
  final PaymentStatus paymentStatus;
  final PaymentMethod paymentMethod;
  final double subtotal;
  final double deliveryFee;
  final double discountAmount;
  final double totalAmount;
  final String? couponCode;
  final String? notes;
  final DateTime estimatedDeliveryTime;
  final DateTime createdAt;
  final DateTime updatedAt;
  final List<OrderItem> items;
  final List<OrderStatusHistory> statusHistory;

  const Order({
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

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      id: json['id'] as String,
      userId: json['userId'] as String,
      merchantId: json['merchantId'] as String,
      merchantName: json['merchantName'] as String,
      merchantLogoUrl: json['merchantLogoUrl'] as String?,
      deliveryAddressId: json['deliveryAddressId'] as String,
      deliveryAddress: json['deliveryAddress'] as String,
      deliveryLatitude: (json['deliveryLatitude'] as num).toDouble(),
      deliveryLongitude: (json['deliveryLongitude'] as num).toDouble(),
      status: OrderStatus.fromString(json['status'] as String),
      paymentStatus: PaymentStatus.fromString(json['paymentStatus'] as String),
      paymentMethod: PaymentMethod.fromString(json['paymentMethod'] as String),
      subtotal: (json['subtotal'] as num).toDouble(),
      deliveryFee: (json['deliveryFee'] as num).toDouble(),
      discountAmount: (json['discountAmount'] as num).toDouble(),
      totalAmount: (json['totalAmount'] as num).toDouble(),
      couponCode: json['couponCode'] as String?,
      notes: json['notes'] as String?,
      estimatedDeliveryTime: DateTime.parse(json['estimatedDeliveryTime'] as String),
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
      items: (json['items'] as List<dynamic>)
          .map((item) => OrderItem.fromJson(item as Map<String, dynamic>))
          .toList(),
      statusHistory: (json['statusHistory'] as List<dynamic>)
          .map((status) => OrderStatusHistory.fromJson(status as Map<String, dynamic>))
          .toList(),
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
      'status': status.value,
      'paymentStatus': paymentStatus.value,
      'paymentMethod': paymentMethod.value,
      'subtotal': subtotal,
      'deliveryFee': deliveryFee,
      'discountAmount': discountAmount,
      'totalAmount': totalAmount,
      'couponCode': couponCode,
      'notes': notes,
      'estimatedDeliveryTime': estimatedDeliveryTime.toIso8601String(),
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'items': items.map((item) => item.toJson()).toList(),
      'statusHistory': statusHistory.map((status) => status.toJson()).toList(),
    };
  }
}

class OrderItem {
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

  const OrderItem({
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

  factory OrderItem.fromJson(Map<String, dynamic> json) {
    return OrderItem(
      id: json['id'] as String,
      productId: json['productId'] as String,
      productName: json['productName'] as String,
      productImageUrl: json['productImageUrl'] as String?,
      unitPrice: (json['unitPrice'] as num).toDouble(),
      quantity: json['quantity'] as int,
      totalPrice: (json['totalPrice'] as num).toDouble(),
      selectedVariantId: json['selectedVariantId'] as String?,
      selectedVariantName: json['selectedVariantName'] as String?,
      selectedOptionIds: (json['selectedOptionIds'] as List<dynamic>).cast<String>(),
      selectedOptionNames: (json['selectedOptionNames'] as List<dynamic>).cast<String>(),
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
}

class OrderStatusHistory {
  final String id;
  final String orderId;
  final OrderStatus status;
  final String? notes;
  final DateTime timestamp;

  const OrderStatusHistory({
    required this.id,
    required this.orderId,
    required this.status,
    this.notes,
    required this.timestamp,
  });

  factory OrderStatusHistory.fromJson(Map<String, dynamic> json) {
    return OrderStatusHistory(
      id: json['id'] as String,
      orderId: json['orderId'] as String,
      status: OrderStatus.fromString(json['status'] as String),
      notes: json['notes'] as String?,
      timestamp: DateTime.parse(json['timestamp'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'orderId': orderId,
      'status': status.value,
      'notes': notes,
      'timestamp': timestamp.toIso8601String(),
    };
  }
}

enum OrderStatus {
  pending('pending'),
  confirmed('confirmed'),
  preparing('preparing'),
  ready('ready'),
  onTheWay('onTheWay'),
  delivered('delivered'),
  cancelled('cancelled');

  const OrderStatus(this.value);
  final String value;

  static OrderStatus fromString(String value) {
    switch (value.toLowerCase()) {
      case 'pending':
        return OrderStatus.pending;
      case 'confirmed':
        return OrderStatus.confirmed;
      case 'preparing':
        return OrderStatus.preparing;
      case 'ready':
        return OrderStatus.ready;
      case 'ontheway':
        return OrderStatus.onTheWay;
      case 'delivered':
        return OrderStatus.delivered;
      case 'cancelled':
        return OrderStatus.cancelled;
      default:
        return OrderStatus.pending;
    }
  }

  String get displayName {
    switch (this) {
      case OrderStatus.pending:
        return 'Beklemede';
      case OrderStatus.confirmed:
        return 'Onaylandı';
      case OrderStatus.preparing:
        return 'Hazırlanıyor';
      case OrderStatus.ready:
        return 'Hazır';
      case OrderStatus.onTheWay:
        return 'Yolda';
      case OrderStatus.delivered:
        return 'Teslim Edildi';
      case OrderStatus.cancelled:
        return 'İptal Edildi';
    }
  }
}

enum PaymentStatus {
  pending('pending'),
  paid('paid'),
  failed('failed'),
  refunded('refunded');

  const PaymentStatus(this.value);
  final String value;

  static PaymentStatus fromString(String value) {
    switch (value.toLowerCase()) {
      case 'pending':
        return PaymentStatus.pending;
      case 'paid':
        return PaymentStatus.paid;
      case 'failed':
        return PaymentStatus.failed;
      case 'refunded':
        return PaymentStatus.refunded;
      default:
        return PaymentStatus.pending;
    }
  }

  String get displayName {
    switch (this) {
      case PaymentStatus.pending:
        return 'Beklemede';
      case PaymentStatus.paid:
        return 'Ödendi';
      case PaymentStatus.failed:
        return 'Başarısız';
      case PaymentStatus.refunded:
        return 'İade Edildi';
    }
  }
}

enum PaymentMethod {
  cash('cash'),
  card('card'),
  online('online');

  const PaymentMethod(this.value);
  final String value;

  static PaymentMethod fromString(String value) {
    switch (value.toLowerCase()) {
      case 'cash':
        return PaymentMethod.cash;
      case 'card':
        return PaymentMethod.card;
      case 'online':
        return PaymentMethod.online;
      default:
        return PaymentMethod.cash;
    }
  }

  String get displayName {
    switch (this) {
      case PaymentMethod.cash:
        return 'Nakit';
      case PaymentMethod.card:
        return 'Kart';
      case PaymentMethod.online:
        return 'Online';
    }
  }
}
