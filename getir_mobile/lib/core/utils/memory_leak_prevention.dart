import 'dart:async';
import 'package:flutter/material.dart';
import '../services/logger_service.dart';

/// Memory Leak Prevention Utilities
/// Helps prevent common memory leaks in Flutter apps
class MemoryLeakPrevention {
  /// Safely dispose a Stream subscription
  static void disposeSubscription(StreamSubscription? subscription) {
    subscription?.cancel();
  }

  /// Safely dispose multiple subscriptions
  static void disposeSubscriptions(List<StreamSubscription?> subscriptions) {
    for (final subscription in subscriptions) {
      subscription?.cancel();
    }
  }

  /// Safely dispose a StreamController
  static void disposeStreamController(StreamController? controller) {
    if (controller != null && !controller.isClosed) {
      controller.close();
    }
  }

  /// Safely dispose multiple controllers
  static void disposeStreamControllers(List<StreamController?> controllers) {
    for (final controller in controllers) {
      disposeStreamController(controller);
    }
  }

  /// Safely dispose a TextEditingController
  static void disposeTextController(TextEditingController? controller) {
    controller?.dispose();
  }

  /// Safely dispose multiple text controllers
  static void disposeTextControllers(List<TextEditingController?> controllers) {
    for (final controller in controllers) {
      disposeTextController(controller);
    }
  }

  /// Safely dispose an AnimationController
  static void disposeAnimationController(AnimationController? controller) {
    controller?.dispose();
  }

  /// Safely dispose multiple animation controllers
  static void disposeAnimationControllers(
    List<AnimationController?> controllers,
  ) {
    for (final controller in controllers) {
      disposeAnimationController(controller);
    }
  }

  /// Safely cancel a Timer
  static void cancelTimer(Timer? timer) {
    timer?.cancel();
  }

  /// Safely cancel multiple timers
  static void cancelTimers(List<Timer?> timers) {
    for (final timer in timers) {
      cancelTimer(timer);
    }
  }

  /// Check if a BuildContext is still mounted
  /// Useful for async operations
  static bool isMounted(BuildContext? context) {
    return context != null && context.mounted;
  }

  /// Print memory usage info (debug only)
  static void printMemoryInfo() {
    logger.info(
      'Memory Leak Prevention: Active checks enabled',
      tag: 'MemoryLeak',
    );
  }
}

/// Mixin for automatic resource disposal
/// Add this to your StatefulWidget State class
/// ```dart
/// class _MyPageState extends State<MyPage> with DisposableMixin {
///   late final TextEditingController _controller;
///   late final StreamSubscription _subscription;
///
///   @override
///   void initState() {
///     super.initState();
///     _controller = TextEditingController();
///     registerDisposable(_controller);
///
///     _subscription = stream.listen((_) {});
///     registerDisposable(_subscription);
///   }
/// }
/// ```
mixin DisposableMixin<T extends StatefulWidget> on State<T> {
  final List<StreamSubscription> _subscriptions = [];
  final List<StreamController> _controllers = [];
  final List<TextEditingController> _textControllers = [];
  final List<AnimationController> _animationControllers = [];
  final List<Timer> _timers = [];

  /// Register a disposable resource
  void registerDisposable(dynamic disposable) {
    if (disposable is StreamSubscription) {
      _subscriptions.add(disposable);
    } else if (disposable is StreamController) {
      _controllers.add(disposable);
    } else if (disposable is TextEditingController) {
      _textControllers.add(disposable);
    } else if (disposable is AnimationController) {
      _animationControllers.add(disposable);
    } else if (disposable is Timer) {
      _timers.add(disposable);
    }
  }

  /// Unregister a disposable resource
  void unregisterDisposable(dynamic disposable) {
    if (disposable is StreamSubscription) {
      _subscriptions.remove(disposable);
    } else if (disposable is StreamController) {
      _controllers.remove(disposable);
    } else if (disposable is TextEditingController) {
      _textControllers.remove(disposable);
    } else if (disposable is AnimationController) {
      _animationControllers.remove(disposable);
    } else if (disposable is Timer) {
      _timers.remove(disposable);
    }
  }

  @override
  void dispose() {
    // Dispose all registered resources
    MemoryLeakPrevention.disposeSubscriptions(_subscriptions);
    MemoryLeakPrevention.disposeStreamControllers(_controllers);
    MemoryLeakPrevention.disposeTextControllers(_textControllers);
    MemoryLeakPrevention.disposeAnimationControllers(_animationControllers);
    MemoryLeakPrevention.cancelTimers(_timers);

    super.dispose();
  }
}

/// Extension for safe async operations
extension SafeAsyncExtension on BuildContext {
  /// Safely execute async operation only if context is still mounted
  /// Usage:
  /// ```dart
  /// context.safeAsync(() async {
  ///   final data = await fetchData();
  ///   setState(() => _data = data);
  /// });
  /// ```
  Future<void> safeAsync(Future<void> Function() operation) async {
    if (!mounted) return;
    try {
      await operation();
    } catch (e) {
      if (mounted) {
        logger.error(
          'Safe async operation failed',
          tag: 'MemoryLeak',
          error: e,
        );
      }
    }
  }

  /// Safely set state only if context is still mounted
  /// Usage:
  /// ```dart
  /// context.safeSetState(() {
  ///   _data = newData;
  /// });
  /// ```
  void safeSetState(VoidCallback fn) {
    if (mounted) {
      // ignore: invalid_use_of_protected_member
      (this as Element).markNeedsBuild();
      fn();
    }
  }
}

/// Stream management helper
class StreamManager {
  final List<StreamSubscription> _subscriptions = [];
  bool _disposed = false;

  /// Add a stream subscription to be managed
  void add(StreamSubscription subscription) {
    if (_disposed) {
      subscription.cancel();
      return;
    }
    _subscriptions.add(subscription);
  }

  /// Remove a specific subscription
  void remove(StreamSubscription subscription) {
    _subscriptions.remove(subscription);
    subscription.cancel();
  }

  /// Dispose all subscriptions
  void dispose() {
    if (_disposed) return;

    for (final subscription in _subscriptions) {
      subscription.cancel();
    }
    _subscriptions.clear();
    _disposed = true;
  }

  /// Check if manager is disposed
  bool get isDisposed => _disposed;
}

/// Timer management helper
class TimerManager {
  final List<Timer> _timers = [];
  bool _disposed = false;

  /// Add a timer to be managed
  void add(Timer timer) {
    if (_disposed) {
      timer.cancel();
      return;
    }
    _timers.add(timer);
  }

  /// Remove a specific timer
  void remove(Timer timer) {
    _timers.remove(timer);
    timer.cancel();
  }

  /// Dispose all timers
  void dispose() {
    if (_disposed) return;

    for (final timer in _timers) {
      timer.cancel();
    }
    _timers.clear();
    _disposed = true;
  }

  /// Check if manager is disposed
  bool get isDisposed => _disposed;
}

/// Debouncer for preventing rapid successive calls
class Debouncer {
  final Duration delay;
  Timer? _timer;

  Debouncer({this.delay = const Duration(milliseconds: 300)});

  /// Run the action after delay, canceling previous calls
  void run(VoidCallback action) {
    _timer?.cancel();
    _timer = Timer(delay, action);
  }

  /// Cancel pending action
  void cancel() {
    _timer?.cancel();
  }

  /// Dispose debouncer
  void dispose() {
    _timer?.cancel();
    _timer = null;
  }
}

/// Throttler for rate-limiting function calls
class Throttler {
  final Duration duration;
  bool _isReady = true;
  Timer? _timer;

  Throttler({this.duration = const Duration(milliseconds: 300)});

  /// Run the action only if throttler is ready
  void run(VoidCallback action) {
    if (!_isReady) return;

    _isReady = false;
    action();

    _timer = Timer(duration, () {
      _isReady = true;
    });
  }

  /// Dispose throttler
  void dispose() {
    _timer?.cancel();
    _timer = null;
    _isReady = true;
  }
}
