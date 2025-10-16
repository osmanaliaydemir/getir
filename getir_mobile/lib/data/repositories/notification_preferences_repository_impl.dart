import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/notification_preferences.dart';
import '../../domain/repositories/notification_preferences_repository.dart';
import '../datasources/notification_preferences_datasource.dart';

class NotificationPreferencesRepositoryImpl
    implements INotificationPreferencesRepository {
  final NotificationPreferencesDataSource _dataSource;

  NotificationPreferencesRepositoryImpl(this._dataSource);

  @override
  Future<Result<NotificationPreferences>> getNotificationPreferences() async {
    try {
      final preferences = await _dataSource.getNotificationPreferences();
      return Result.success(preferences);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get notification preferences: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<NotificationPreferences>> updateNotificationPreferences(
    NotificationPreferences preferences,
  ) async {
    try {
      final updatedPreferences = await _dataSource
          .updateNotificationPreferences(preferences);
      return Result.success(updatedPreferences);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to update notification preferences: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<NotificationPreferences>> resetToDefaults() async {
    try {
      final preferences = await _dataSource.resetToDefaults();
      return Result.success(preferences);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to reset to defaults: ${e.toString()}'),
      );
    }
  }
}
