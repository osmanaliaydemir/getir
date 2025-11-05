import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'local_storage_service.dart';
import '../config/environment_config.dart';
import '../interceptors/ssl_pinning_interceptor.dart';
import '../interceptors/cache_interceptor.dart';
import 'secure_encryption_service.dart';
import '../interceptors/token_refresh_interceptor.dart';
import 'dart:math';

class ApiClient {
  static final ApiClient _instance = ApiClient._internal();
  factory ApiClient() => _instance;
  ApiClient._internal() {
    // Use new EnvironmentConfig for base URL and timeout
    dio = Dio(
      BaseOptions(
        baseUrl: EnvironmentConfig.apiBaseUrl,
        connectTimeout: Duration(milliseconds: EnvironmentConfig.apiTimeout),
        receiveTimeout: Duration(milliseconds: EnvironmentConfig.apiTimeout),
        headers: {
          'Accept': 'application/json',
          'X-API-Key': EnvironmentConfig.apiKey,
        },
      ),
    );

    // Add interceptors in order:
    // 1. Cache (if initialized)
    // 2. SSL Pinning (production only)
    // 3. Auth
    // 4. Logging
    // 5. Retry
    // 6. Response Adapter
    dio.interceptors.addAll([
      if (ApiCacheInterceptor.isInitialized) ApiCacheInterceptor.interceptor,
      SslPinningInterceptor(),
      _AuthInterceptor(_storage),
      _IdempotencyInterceptor(),
      // Token refresh for 401s (must be after auth header injector)
      TokenRefreshInterceptor(dio, _encryptionService),
      _LoggingInterceptor(),
      _RetryInterceptor(dio: dio),
      _ResponseAdapterInterceptor(),
    ]);

    if (EnvironmentConfig.debugMode) {
      debugPrint('🚀 ApiClient initialized');
      debugPrint('   Base URL: ${EnvironmentConfig.apiBaseUrl}');
      debugPrint('   Timeout: ${EnvironmentConfig.apiTimeout}ms');
      debugPrint('   SSL Pinning: ${EnvironmentConfig.enableSslPinning}');
    }
  }

  late final Dio dio;
  final LocalStorageService _storage = LocalStorageService();
  final SecureEncryptionService _encryptionService = SecureEncryptionService();
}

class _IdempotencyInterceptor extends Interceptor {
  static const _header = 'Idempotency-Key';
  final Random _random = Random();

  bool _isMutating(String method) {
    final m = method.toUpperCase();
    return m == 'POST' || m == 'PUT' || m == 'PATCH' || m == 'DELETE';
  }

  String _generateKey() {
    // Lightweight UUIDv4-like generator without external deps
    String hex(int len) =>
        List.generate(len, (_) => _random.nextInt(16).toRadixString(16)).join();
    return '${hex(8)}-${hex(4)}-${hex(4)}-${hex(4)}-${hex(12)}';
  }

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (_isMutating(options.method) && options.headers[_header] == null) {
      final key = _generateKey();
      options.headers[_header] = key;
      // store for retries
      options.extra[_header] = key;
    }
    super.onRequest(options, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    // ensure retries reuse the same key
    final key = err.requestOptions.extra[_header];
    if (key != null) {
      err.requestOptions.headers[_header] = key;
    }
    super.onError(err, handler);
  }
}

class _AuthInterceptor extends Interceptor {
  final LocalStorageService storage;
  final SecureEncryptionService _encryptionService = SecureEncryptionService();

  _AuthInterceptor(this.storage);

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    // Try to get token from secure storage first
    final secureToken = await _encryptionService.getAccessToken();
    final token = secureToken ?? storage.getUserData('auth_token');

    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    super.onRequest(options, handler);
  }
}

class _LoggingInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (kDebugMode) {
      final maskedHeaders = Map.of(options.headers);
      if (maskedHeaders.containsKey('Authorization')) {
        maskedHeaders['Authorization'] = 'Bearer ****';
      }
      if (maskedHeaders.containsKey('X-API-Key')) {
        maskedHeaders['X-API-Key'] = '****';
      }
      if (maskedHeaders.containsKey('Idempotency-Key')) {
        maskedHeaders['Idempotency-Key'] = '****';
      }
      debugPrint('➡️ ${options.method} ${options.uri}');
      debugPrint('Headers: $maskedHeaders');
      if (options.data != null) {
        // basic PII masking for common keys
        dynamic body = options.data;
        if (body is Map) {
          final b = Map.of(body);
          for (final key in [
            'password',
            'token',
            'accessToken',
            'refreshToken',
          ]) {
            if (b.containsKey(key)) b[key] = '****';
          }
          debugPrint('Body: $b');
        } else {
          debugPrint('Body: ${options.data}');
        }
      }
    }
    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('✅ ${response.statusCode} ${response.requestOptions.uri}');
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('❌ ${err.type} ${err.requestOptions.uri}');
    }
    super.onError(err, handler);
  }
}

class _RetryInterceptor extends Interceptor {
  _RetryInterceptor({required this.dio});
  final Dio dio;
  final int maxRetries = 4;
  final Duration baseDelay = const Duration(milliseconds: 300);

  @override
  Future onError(DioException err, ErrorInterceptorHandler handler) async {
    int attempt = (err.requestOptions.extra['retry_attempt'] as int?) ?? 0;

    // Identify retryable conditions
    final status = err.response?.statusCode ?? 0;
    final isTimeout =
        err.type == DioExceptionType.receiveTimeout ||
        err.type == DioExceptionType.sendTimeout;
    final isConn = err.type == DioExceptionType.connectionError;
    final is5xx = status >= 500 && status < 600;
    final is429 = status == 429;

    final shouldRetry =
        (isTimeout || isConn || is5xx || is429) && attempt < maxRetries;

    if (!shouldRetry) return handler.next(err);

    // Compute backoff
    Duration delay;
    if (is429) {
      final ra = err.response?.headers.value('Retry-After');
      if (ra != null) {
        final seconds = int.tryParse(ra);
        if (seconds != null) {
          delay = Duration(seconds: seconds);
        } else {
          delay = baseDelay * (1 << attempt);
        }
      } else {
        delay = baseDelay * (1 << attempt);
      }
    } else {
      delay = baseDelay * (1 << attempt);
    }
    // add jitter (+/- 20%)
    final jitterMs = (delay.inMilliseconds * 0.2).round();
    final rand = Random();
    final jitter = rand.nextInt(jitterMs * 2) - jitterMs;
    delay = Duration(
      milliseconds: (delay.inMilliseconds + jitter).clamp(100, 30 * 1000),
    );

    await Future.delayed(delay);

    final req = err.requestOptions;
    req.extra['retry_attempt'] = attempt + 1;

    try {
      final response = await dio.fetch(req);
      return handler.resolve(response);
    } catch (_) {
      return handler.next(err);
    }
  }
}

/// Normalizes API responses to a consistent shape: { data, statusCode }
class _ResponseAdapterInterceptor extends Interceptor {
  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    // If response already has a data field with nested data, unwrap gracefully
    final dynamic body = response.data;
    if (body is Map<String, dynamic>) {
      final normalized = <String, dynamic>{}
        ..addAll(body)
        ..putIfAbsent('statusCode', () => response.statusCode);
      response.data = normalized['data'] ?? normalized;
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    // Convert to AppException using ExceptionFactory
    handler.next(err);
  }
}
