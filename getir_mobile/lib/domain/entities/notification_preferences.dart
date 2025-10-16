import 'package:equatable/equatable.dart';

class NotificationPreferences extends Equatable {
  final String id;
  final String userId;

  // Email preferences
  final bool emailEnabled;
  final bool emailOrderUpdates;
  final bool emailPromotions;
  final bool emailNewsletter;
  final bool emailSecurityAlerts;

  // SMS preferences
  final bool smsEnabled;
  final bool smsOrderUpdates;
  final bool smsPromotions;
  final bool smsSecurityAlerts;

  // Push notification preferences
  final bool pushEnabled;
  final bool pushOrderUpdates;
  final bool pushPromotions;
  final bool pushMerchantUpdates;
  final bool pushSecurityAlerts;

  // Language preference
  final String language;

  // Timestamps
  final DateTime createdAt;
  final DateTime updatedAt;

  const NotificationPreferences({
    required this.id,
    required this.userId,
    required this.emailEnabled,
    required this.emailOrderUpdates,
    required this.emailPromotions,
    required this.emailNewsletter,
    required this.emailSecurityAlerts,
    required this.smsEnabled,
    required this.smsOrderUpdates,
    required this.smsPromotions,
    required this.smsSecurityAlerts,
    required this.pushEnabled,
    required this.pushOrderUpdates,
    required this.pushPromotions,
    required this.pushMerchantUpdates,
    required this.pushSecurityAlerts,
    required this.language,
    required this.createdAt,
    required this.updatedAt,
  });

  factory NotificationPreferences.fromJson(Map<String, dynamic> json) {
    return NotificationPreferences(
      id: json['id'] as String,
      userId: json['userId'] as String,
      emailEnabled: json['emailEnabled'] as bool? ?? true,
      emailOrderUpdates: json['emailOrderUpdates'] as bool? ?? true,
      emailPromotions: json['emailPromotions'] as bool? ?? true,
      emailNewsletter: json['emailNewsletter'] as bool? ?? true,
      emailSecurityAlerts: json['emailSecurityAlerts'] as bool? ?? true,
      smsEnabled: json['smsEnabled'] as bool? ?? true,
      smsOrderUpdates: json['smsOrderUpdates'] as bool? ?? true,
      smsPromotions: json['smsPromotions'] as bool? ?? false,
      smsSecurityAlerts: json['smsSecurityAlerts'] as bool? ?? true,
      pushEnabled: json['pushEnabled'] as bool? ?? true,
      pushOrderUpdates: json['pushOrderUpdates'] as bool? ?? true,
      pushPromotions: json['pushPromotions'] as bool? ?? true,
      pushMerchantUpdates: json['pushMerchantUpdates'] as bool? ?? true,
      pushSecurityAlerts: json['pushSecurityAlerts'] as bool? ?? true,
      language: json['language'] as String? ?? 'tr-TR',
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: DateTime.parse(json['updatedAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      // Note: id, userId, createdAt, updatedAt are removed in datasource before sending to API
      'emailEnabled': emailEnabled,
      'emailOrderUpdates': emailOrderUpdates,
      'emailPromotions': emailPromotions,
      'emailNewsletter': emailNewsletter,
      'emailSecurityAlerts': emailSecurityAlerts,
      'smsEnabled': smsEnabled,
      'smsOrderUpdates': smsOrderUpdates,
      'smsPromotions': smsPromotions,
      'smsSecurityAlerts': smsSecurityAlerts,
      'pushEnabled': pushEnabled,
      'pushOrderUpdates': pushOrderUpdates,
      'pushPromotions': pushPromotions,
      'pushMerchantUpdates': pushMerchantUpdates,
      'pushSecurityAlerts': pushSecurityAlerts,
      'language': language,
    };
  }

  NotificationPreferences copyWith({
    bool? emailEnabled,
    bool? emailOrderUpdates,
    bool? emailPromotions,
    bool? emailNewsletter,
    bool? emailSecurityAlerts,
    bool? smsEnabled,
    bool? smsOrderUpdates,
    bool? smsPromotions,
    bool? smsSecurityAlerts,
    bool? pushEnabled,
    bool? pushOrderUpdates,
    bool? pushPromotions,
    bool? pushMerchantUpdates,
    bool? pushSecurityAlerts,
    String? language,
  }) {
    return NotificationPreferences(
      id: id,
      userId: userId,
      emailEnabled: emailEnabled ?? this.emailEnabled,
      emailOrderUpdates: emailOrderUpdates ?? this.emailOrderUpdates,
      emailPromotions: emailPromotions ?? this.emailPromotions,
      emailNewsletter: emailNewsletter ?? this.emailNewsletter,
      emailSecurityAlerts: emailSecurityAlerts ?? this.emailSecurityAlerts,
      smsEnabled: smsEnabled ?? this.smsEnabled,
      smsOrderUpdates: smsOrderUpdates ?? this.smsOrderUpdates,
      smsPromotions: smsPromotions ?? this.smsPromotions,
      smsSecurityAlerts: smsSecurityAlerts ?? this.smsSecurityAlerts,
      pushEnabled: pushEnabled ?? this.pushEnabled,
      pushOrderUpdates: pushOrderUpdates ?? this.pushOrderUpdates,
      pushPromotions: pushPromotions ?? this.pushPromotions,
      pushMerchantUpdates: pushMerchantUpdates ?? this.pushMerchantUpdates,
      pushSecurityAlerts: pushSecurityAlerts ?? this.pushSecurityAlerts,
      language: language ?? this.language,
      createdAt: createdAt,
      updatedAt: DateTime.now(),
    );
  }

  @override
  List<Object?> get props => [
    id,
    userId,
    emailEnabled,
    emailOrderUpdates,
    emailPromotions,
    emailNewsletter,
    emailSecurityAlerts,
    smsEnabled,
    smsOrderUpdates,
    smsPromotions,
    smsSecurityAlerts,
    pushEnabled,
    pushOrderUpdates,
    pushPromotions,
    pushMerchantUpdates,
    pushSecurityAlerts,
    language,
    createdAt,
    updatedAt,
  ];
}
