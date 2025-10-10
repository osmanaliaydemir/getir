import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../cubits/network/network_cubit.dart';
import '../theme/app_colors.dart';
import '../theme/app_typography.dart';

/// Offline Indicator Banner
///
/// Shows a banner at the top of the screen when the device is offline.
/// Features:
/// - Automatic show/hide based on network status
/// - Retry connection button
/// - Smooth animations
/// - Material Design 3 style
class OfflineIndicatorBanner extends StatelessWidget {
  const OfflineIndicatorBanner({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<NetworkCubit, NetworkState>(
      builder: (context, state) {
        if (state.isOnline) {
          return const SizedBox.shrink();
        }

        return AnimatedSlide(
          duration: const Duration(milliseconds: 300),
          offset: state.isOffline ? Offset.zero : const Offset(0, -1),
          curve: Curves.easeInOut,
          child: Container(
            width: double.infinity,
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
            decoration: BoxDecoration(
              color: AppColors.error,
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.1),
                  offset: const Offset(0, 2),
                  blurRadius: 4,
                ),
              ],
            ),
            child: SafeArea(
              bottom: false,
              child: Row(
                children: [
                  const Icon(Icons.wifi_off, color: AppColors.white, size: 20),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          'No Internet Connection',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.white,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        Text(
                          'Some features may not be available',
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.white.withOpacity(0.9),
                          ),
                        ),
                      ],
                    ),
                  ),
                  if (state.isRetrying)
                    const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        valueColor: AlwaysStoppedAnimation<Color>(
                          AppColors.white,
                        ),
                      ),
                    )
                  else
                    TextButton(
                      onPressed: () =>
                          context.read<NetworkCubit>().retryConnection(),
                      style: TextButton.styleFrom(
                        foregroundColor: AppColors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 12,
                          vertical: 6,
                        ),
                        backgroundColor: Colors.white.withOpacity(0.2),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: const Text('Retry'),
                    ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}

/// Connection Status Indicator
///
/// Shows a small connection status indicator (e.g. in app bar)
class ConnectionStatusIndicator extends StatelessWidget {
  const ConnectionStatusIndicator({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<NetworkCubit, NetworkState>(
      builder: (context, state) {
        if (state.isOnline) {
          return const SizedBox.shrink();
        }

        return Container(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          decoration: BoxDecoration(
            color: AppColors.error,
            borderRadius: BorderRadius.circular(12),
          ),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.wifi_off, color: AppColors.white, size: 14),
              const SizedBox(width: 4),
              Text(
                'Offline',
                style: AppTypography.bodySmall.copyWith(
                  color: AppColors.white,
                  fontSize: 11,
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}

/// Offline Mode Snackbar
///
/// Shows a snackbar when going offline/online
class OfflineModeSnackbar {
  static void show(BuildContext context, bool isOffline) {
    ScaffoldMessenger.of(context).clearSnackBars();
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            Icon(
              isOffline ? Icons.wifi_off : Icons.wifi,
              color: AppColors.white,
              size: 20,
            ),
            const SizedBox(width: 12),
            Text(
              isOffline
                  ? 'You are offline. Changes will be synced when you reconnect.'
                  : 'Back online! Syncing your data...',
              style: AppTypography.bodyMedium.copyWith(color: AppColors.white),
            ),
          ],
        ),
        backgroundColor: isOffline ? AppColors.error : AppColors.success,
        behavior: SnackBarBehavior.floating,
        duration: Duration(seconds: isOffline ? 4 : 2),
        action: isOffline
            ? SnackBarAction(
                label: 'Retry',
                textColor: AppColors.white,
                onPressed: () {
                  context.read<NetworkCubit>().retryConnection();
                },
              )
            : null,
      ),
    );
  }
}
