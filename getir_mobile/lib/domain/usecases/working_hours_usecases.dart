import 'package:flutter/material.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/working_hours.dart';
import '../repositories/working_hours_repository.dart';

/// Get Working Hours Use Case
class GetWorkingHoursUseCase {
  final WorkingHoursRepository _repository;

  GetWorkingHoursUseCase(this._repository);

  Future<Result<List<WorkingHours>>> call(String merchantId) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    return await _repository.getWorkingHoursByMerchant(merchantId);
  }
}

/// Check If Merchant Open Use Case
class CheckIfMerchantOpenUseCase {
  final WorkingHoursRepository _repository;

  CheckIfMerchantOpenUseCase(this._repository);

  Future<Result<bool>> call(String merchantId, {DateTime? checkTime}) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    return await _repository.isMerchantOpen(merchantId, checkTime: checkTime);
  }
}

/// Get Next Open Time Use Case
class GetNextOpenTimeUseCase {
  final WorkingHoursRepository _repository;

  GetNextOpenTimeUseCase(this._repository);

  Future<Result<(String dayName, TimeOfDay openTime)?>> call(
    String merchantId,
  ) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    final result = await _repository.getWorkingHoursByMerchant(merchantId);

    return result.when(
      success: (workingHours) {
        if (workingHours.isEmpty) {
          return Result.success(null);
        }
        final nextOpenTime = WorkingHoursHelper.getNextOpenTime(workingHours);
        return Result.success(nextOpenTime);
      },
      failure: (exception) => Result.failure(exception),
    );
  }
}

/// Get Today Working Hours Use Case
class GetTodayWorkingHoursUseCase {
  final WorkingHoursRepository _repository;

  GetTodayWorkingHoursUseCase(this._repository);

  Future<Result<WorkingHours?>> call(String merchantId) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    final result = await _repository.getWorkingHoursByMerchant(merchantId);

    return result.when(
      success: (workingHours) {
        if (workingHours.isEmpty) {
          return Result.success(null);
        }
        final todayHours = WorkingHoursHelper.getTodayWorkingHours(
          workingHours,
        );
        return Result.success(todayHours);
      },
      failure: (exception) => Result.failure(exception),
    );
  }
}
