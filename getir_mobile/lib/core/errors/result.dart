/// Result pattern implementation
/// Similar to Result<T> in .NET - represents success or failure
///
/// Usage:
/// ```dart
/// Future<Result<User>> login(String email, String password) async {
///   try {
///     final user = await api.login(email, password);
///     return Result.success(user);
///   } on DioException catch (e) {
///     return Result.failure(ExceptionFactory.fromDioError(e));
///   }
/// }
///
/// // Usage in BLoC:
/// final result = await repository.login(email, password);
/// result.when(
///   success: (user) => emit(AuthAuthenticated(user)),
///   failure: (error) => emit(AuthError(error.message)),
/// );
/// ```
abstract class Result<T> {
  const Result();

  /// Create a successful result with data
  factory Result.success(T data) = Success<T>;

  /// Create a failed result with exception
  factory Result.failure(Exception exception) = Failure<T>;

  /// Check if result is successful
  bool get isSuccess => this is Success<T>;

  /// Check if result is failure
  bool get isFailure => this is Failure<T>;

  /// Get data (throws if failure)
  T get data {
    if (this is Success<T>) {
      return (this as Success<T>).data;
    }
    throw StateError('Cannot get data from Failure result');
  }

  /// Get exception (throws if success)
  Exception get exception {
    if (this is Failure<T>) {
      return (this as Failure<T>).exception;
    }
    throw StateError('Cannot get exception from Success result');
  }

  /// Get data or null
  T? get dataOrNull {
    if (this is Success<T>) {
      return (this as Success<T>).data;
    }
    return null;
  }

  /// Get exception or null
  Exception? get exceptionOrNull {
    if (this is Failure<T>) {
      return (this as Failure<T>).exception;
    }
    return null;
  }

  /// Pattern matching
  R when<R>({
    required R Function(T data) success,
    required R Function(Exception exception) failure,
  }) {
    if (this is Success<T>) {
      return success((this as Success<T>).data);
    } else {
      return failure((this as Failure<T>).exception);
    }
  }

  /// Map the success value
  Result<R> map<R>(R Function(T data) transform) {
    return when(
      success: (data) => Result.success(transform(data)),
      failure: (exception) => Result.failure(exception),
    );
  }

  /// FlatMap (chain operations)
  Result<R> flatMap<R>(Result<R> Function(T data) transform) {
    return when(
      success: (data) => transform(data),
      failure: (exception) => Result.failure(exception),
    );
  }

  /// Get data or provide default value
  T getOrElse(T defaultValue) {
    return when(success: (data) => data, failure: (_) => defaultValue);
  }

  /// Get data or compute from exception
  T getOrElseFrom(T Function(Exception exception) onFailure) {
    return when(
      success: (data) => data,
      failure: (exception) => onFailure(exception),
    );
  }
}

/// Success result with data
class Success<T> extends Result<T> {
  final T data;

  const Success(this.data);

  @override
  String toString() => 'Success($data)';

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Success<T> &&
          runtimeType == other.runtimeType &&
          data == other.data;

  @override
  int get hashCode => data.hashCode;
}

/// Failure result with exception
class Failure<T> extends Result<T> {
  final Exception exception;

  const Failure(this.exception);

  @override
  String toString() => 'Failure($exception)';

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Failure<T> &&
          runtimeType == other.runtimeType &&
          exception == other.exception;

  @override
  int get hashCode => exception.hashCode;
}

/// Extension for Future<Result<T>>
extension FutureResultExtension<T> on Future<Result<T>> {
  /// Handle both success and failure cases
  Future<R> handle<R>({
    required R Function(T data) onSuccess,
    required R Function(Exception exception) onFailure,
  }) async {
    final result = await this;
    return result.when(success: onSuccess, failure: onFailure);
  }

  /// Get data or throw exception
  Future<T> getOrThrow() async {
    final result = await this;
    return result.when(
      success: (data) => data,
      failure: (exception) => throw exception,
    );
  }

  /// Get data or return null
  Future<T?> getOrNull() async {
    final result = await this;
    return result.dataOrNull;
  }
}

/// Helper to wrap async operations in Result
extension ResultAsyncExtension on Result {
  /// Wrap an async operation in Result
  static Future<Result<T>> tryAsync<T>(Future<T> Function() operation) async {
    try {
      final data = await operation();
      return Result.success(data);
    } on Exception catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(Exception(e.toString()));
    }
  }

  /// Wrap a sync operation in Result
  static Result<T> trySync<T>(T Function() operation) {
    try {
      final data = operation();
      return Result.success(data);
    } on Exception catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(Exception(e.toString()));
    }
  }
}
