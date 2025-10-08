import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/notification_preferences.dart';
import '../../domain/repositories/notification_repository.dart';
import '../datasources/notification_preferences_datasource.dart';

class NotificationRepositoryImpl implements NotificationRepository {
  final NotificationPreferencesDataSource _ds;

  NotificationRepositoryImpl(this._ds);

  @override
  Future<Result<NotificationPreferences>> getPreferences() async {
    try {
      final preferences = await _ds.getPreferences();
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
  Future<Result<NotificationPreferences>> updatePreferences(
    NotificationPreferences preferences,
  ) async {
    try {
      final updatedPrefs = await _ds.updatePreferences(preferences);
      return Result.success(updatedPrefs);
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
}