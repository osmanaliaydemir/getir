import '../../domain/entities/notification.dart';

class AppNotificationDto {
  final String id;
  final String title;
  final String body;
  final String type;
  final DateTime createdAt;
  final bool isRead;
  final Map<String, dynamic> data;

  const AppNotificationDto({
    required this.id,
    required this.title,
    required this.body,
    required this.type,
    required this.createdAt,
    required this.isRead,
    required this.data,
  });

  factory AppNotificationDto.fromJson(Map<String, dynamic> json) {
    return AppNotificationDto(
      id: (json['id'] ?? '').toString(),
      title: (json['title'] ?? '').toString(),
      body: (json['body'] ?? '').toString(),
      type: (json['type'] ?? json['notification_type'] ?? 'general').toString(),
      createdAt: DateTime.parse(
        (json['createdAt'] ??
                json['created_at'] ??
                DateTime.now().toIso8601String())
            .toString(),
      ),
      isRead: json['isRead'] == true || json['read'] == true,
      data: json['data'] is Map<String, dynamic>
          ? (json['data'] as Map<String, dynamic>)
          : <String, dynamic>{},
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'body': body,
      'type': type,
      'createdAt': createdAt.toIso8601String(),
      'isRead': isRead,
      'data': data,
    };
  }

  AppNotification toDomain() {
    return AppNotification(
      id: id,
      title: title,
      body: body,
      type: type,
      createdAt: createdAt,
      isRead: isRead,
      data: data,
    );
  }
  
  /// Convert from Domain Entity (for API requests)
  factory AppNotificationDto.fromDomain(AppNotification notification) {
    return AppNotificationDto(
      id: notification.id,
      title: notification.title,
      body: notification.body,
      type: notification.type,
      createdAt: notification.createdAt,
      isRead: notification.isRead,
      data: notification.data,
    );
  }
}
