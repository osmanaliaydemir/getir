import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../cubits/network/network_cubit.dart';
import '../widgets/offline_indicator_banner.dart';
import '../services/logger_service.dart';

/// Offline Mode Helper
///
/// Utility functions and widgets for handling offline mode gracefully
class OfflineModeHelper {
  /// Show offline warning dialog
  static Future<bool?> showOfflineWarningDialog(
    BuildContext context, {
    required String title,
    required String message,
    String confirmText = 'Continue Offline',
    String cancelText = 'Cancel',
  }) async {
    return showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        icon: const Icon(Icons.wifi_off, size: 48),
        title: Text(title),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: Text(cancelText),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: Text(confirmText),
          ),
        ],
      ),
    );
  }

  /// Check if online and show warning if not
  static Future<bool> checkOnlineOrWarn(
    BuildContext context, {
    required String action,
    bool allowOffline = false,
  }) async {
    final networkState = context.read<NetworkCubit>().state;

    if (networkState.isOnline) {
      return true;
    }

    if (allowOffline) {
      final result = await showOfflineWarningDialog(
        context,
        title: 'You are offline',
        message: 'This action will be performed when you reconnect. Continue?',
      );
      return result ?? false;
    } else {
      OfflineModeSnackbar.show(context, true);
      return false;
    }
  }

  /// Execute action with offline fallback
  static Future<T?> executeWithOfflineFallback<T>({
    required Future<T> Function() onlineAction,
    required Future<T> Function() offlineAction,
    required bool isOnline,
  }) async {
    try {
      if (isOnline) {
        return await onlineAction();
      } else {
        return await offlineAction();
      }
    } catch (e) {
      return null;
    }
  }
}

/// Offline Indicator Wrapper
///
/// Wraps a screen with automatic offline indicator
class OfflineIndicatorWrapper extends StatelessWidget {
  final Widget child;
  final bool showBanner;

  const OfflineIndicatorWrapper({
    super.key,
    required this.child,
    this.showBanner = true,
  });

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        child,
        if (showBanner)
          const Positioned(
            top: 0,
            left: 0,
            right: 0,
            child: OfflineIndicatorBanner(),
          ),
      ],
    );
  }
}

/// Offline Mode Mixin
///
/// Mixin for pages that need offline mode handling
mixin OfflineModeMixin<T extends StatefulWidget> on State<T> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _setupOfflineListener();
    });
  }

  void _setupOfflineListener() {
    // Subscribe to network state changes via BlocListener
    // This should be handled in the parent widget using BlocListener<NetworkCubit>
    // Example usage in parent widget:
    // BlocListener<NetworkCubit, NetworkState>(
    //   listener: (context, state) {
    //     if (state.isOffline && !_wasOffline) {
    //       onOffline();
    //       _wasOffline = true;
    //     } else if (state.isOnline && _wasOffline) {
    //       onOnline();
    //       _wasOffline = false;
    //     }
    //   },
    //   child: YourWidget(),
    // )
  }

  /// Override to handle offline event
  void onOffline() {
    logger.debug('Page went offline', tag: 'OfflineMode');
  }

  /// Override to handle online event
  void onOnline() {
    logger.debug('Page went online', tag: 'OfflineMode');
  }

  @override
  void dispose() {
    // No need to remove listener with BLoC pattern
    super.dispose();
  }
}
