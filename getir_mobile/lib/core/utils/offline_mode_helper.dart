import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/network_provider.dart';
import '../widgets/offline_indicator_banner.dart';

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
    final networkProvider = Provider.of<NetworkProvider>(
      context,
      listen: false,
    );

    if (networkProvider.isOnline) {
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
  bool _wasOffline = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _setupOfflineListener();
    });
  }

  void _setupOfflineListener() {
    final networkProvider = Provider.of<NetworkProvider>(
      context,
      listen: false,
    );
    networkProvider.addListener(_handleNetworkChange);
  }

  void _handleNetworkChange() {
    final networkProvider = Provider.of<NetworkProvider>(
      context,
      listen: false,
    );

    if (networkProvider.isOffline && !_wasOffline) {
      onOffline();
      _wasOffline = true;
    } else if (networkProvider.isOnline && _wasOffline) {
      onOnline();
      _wasOffline = false;
    }
  }

  /// Override to handle offline event
  void onOffline() {
    debugPrint('📴 Page went offline');
  }

  /// Override to handle online event
  void onOnline() {
    debugPrint('✅ Page went online');
  }

  @override
  void dispose() {
    final networkProvider = Provider.of<NetworkProvider>(
      context,
      listen: false,
    );
    networkProvider.removeListener(_handleNetworkChange);
    super.dispose();
  }
}
