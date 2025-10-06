class NotificationPreferences {
  final bool orderUpdates;
  final bool promotions;
  final bool system;
  final bool marketing;

  const NotificationPreferences({
    required this.orderUpdates,
    required this.promotions,
    required this.system,
    required this.marketing,
  });

  factory NotificationPreferences.fromJson(Map<String, dynamic> json) {
    return NotificationPreferences(
      orderUpdates:
          (json['orderUpdates'] ?? json['order_updates'] ?? true) as bool,
      promotions: (json['promotions'] ?? true) as bool,
      system: (json['system'] ?? true) as bool,
      marketing: (json['marketing'] ?? false) as bool,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'orderUpdates': orderUpdates,
      'promotions': promotions,
      'system': system,
      'marketing': marketing,
    };
  }

  NotificationPreferences copyWith({
    bool? orderUpdates,
    bool? promotions,
    bool? system,
    bool? marketing,
  }) {
    return NotificationPreferences(
      orderUpdates: orderUpdates ?? this.orderUpdates,
      promotions: promotions ?? this.promotions,
      system: system ?? this.system,
      marketing: marketing ?? this.marketing,
    );
  }
}
