import 'dart:async';
import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'local_storage_service.dart';
import '../config/environment_config.dart';
import '../interceptors/ssl_pinning_interceptor.dart';
import '../interceptors/cache_interceptor.dart';
import 'secure_encryption_service.dart';

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
      debugPrint('➡️ ${options.method} ${options.uri}');
      debugPrint('Headers: ${options.headers}');
      if (options.data != null) debugPrint('Body: ${options.data}');
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
  final int maxRetries = 2;
  final Duration retryDelay = const Duration(milliseconds: 400);

  @override
  Future onError(DioException err, ErrorInterceptorHandler handler) async {
    int attempt = (err.requestOptions.extra['retry_attempt'] as int?) ?? 0;

    final shouldRetry =
        err.type == DioExceptionType.connectionError ||
        err.type == DioExceptionType.receiveTimeout ||
        err.type == DioExceptionType.sendTimeout;

    if (shouldRetry && attempt < maxRetries) {
      await Future.delayed(retryDelay * (attempt + 1));
      final req = err.requestOptions;
      req.extra['retry_attempt'] = attempt + 1;
      try {
        final response = await dio.fetch(req);
        return handler.resolve(response);
      } catch (e) {
        return handler.next(err);
      }
    }

    return handler.next(err);
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
