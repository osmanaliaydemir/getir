/// Standardized API response model
/// Matches backend ApiResponse<T> structure
class ApiResponse<T> {
  final bool success;
  final T? data;
  final String? error;
  final String? errorCode;
  final Map<String, dynamic>? metadata;

  ApiResponse({
    required this.success,
    this.data,
    this.error,
    this.errorCode,
    this.metadata,
  });

  factory ApiResponse.fromJson(
    Map<String, dynamic> json,
    T Function(dynamic) fromJsonT,
  ) {
    return ApiResponse<T>(
      success: json['success'] as bool? ?? false,
      data: json['data'] != null ? fromJsonT(json['data']) : null,
      error: json['error'] as String?,
      errorCode: json['errorCode'] as String?,
      metadata: json['metadata'] as Map<String, dynamic>?,
    );
  }

  /// Compatibility with old format (value instead of data)
  factory ApiResponse.fromJsonLegacy(
    Map<String, dynamic> json,
    T Function(dynamic) fromJsonT,
  ) {
    final dataField = json['data'] ?? json['value']; // Support both formats
    return ApiResponse<T>(
      success: json['success'] as bool? ?? false,
      data: dataField != null ? fromJsonT(dataField) : null,
      error: json['error'] as String?,
      errorCode: json['errorCode'] as String?,
      metadata: json['metadata'] as Map<String, dynamic>?,
    );
  }

  Map<String, dynamic> toJson(Object? Function(T) toJsonT) {
    return {
      'success': success,
      'data': data != null ? toJsonT(data as T) : null,
      'error': error,
      'errorCode': errorCode,
      'metadata': metadata,
    };
  }

  bool get isSuccess => success;
  bool get isFailure => !success;
}
