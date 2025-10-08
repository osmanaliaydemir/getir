import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/working_hours.dart';
import '../../domain/repositories/working_hours_repository.dart';
import '../datasources/working_hours_datasource.dart';

class WorkingHoursRepositoryImpl implements WorkingHoursRepository {
  final WorkingHoursDataSource _dataSource;

  WorkingHoursRepositoryImpl(this._dataSource);

  @override
  Future<Result<List<WorkingHours>>> getWorkingHoursByMerchant(
    String merchantId,
  ) async {
    try {
      final workingHours = await _dataSource.getWorkingHoursByMerchant(
        merchantId,
      );
      return Result.success(workingHours);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get working hours: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<bool>> isMerchantOpen(
    String merchantId, {
    DateTime? checkTime,
  }) async {
    try {
      final isOpen = await _dataSource.isMerchantOpen(
        merchantId,
        checkTime: checkTime,
      );
      return Result.success(isOpen);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to check if merchant is open: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<WorkingHours>> getWorkingHoursById(String id) async {
    try {
      final workingHours = await _dataSource.getWorkingHoursById(id);
      return Result.success(workingHours);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get working hours: ${e.toString()}'),
      );
    }
  }
}
