import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../bloc/notifications_feed/notifications_feed_bloc.dart';

class NotificationsPage extends StatelessWidget {
  const NotificationsPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Bildirimler'),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: BlocBuilder<NotificationsFeedBloc, NotificationsFeedState>(
        builder: (context, state) {
          if (state is NotificationsFeedInitial) {
            context.read<NotificationsFeedBloc>().add(
              const LoadNotificationsFeed(),
            );
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }
          if (state is NotificationsFeedLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }
          if (state is NotificationsFeedError) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(
                      Icons.error_outline,
                      color: AppColors.error,
                      size: 48,
                    ),
                    const SizedBox(height: 12),
                    Text(state.message, textAlign: TextAlign.center),
                    const SizedBox(height: 12),
                    ElevatedButton(
                      onPressed: () => context
                          .read<NotificationsFeedBloc>()
                          .add(const LoadNotificationsFeed()),
                      child: const Text('Tekrar Dene'),
                    ),
                  ],
                ),
              ),
            );
          }
          if (state is NotificationsFeedLoaded) {
            if (state.items.isEmpty) {
              return const Center(child: Text('Henüz bildiriminiz yok'));
            }
            return ListView.separated(
              padding: const EdgeInsets.all(12),
              itemCount: state.items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 8),
              itemBuilder: (context, index) {
                final n = state.items[index];
                return ListTile(
                  tileColor: AppColors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  leading: Icon(
                    n.isRead
                        ? Icons.notifications_none
                        : Icons.notifications_active,
                    color: n.isRead
                        ? AppColors.textSecondary
                        : AppColors.primary,
                  ),
                  title: Text(n.title, style: AppTypography.bodyLarge),
                  subtitle: Text(
                    n.body,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  trailing: n.isRead
                      ? null
                      : TextButton(
                          onPressed: () => context
                              .read<NotificationsFeedBloc>()
                              .add(MarkNotificationRead(n.id)),
                          child: const Text('Okundu'),
                        ),
                );
              },
            );
          }
          return const SizedBox.shrink();
        },
      ),
    );
  }
}
