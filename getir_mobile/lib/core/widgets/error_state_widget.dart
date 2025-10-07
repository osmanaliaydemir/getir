import 'package:flutter/material.dart';
import '../theme/app_colors.dart';
import '../theme/app_typography.dart';

/// Error types for different scenarios
enum ErrorType { network, server, notFound, unauthorized, generic }

/// Custom error state widget with retry functionality
class ErrorStateWidget extends StatelessWidget {
  final ErrorType errorType;
  final String? customMessage;
  final String? customTitle;
  final VoidCallback? onRetry;
  final IconData? customIcon;
  final Color? customIconColor;

  const ErrorStateWidget({
    super.key,
    this.errorType = ErrorType.generic,
    this.customMessage,
    this.customTitle,
    this.onRetry,
    this.customIcon,
    this.customIconColor,
  });

  @override
  Widget build(BuildContext context) {
    final config = _getErrorConfig(context);

    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Error Icon with animation
            TweenAnimationBuilder<double>(
              tween: Tween(begin: 0.0, end: 1.0),
              duration: const Duration(milliseconds: 400),
              curve: Curves.easeOutBack,
              builder: (context, value, child) {
                return Transform.scale(
                  scale: value,
                  child: Container(
                    width: 100,
                    height: 100,
                    decoration: BoxDecoration(
                      color: config.iconColor.withOpacity(0.1),
                      shape: BoxShape.circle,
                    ),
                    child: Icon(config.icon, size: 60, color: config.iconColor),
                  ),
                );
              },
            ),

            const SizedBox(height: 24),

            // Error Title
            Text(
              config.title,
              style: AppTypography.headlineSmall.copyWith(
                fontWeight: FontWeight.w600,
                color: Theme.of(context).colorScheme.onSurface,
              ),
              textAlign: TextAlign.center,
            ),

            const SizedBox(height: 12),

            // Error Message
            Text(
              config.message,
              style: AppTypography.bodyMedium.copyWith(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),

            if (onRetry != null) ...[
              const SizedBox(height: 32),

              // Retry Button
              ElevatedButton.icon(
                onPressed: onRetry,
                icon: const Icon(Icons.refresh),
                label: const Text('Tekrar Dene'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: Theme.of(context).colorScheme.primary,
                  foregroundColor: Theme.of(context).colorScheme.onPrimary,
                  padding: const EdgeInsets.symmetric(
                    horizontal: 32,
                    vertical: 16,
                  ),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  _ErrorConfig _getErrorConfig(BuildContext context) {
    switch (errorType) {
      case ErrorType.network:
        return _ErrorConfig(
          icon: customIcon ?? Icons.wifi_off_rounded,
          iconColor: customIconColor ?? AppColors.warning,
          title: customTitle ?? 'Bağlantı Hatası',
          message:
              customMessage ??
              'İnternet bağlantınızı kontrol edin ve tekrar deneyin.',
        );

      case ErrorType.server:
        return _ErrorConfig(
          icon: customIcon ?? Icons.error_outline_rounded,
          iconColor: customIconColor ?? AppColors.error,
          title: customTitle ?? 'Sunucu Hatası',
          message:
              customMessage ??
              'Bir şeyler yanlış gitti. Lütfen daha sonra tekrar deneyin.',
        );

      case ErrorType.notFound:
        return _ErrorConfig(
          icon: customIcon ?? Icons.search_off_rounded,
          iconColor: customIconColor ?? AppColors.textSecondary,
          title: customTitle ?? 'İçerik Bulunamadı',
          message:
              customMessage ??
              'Aradığınız içerik bulunamadı veya kaldırılmış olabilir.',
        );

      case ErrorType.unauthorized:
        return _ErrorConfig(
          icon: customIcon ?? Icons.lock_outline_rounded,
          iconColor: customIconColor ?? AppColors.error,
          title: customTitle ?? 'Yetki Hatası',
          message:
              customMessage ??
              'Bu işlemi gerçekleştirmek için yetkiniz bulunmuyor.',
        );

      case ErrorType.generic:
        return _ErrorConfig(
          icon: customIcon ?? Icons.error_outline_rounded,
          iconColor: customIconColor ?? AppColors.textSecondary,
          title: customTitle ?? 'Bir Hata Oluştu',
          message:
              customMessage ??
              'Bir şeyler yanlış gitti. Lütfen tekrar deneyin.',
        );
    }
  }
}

/// Error configuration model
class _ErrorConfig {
  final IconData icon;
  final Color iconColor;
  final String title;
  final String message;

  _ErrorConfig({
    required this.icon,
    required this.iconColor,
    required this.title,
    required this.message,
  });
}

/// Empty state widget (similar to error but for empty results)
class EmptyStateWidget extends StatelessWidget {
  final IconData icon;
  final String title;
  final String message;
  final VoidCallback? onAction;
  final String? actionLabel;

  const EmptyStateWidget({
    super.key,
    required this.icon,
    required this.title,
    required this.message,
    this.onAction,
    this.actionLabel,
  });

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // Empty Icon
            Icon(
              icon,
              size: 100,
              color: Theme.of(
                context,
              ).colorScheme.onSurfaceVariant.withOpacity(0.5),
            ),

            const SizedBox(height: 24),

            // Title
            Text(
              title,
              style: AppTypography.headlineSmall.copyWith(
                fontWeight: FontWeight.w600,
                color: Theme.of(context).colorScheme.onSurface,
              ),
              textAlign: TextAlign.center,
            ),

            const SizedBox(height: 12),

            // Message
            Text(
              message,
              style: AppTypography.bodyMedium.copyWith(
                color: Theme.of(context).colorScheme.onSurfaceVariant,
              ),
              textAlign: TextAlign.center,
            ),

            if (onAction != null && actionLabel != null) ...[
              const SizedBox(height: 32),

              // Action Button
              ElevatedButton(
                onPressed: onAction,
                style: ElevatedButton.styleFrom(
                  backgroundColor: Theme.of(context).colorScheme.primary,
                  foregroundColor: Theme.of(context).colorScheme.onPrimary,
                  padding: const EdgeInsets.symmetric(
                    horizontal: 32,
                    vertical: 16,
                  ),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: Text(actionLabel!),
              ),
            ],
          ],
        ),
      ),
    );
  }
}
