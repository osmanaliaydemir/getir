import 'package:dio/dio.dart';

/// Base exception class for all app exceptions
abstract class AppException implements Exception {
  final String message;
  final String? code;
  final dynamic originalError;

  const AppException({required this.message, this.code, this.originalError});

  @override
  String toString() => 'AppException: $message';
}

/// Network related exceptions
class NetworkException extends AppException {
  const NetworkException({
    required super.message,
    super.code,
    super.originalError,
  });
}

class NoInternetException extends NetworkException {
  const NoInternetException({
    super.message = 'No internet connection',
    super.code,
    super.originalError,
  });
}

class TimeoutException extends NetworkException {
  const TimeoutException({
    super.message = 'Request timeout',
    super.code,
    super.originalError,
  });
}

/// API related exceptions
class ApiException extends AppException {
  final int? statusCode;
  final Map<String, dynamic>? responseData;

  const ApiException({
    required super.message,
    super.code,
    this.statusCode,
    this.responseData,
    super.originalError,
  });
}

class UnauthorizedException extends ApiException {
  const UnauthorizedException({
    super.message = 'Unauthorized access',
    super.code,
    super.statusCode,
    super.responseData,
    super.originalError,
  });
}

class ForbiddenException extends ApiException {
  const ForbiddenException({
    super.message = 'Access forbidden',
    super.code,
    super.statusCode,
    super.responseData,
    super.originalError,
  });
}

class NotFoundException extends ApiException {
  const NotFoundException({
    super.message = 'Resource not found',
    super.code,
    super.statusCode,
    super.responseData,
    super.originalError,
  });
}

class ServerException extends ApiException {
  const ServerException({
    super.message = 'Server error',
    super.code,
    super.statusCode,
    super.responseData,
    super.originalError,
  });
}

class ValidationException extends ApiException {
  final Map<String, List<String>>? validationErrors;

  const ValidationException({
    required super.message,
    super.code,
    super.statusCode,
    this.validationErrors,
    super.responseData,
    super.originalError,
  });
}

/// Local storage exceptions
class StorageException extends AppException {
  const StorageException({
    required super.message,
    super.code,
    super.originalError,
  });
}

/// Cache exceptions
class CacheException extends AppException {
  const CacheException({
    required super.message,
    super.code,
    super.originalError,
  });
}

/// Business logic exceptions
class BusinessException extends AppException {
  const BusinessException({
    required super.message,
    super.code,
    super.originalError,
  });
}

class InsufficientFundsException extends BusinessException {
  const InsufficientFundsException({
    super.message = 'Insufficient funds',
    super.code,
    super.originalError,
  });
}

class ProductUnavailableException extends BusinessException {
  const ProductUnavailableException({
    super.message = 'Product is unavailable',
    super.code,
    super.originalError,
  });
}

class OrderLimitExceededException extends BusinessException {
  const OrderLimitExceededException({
    super.message = 'Order limit exceeded',
    super.code,
    super.originalError,
  });
}

/// Exception factory for creating exceptions from Dio errors
class ExceptionFactory {
  static AppException fromDioError(DioException error) {
    switch (error.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
        return TimeoutException(
          message: 'Request timeout. Please try again.',
          originalError: error,
        );

      case DioExceptionType.connectionError:
        return NoInternetException(
          message: 'No internet connection. Please check your network.',
          originalError: error,
        );

      case DioExceptionType.badResponse:
        return _handleBadResponse(error);

      case DioExceptionType.cancel:
        return NetworkException(
          message: 'Request was cancelled',
          originalError: error,
        );

      case DioExceptionType.unknown:
      default:
        return NetworkException(
          message: 'An unexpected error occurred',
          originalError: error,
        );
    }
  }

  static AppException _handleBadResponse(DioException error) {
    final statusCode = error.response?.statusCode;
    final responseData = error.response?.data;

    // Handle case where responseData is not a Map
    if (responseData != null && responseData is! Map<String, dynamic>) {
      return NotFoundException(
        message: responseData.toString(),
        statusCode: statusCode,
        responseData: responseData,
        originalError: error,
      );
    }

    switch (statusCode) {
      case 400:
        return ValidationException(
          message: _extractErrorMessage(responseData) ?? 'Invalid request',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      case 401:
        return UnauthorizedException(
          message: _extractErrorMessage(responseData) ?? 'Unauthorized access',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      case 403:
        return ForbiddenException(
          message: _extractErrorMessage(responseData) ?? 'Access forbidden',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      case 404:
        return NotFoundException(
          message: _extractErrorMessage(responseData) ?? 'Resource not found',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      case 422:
        return ValidationException(
          message: _extractErrorMessage(responseData) ?? 'Validation failed',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      case 500:
      case 502:
      case 503:
      case 504:
        return ServerException(
          message: _extractErrorMessage(responseData) ?? 'Server error',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );

      default:
        return ApiException(
          message: _extractErrorMessage(responseData) ?? 'API error',
          statusCode: statusCode,
          responseData: responseData,
          originalError: error,
        );
    }
  }

  static String? _extractErrorMessage(dynamic responseData) {
    if (responseData is Map<String, dynamic>) {
      return responseData['message'] as String? ??
          responseData['error'] as String? ??
          responseData['detail'] as String?;
    } else if (responseData is String) {
      return responseData;
    }
    return null;
  }
}
