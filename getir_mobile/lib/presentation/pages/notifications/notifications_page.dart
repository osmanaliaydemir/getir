import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/widgets/error_state_widget.dart';
import '../../../domain/entities/notification.dart';
import '../../bloc/notifications_feed/notifications_feed_bloc.dart';
import '../../widgets/notification/notification_card.dart';
import '../../widgets/notification/notification_card_skeleton.dart';

class NotificationsPage extends StatefulWidget {
  const NotificationsPage({super.key});

  @override
  State<NotificationsPage> createState() => _NotificationsPageState();
}

class _NotificationsPageState extends State<NotificationsPage> {
  @override
  void initState() {
    super.initState();
    // Load notifications on page init
    context.read<NotificationsFeedBloc>().add(const LoadNotificationsFeed());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      appBar: AppBar(
        title: const Text('Bildirimler'),
        backgroundColor: Theme.of(context).colorScheme.primary,
        foregroundColor: Theme.of(context).colorScheme.onPrimary,
        elevation: 0,
        actions: [
          // Mark all as read button
          IconButton(
            icon: const Icon(Icons.done_all),
            tooltip: 'Tümünü Okundu İşaretle',
            onPressed: () {
              _showMarkAllReadDialog(context);
            },
          ),
        ],
      ),
      body: BlocBuilder<NotificationsFeedBloc, NotificationsFeedState>(
        builder: (context, state) {
          if (state is NotificationsFeedLoading) {
            return const NotificationListSkeleton(itemCount: 8);
          }

          if (state is NotificationsFeedError) {
            return ErrorStateWidget(
              errorType: _getErrorTypeFromMessage(state.message),
              customMessage: state.message,
              onRetry: () {
                context.read<NotificationsFeedBloc>().add(
                  const LoadNotificationsFeed(),
                );
              },
            );
          }

          if (state is NotificationsFeedLoaded) {
            if (state.items.isEmpty) {
              return EmptyStateWidget(
                icon: Icons.notifications_none,
                title: 'Henüz Bildiriminiz Yok',
                message:
                    'Sipariş güncellemeleri ve kampanyalar hakkında bildirim alacaksınız.',
              );
            }

            // Unread count
            final unreadCount = state.items.where((n) => !n.isRead).length;

            return Column(
              children: [
                // Unread count banner (if any)
                if (unreadCount > 0)
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.symmetric(
                      horizontal: 16,
                      vertical: 12,
                    ),
                    color: AppColors.primary.withOpacity(0.1),
                    child: Row(
                      children: [
                        Icon(
                          Icons.mark_email_unread,
                          color: AppColors.primary,
                          size: 20,
                        ),
                        const SizedBox(width: 8),
                        Text(
                          '$unreadCount okunmamış bildirim',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ],
                    ),
                  ),

                // Notifications List
                Expanded(
                  child: RefreshIndicator(
                    onRefresh: () async {
                      context.read<NotificationsFeedBloc>().add(
                        const LoadNotificationsFeed(),
                      );
                    },
                    color: AppColors.primary,
                    child: ListView.builder(
                      padding: const EdgeInsets.symmetric(vertical: 8),
                      itemCount: state.items.length,
                      itemBuilder: (context, index) {
                        final notification = state.items[index];
                        return NotificationCard(
                          notification: notification,
                          onTap: () {
                            _handleNotificationTap(context, notification);
                          },
                        );
                      },
                    ),
                  ),
                ),
              ],
            );
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  void _showMarkAllReadDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Tümünü Okundu İşaretle'),
        content: const Text('Tüm bildirimler okundu olarak işaretlensin mi?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(dialogContext),
            child: const Text('İptal'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(dialogContext);
              // TODO: Implement mark all as read
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Tüm bildirimler okundu olarak işaretlendi'),
                  backgroundColor: AppColors.success,
                ),
              );
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: AppColors.white,
            ),
            child: const Text('Onayla'),
          ),
        ],
      ),
    );
  }

  void _handleNotificationTap(
    BuildContext context,
    AppNotification notification,
  ) {
    // Handle navigation based on notification type and data
    final type = notification.type.toLowerCase();
    final data = notification.data;

    if (type == 'order_update' || type == 'order') {
      final orderId = data['orderId'] ?? data['order_id'];
      if (orderId != null) {
        Navigator.pushNamed(context, '/order/$orderId/tracking');
      }
    } else if (type == 'promotion' || type == 'campaign') {
      final merchantId = data['merchantId'] ?? data['merchant_id'];
      if (merchantId != null) {
        Navigator.pushNamed(context, '/merchant/$merchantId');
      }
    }
    // Add more navigation logic as needed
  }

  ErrorType _getErrorTypeFromMessage(String message) {
    final lowerMessage = message.toLowerCase();

    if (lowerMessage.contains('network') ||
        lowerMessage.contains('connection') ||
        lowerMessage.contains('internet') ||
        lowerMessage.contains('bağlantı')) {
      return ErrorType.network;
    } else if (lowerMessage.contains('500') ||
        lowerMessage.contains('502') ||
        lowerMessage.contains('503') ||
        lowerMessage.contains('server') ||
        lowerMessage.contains('sunucu')) {
      return ErrorType.server;
    } else if (lowerMessage.contains('401') ||
        lowerMessage.contains('403') ||
        lowerMessage.contains('unauthorized') ||
        lowerMessage.contains('yetkisiz')) {
      return ErrorType.unauthorized;
    }

    return ErrorType.generic;
  }
}
