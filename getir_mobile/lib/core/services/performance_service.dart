import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'dart:async';

/// Performance monitoring service for tracking app performance metrics
class PerformanceService {
  static final PerformanceService _instance = PerformanceService._internal();
  factory PerformanceService() => _instance;
  PerformanceService._internal();

  DateTime? _startupStart;
  final Map<String, DateTime> _timers = {};
  final Map<String, dynamic> _metrics = {};

  void markStartupBegin() {
    _startupStart = DateTime.now();
  }

  void markStartupEnd() {
    if (_startupStart == null) return;
    final elapsed = DateTime.now().difference(_startupStart!);
    _log('app_startup_ms', elapsed.inMilliseconds);
    _startupStart = null;
  }

  void logEvent(String name, {Map<String, Object?> parameters = const {}}) {
    _log(name, parameters);
  }

  void _log(String name, Object value) {
    if (kDebugMode) {
      // Replace with real analytics SDK when ready
      print('[Analytics] $name: $value');
    }
  }

  // Simple timing helpers used by PerformanceMonitor
  void startTiming(String name) {
    _timers[name] = DateTime.now();
  }

  void endTiming(String name) {
    final start = _timers.remove(name);
    if (start == null) return;
    final elapsedMs = DateTime.now().difference(start).inMilliseconds;
    recordMetric('${name}_ms', elapsedMs);
  }

  // Metrics API used by overlays/trackers
  void recordMetric(String key, Object value) {
    _metrics[key] = value;
    _log('metric_$key', value);
  }

  Map<String, dynamic> getAllMetrics() {
    return Map<String, dynamic>.from(_metrics);
  }
}

/// Performance monitoring widget that tracks build times
class PerformanceMonitor extends StatefulWidget {
  final Widget child;
  final String name;
  final bool enabled;

  const PerformanceMonitor({
    super.key,
    required this.child,
    required this.name,
    this.enabled = kDebugMode,
  });

  @override
  State<PerformanceMonitor> createState() => _PerformanceMonitorState();
}

class _PerformanceMonitorState extends State<PerformanceMonitor> {
  @override
  void initState() {
    super.initState();
    if (widget.enabled) {
      PerformanceService().startTiming('widget_${widget.name}');
    }
  }

  @override
  void dispose() {
    if (widget.enabled) {
      PerformanceService().endTiming('widget_${widget.name}');
    }
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return widget.child;
  }
}

/// Performance overlay for debugging
class PerformanceOverlay extends StatefulWidget {
  final Widget child;
  final bool enabled;

  const PerformanceOverlay({
    super.key,
    required this.child,
    this.enabled = kDebugMode,
  });

  @override
  State<PerformanceOverlay> createState() => _PerformanceOverlayState();
}

class _PerformanceOverlayState extends State<PerformanceOverlay> {
  Timer? _timer;
  Map<String, dynamic> _currentMetrics = {};

  @override
  void initState() {
    super.initState();
    if (widget.enabled) {
      _startMonitoring();
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _startMonitoring() {
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (mounted) {
        setState(() {
          _currentMetrics = PerformanceService().getAllMetrics();
        });
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    if (!widget.enabled) {
      return widget.child;
    }

    return Stack(
      children: [
        widget.child,
        Positioned(
          top: 50,
          right: 10,
          child: Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: Colors.black.withOpacity(0.7),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(
                  'Performance',
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                ),
                const SizedBox(height: 4),
                ..._currentMetrics.entries.take(5).map((entry) {
                  return Text(
                    '${entry.key}: ${entry.value}',
                    style: const TextStyle(color: Colors.white, fontSize: 10),
                  );
                }),
              ],
            ),
          ),
        ),
      ],
    );
  }
}

/// App startup performance tracker
class AppStartupTracker {
  static final AppStartupTracker _instance = AppStartupTracker._internal();
  factory AppStartupTracker() => _instance;
  AppStartupTracker._internal();

  DateTime? _appStartTime;
  DateTime? _firstFrameTime;
  final Map<String, DateTime> _milestones = {};

  /// Mark app start
  void markAppStart() {
    _appStartTime = DateTime.now();
    _milestones['app_start'] = _appStartTime!;
  }

  /// Mark first frame rendered
  void markFirstFrame() {
    _firstFrameTime = DateTime.now();
    _milestones['first_frame'] = _firstFrameTime!;

    if (_appStartTime != null) {
      final startupTime = _firstFrameTime!.difference(_appStartTime!);
      PerformanceService().recordMetric(
        'app_startup_time_ms',
        startupTime.inMilliseconds,
      );

      if (kDebugMode) {
        print('App startup time: ${startupTime.inMilliseconds}ms');
      }
    }
  }

  /// Mark a milestone
  void markMilestone(String name) {
    _milestones[name] = DateTime.now();
  }

  /// Get startup time
  Duration? getStartupTime() {
    if (_appStartTime == null || _firstFrameTime == null) return null;
    return _firstFrameTime!.difference(_appStartTime!);
  }

  /// Get all milestones
  Map<String, DateTime> getMilestones() {
    return Map.from(_milestones);
  }
}
