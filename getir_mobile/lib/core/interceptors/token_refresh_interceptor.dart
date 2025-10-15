import 'package:dio/dio.dart';
import '../services/encryption_service.dart';
import '../services/logger_service.dart';

/// Token Refresh Interceptor
/// 
/// Automatically refreshes access token when API returns 401 Unauthorized.
/// Prevents manual token refresh in every BLoC/Service.
/// 
/// **How it works:**
/// 1. API returns 401 Unauthorized
/// 2. Interceptor catches the error
/// 3. Calls /api/v1/auth/refresh with refresh token
/// 4. Saves new tokens
/// 5. Retries original request with new token
/// 6. If refresh fails (401), logs out user
/// 
/// **Benefits:**
/// - Seamless UX (user doesn't see auth errors)
/// - No manual refresh in BLoCs
/// - Single responsibility
/// - Prevents token expiration issues
class TokenRefreshInterceptor extends QueuedInterceptor {
  final Dio _dio;
  final EncryptionService _encryptionService;
  
  bool _isRefreshing = false;
  final List<RequestOptions> _requestsQueue = [];

  TokenRefreshInterceptor(this._dio, this._encryptionService);

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) async {
    // Only handle 401 Unauthorized
    if (err.response?.statusCode != 401) {
      return super.onError(err, handler);
    }

    // Skip refresh endpoint itself (prevent infinite loop)
    if (err.requestOptions.path.contains('/auth/refresh')) {
      logger.warning(
        'Refresh token itself returned 401 - Logging out user',
        tag: 'TokenRefresh',
      );
      await _handleLogout();
      return super.onError(err, handler);
    }

    logger.debug(
      '401 Unauthorized - Attempting token refresh',
      tag: 'TokenRefresh',
    );

    // If already refreshing, queue the request
    if (_isRefreshing) {
      _requestsQueue.add(err.requestOptions);
      return handler.next(err);
    }

    _isRefreshing = true;

    try {
      // Get refresh token
      final refreshToken = await _encryptionService.getRefreshToken();
      
      if (refreshToken == null || refreshToken.isEmpty) {
        logger.warning('No refresh token found - Logging out', tag: 'TokenRefresh');
        await _handleLogout();
        return super.onError(err, handler);
      }

      // Call refresh endpoint
      final refreshResponse = await _dio.post(
        '/api/v1/auth/refresh',
        data: {'refreshToken': refreshToken},
        options: Options(
          headers: {
            'Authorization': null, // Don't send expired token
          },
        ),
      );

      if (refreshResponse.statusCode == 200) {
        // Extract new tokens from response
        final data = refreshResponse.data['data'] ?? refreshResponse.data;
        final newAccessToken = data['accessToken'] as String;
        final newRefreshToken = data['refreshToken'] as String;

        // Save new tokens
        await _encryptionService.saveAccessToken(newAccessToken);
        await _encryptionService.saveRefreshToken(newRefreshToken);

        logger.info('Token refreshed successfully', tag: 'TokenRefresh');

        // Retry original request with new token
        final retryResponse = await _retryRequest(
          err.requestOptions,
          newAccessToken,
        );

        _isRefreshing = false;

        // Retry queued requests
        await _retryQueuedRequests(newAccessToken);

        return handler.resolve(retryResponse);
      } else {
        logger.error(
          'Token refresh failed with status ${refreshResponse.statusCode}',
          tag: 'TokenRefresh',
        );
        await _handleLogout();
        _isRefreshing = false;
        return super.onError(err, handler);
      }
    } catch (e, stackTrace) {
      logger.error(
        'Token refresh exception',
        tag: 'TokenRefresh',
        error: e,
        stackTrace: stackTrace,
      );
      await _handleLogout();
      _isRefreshing = false;
      return super.onError(err, handler);
    }
  }

  /// Retry original request with new token
  Future<Response> _retryRequest(
    RequestOptions requestOptions,
    String newAccessToken,
  ) async {
    final options = Options(
      method: requestOptions.method,
      headers: {
        ...requestOptions.headers,
        'Authorization': 'Bearer $newAccessToken',
      },
    );

    return await _dio.request(
      requestOptions.path,
      data: requestOptions.data,
      queryParameters: requestOptions.queryParameters,
      options: options,
    );
  }

  /// Retry queued requests with new token
  Future<void> _retryQueuedRequests(String newAccessToken) async {
    for (final request in _requestsQueue) {
      try {
        await _retryRequest(request, newAccessToken);
      } catch (e) {
        logger.error(
          'Failed to retry queued request',
          tag: 'TokenRefresh',
          error: e,
        );
      }
    }
    _requestsQueue.clear();
  }

  /// Handle logout (clear tokens and user data)
  Future<void> _handleLogout() async {
    try {
      await _encryptionService.clearAll();
      logger.info('User logged out due to invalid refresh token', tag: 'TokenRefresh');
    } catch (e) {
      logger.error('Failed to clear tokens during logout', tag: 'TokenRefresh', error: e);
    }
  }
}

