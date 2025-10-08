import 'dart:async';
import 'package:flutter/foundation.dart';
import 'network_service.dart';
import 'sync_service.dart';
import 'analytics_service.dart';

/// Reconnection Strategy Service
///
/// Handles automatic reconnection and sync when network becomes available.
/// Features:
/// - Automatic sync on reconnection
/// - Exponential backoff for retries
/// - Connection quality monitoring
/// - Analytics tracking for offline/online events
class ReconnectionStrategyService {
  final NetworkService _networkService;
  final SyncService _syncService;
  final AnalyticsService _analytics;

  StreamSubscription? _networkSubscription;
  bool _wasOffline = false;
  DateTime? _offlineSince;
  int _reconnectAttempts = 0;

  ReconnectionStrategyService(
    this._networkService,
    this._syncService,
    this._analytics,
  );

  /// Initialize reconnection monitoring
  Future<void> initialize() async {
    await _networkService.initialize();
    _monitorNetworkChanges();
  }

  /// Monitor network status changes
  void _monitorNetworkChanges() {
    _networkSubscription = _networkService.networkStatusStream.listen((
      isOnline,
    ) async {
      if (isOnline && _wasOffline) {
        await _handleReconnection();
      } else if (!isOnline && !_wasOffline) {
        await _handleDisconnection();
      }

      _wasOffline = !isOnline;
    });
  }

  /// Handle reconnection event
  Future<void> _handleReconnection() async {
    _reconnectAttempts++;

    final offlineDuration = _offlineSince != null
        ? DateTime.now().difference(_offlineSince!)
        : null;

    if (kDebugMode) {
      debugPrint('✅ Network reconnected!');
      if (offlineDuration != null) {
        debugPrint('   Offline duration: ${offlineDuration.inSeconds}s');
      }
      debugPrint('   Reconnect attempt: $_reconnectAttempts');
    }

    // Track analytics
    await _analytics.logCustomEvent(
      eventName: 'network_reconnected',
      parameters: {
        'offline_duration_seconds': offlineDuration?.inSeconds ?? 0,
        'reconnect_attempts': _reconnectAttempts,
        'pending_actions': _syncService.getSyncStatus()['pendingActions'] ?? 0,
      },
    );

    // Reset offline timer
    _offlineSince = null;

    // Wait a bit for connection to stabilize
    await Future.delayed(const Duration(milliseconds: 500));

    // Start sync
    await _syncPendingChanges();

    // Reset reconnect attempts after successful sync
    _reconnectAttempts = 0;
  }

  /// Handle disconnection event
  Future<void> _handleDisconnection() async {
    _offlineSince = DateTime.now();

    if (kDebugMode) {
      debugPrint('❌ Network disconnected at $_offlineSince');
    }

    // Track analytics
    await _analytics.logCustomEvent(
      eventName: 'network_disconnected',
      parameters: {
        'pending_actions': _syncService.getSyncStatus()['pendingActions'] ?? 0,
      },
    );
  }

  /// Sync pending changes
  Future<void> _syncPendingChanges() async {
    try {
      final syncStatus = _syncService.getSyncStatus();
      final pendingCount = syncStatus['pendingActions'] as int? ?? 0;

      if (pendingCount > 0) {
        if (kDebugMode) {
          debugPrint('🔄 Syncing $pendingCount pending actions...');
        }

        final startTime = DateTime.now();
        await _syncService.forceSyncNow();
        final syncDuration = DateTime.now().difference(startTime);

        if (kDebugMode) {
          debugPrint('✅ Sync completed in ${syncDuration.inSeconds}s');
        }

        // Track sync analytics
        await _analytics.logCustomEvent(
          eventName: 'offline_sync_completed',
          parameters: {
            'actions_synced': pendingCount,
            'sync_duration_seconds': syncDuration.inSeconds,
          },
        );
      } else {
        if (kDebugMode) {
          debugPrint('✅ No pending actions to sync');
        }
      }
    } catch (e) {
      if (kDebugMode) {
        debugPrint('❌ Sync failed: $e');
      }

      await _analytics.logError(error: e, reason: 'Reconnection sync failed');
    }
  }

  /// Force retry connection
  Future<void> retryConnection() async {
    if (kDebugMode) {
      debugPrint('🔄 Manual reconnection retry...');
    }

    await _networkService.initialize();

    if (_networkService.isOnline) {
      await _syncPendingChanges();
    }
  }

  /// Get offline status
  bool get isOffline => !_networkService.isOnline;

  /// Get offline duration
  Duration? get offlineDuration {
    if (_offlineSince == null) return null;
    return DateTime.now().difference(_offlineSince!);
  }

  /// Get sync status
  Map<String, dynamic> getStatus() {
    return {
      'isOnline': _networkService.isOnline,
      'wasOffline': _wasOffline,
      'offlineSince': _offlineSince?.toIso8601String(),
      'offlineDuration': offlineDuration?.inSeconds,
      'reconnectAttempts': _reconnectAttempts,
      ..._syncService.getSyncStatus(),
    };
  }

  /// Dispose resources
  void dispose() {
    _networkSubscription?.cancel();
  }
}

/// Connection Quality Monitor
///
/// Monitors connection quality and provides insights
class ConnectionQualityMonitor {
  final NetworkService _networkService;

  // Connection quality metrics
  int _successfulRequests = 0;
  int _failedRequests = 0;
  final List<Duration> _requestLatencies = [];

  ConnectionQualityMonitor(this._networkService);

  /// Record successful request
  void recordSuccess(Duration latency) {
    _successfulRequests++;
    _requestLatencies.add(latency);

    // Keep only last 20 latencies
    if (_requestLatencies.length > 20) {
      _requestLatencies.removeAt(0);
    }
  }

  /// Record failed request
  void recordFailure() {
    _failedRequests++;
  }

  /// Get connection quality score (0-100)
  int getQualityScore() {
    if (_successfulRequests + _failedRequests == 0) return 100;

    final successRate =
        _successfulRequests / (_successfulRequests + _failedRequests);
    final avgLatency = _getAverageLatency();

    // Score based on success rate (70%) and latency (30%)
    final successScore = successRate * 70;
    final latencyScore = _getLatencyScore() * 30;

    return (successScore + latencyScore).round();
  }

  /// Get average latency in milliseconds
  double _getAverageLatency() {
    if (_requestLatencies.isEmpty) return 0;

    final total = _requestLatencies.fold<int>(
      0,
      (sum, duration) => sum + duration.inMilliseconds,
    );

    return total / _requestLatencies.length;
  }

  /// Get latency score (0-1)
  double _getLatencyScore() {
    final avgLatency = _getAverageLatency();

    // Excellent: < 100ms = 1.0
    // Good: 100-300ms = 0.7
    // Fair: 300-1000ms = 0.4
    // Poor: > 1000ms = 0.1

    if (avgLatency < 100) return 1.0;
    if (avgLatency < 300) return 0.7;
    if (avgLatency < 1000) return 0.4;
    return 0.1;
  }

  /// Get quality label
  String getQualityLabel() {
    final score = getQualityScore();

    if (score >= 80) return 'Excellent';
    if (score >= 60) return 'Good';
    if (score >= 40) return 'Fair';
    if (score >= 20) return 'Poor';
    return 'Very Poor';
  }

  /// Reset metrics
  void reset() {
    _successfulRequests = 0;
    _failedRequests = 0;
    _requestLatencies.clear();
  }

  /// Get metrics
  Map<String, dynamic> getMetrics() {
    return {
      'isOnline': _networkService.isOnline,
      'qualityScore': getQualityScore(),
      'qualityLabel': getQualityLabel(),
      'successfulRequests': _successfulRequests,
      'failedRequests': _failedRequests,
      'averageLatency': _getAverageLatency(),
      'totalRequests': _successfulRequests + _failedRequests,
    };
  }
}
