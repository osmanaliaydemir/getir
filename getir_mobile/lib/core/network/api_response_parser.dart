import '../errors/app_exceptions.dart';

/// Generic API response parser
/// Handles both new (data) and legacy (value) response formats
class ApiResponseParser {
  /// Parse API response with automatic format detection
  static T parse<T>({
    required dynamic responseData,
    required T Function(Map<String, dynamic>) fromJson,
    String? endpointName,
  }) {
    // Handle null response
    if (responseData == null) {
      throw ApiException(
        message:
            'Response is null${endpointName != null ? " ($endpointName)" : ""}',
      );
    }

    // Handle direct error string (404, etc.)
    if (responseData is String) {
      throw ApiException(
        message:
            '$responseData${endpointName != null ? " ($endpointName)" : ""}',
      );
    }

    // Handle Map response
    if (responseData is Map<String, dynamic>) {
      // Check if it's wrapped in ApiResponse format
      if (responseData.containsKey('isSuccess')) {
        final isSuccess = responseData['isSuccess'] as bool?;

        if (isSuccess == true) {
          // Success - extract data from 'data' or 'value' field
          final dataField = responseData['data'] ?? responseData['value'];

          if (dataField == null) {
            throw ApiException(message: 'Success response but data is null');
          }

          if (dataField is Map<String, dynamic>) {
            return fromJson(dataField);
          }

          // If data is already the expected type
          if (dataField is T) {
            return dataField;
          }

          throw ApiException(message: 'Invalid data format in response');
        } else {
          // Error response
          final error = responseData['error'] as String? ?? 'Unknown error';
          final errorCode = responseData['errorCode'] as String?;

          throw ApiException(message: error, code: errorCode);
        }
      }

      // Direct data (no wrapper)
      return fromJson(responseData);
    }

    throw ApiException(
      message: 'Unexpected response format: ${responseData.runtimeType}',
    );
  }

  /// Parse list response
  static List<T> parseList<T>({
    required dynamic responseData,
    required T Function(Map<String, dynamic>) fromJson,
    String? endpointName,
  }) {
    if (responseData == null) {
      throw ApiException(
        message:
            'Response is null${endpointName != null ? " ($endpointName)" : ""}',
      );
    }

    if (responseData is String) {
      throw ApiException(
        message:
            '$responseData${endpointName != null ? " ($endpointName)" : ""}',
      );
    }

    if (responseData is Map<String, dynamic>) {
      if (responseData.containsKey('isSuccess')) {
        final isSuccess = responseData['isSuccess'] as bool?;

        if (isSuccess == true) {
          final dataField = responseData['data'] ?? responseData['value'];

          if (dataField is List) {
            return dataField
                .map((item) => fromJson(item as Map<String, dynamic>))
                .toList();
          }

          throw ApiException(
            message: 'Expected list but got ${dataField.runtimeType}',
          );
        } else {
          final error = responseData['error'] as String? ?? 'Unknown error';
          final errorCode = responseData['errorCode'] as String?;

          throw ApiException(message: error, code: errorCode);
        }
      }
    }

    // Direct list (no wrapper)
    if (responseData is List) {
      return responseData
          .map((item) => fromJson(item as Map<String, dynamic>))
          .toList();
    }

    throw ApiException(message: 'Unexpected response format for list');
  }
}
