import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/notification.dart';
import '../../domain/repositories/notifications_feed_repository.dart';
import '../datasources/notifications_feed_datasource.dart';

class NotificationsFeedRepositoryImpl implements NotificationsFeedRepository {
  final NotificationsFeedDataSource _ds;

  NotificationsFeedRepositoryImpl(this._ds);

  @override
  Future<Result<List<AppNotification>>> getNotifications({
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final dtos = await _ds.getNotifications(page: page, pageSize: pageSize);
      final notifications = dtos.map((e) => e.toDomain()).toList();
      return Result.success(notifications);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get notifications: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> markAsRead(String notificationId) async {
    try {
      await _ds.markAsRead(notificationId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to mark as read: ${e.toString()}'),
      );
    }
  }
}
