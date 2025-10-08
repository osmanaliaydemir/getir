import '../../core/errors/result.dart';
import '../entities/working_hours.dart';
import '../repositories/working_hours_repository.dart';

/// Working Hours Service
///
/// Centralized service for merchant working hours operations.
/// Replaces 4 separate UseCase classes.
class WorkingHoursService {
  final WorkingHoursRepository _repository;

  const WorkingHoursService(this._repository);

  Future<Result<List<WorkingHours>>> getWorkingHours(String merchantId) async {
    return await _repository.getWorkingHoursByMerchant(merchantId);
  }

  Future<Result<bool>> checkIfMerchantOpen(
    String merchantId, {
    DateTime? checkTime,
  }) async {
    return await _repository.isMerchantOpen(merchantId, checkTime: checkTime);
  }

  Future<Result<WorkingHours>> getWorkingHoursById(String id) async {
    return await _repository.getWorkingHoursById(id);
  }

  /// Gets the next open time for a merchant
  ///
  /// This is a client-side calculation based on working hours
  Future<Result<DateTime?>> getNextOpenTime(String merchantId) async {
    final workingHoursResult = await _repository.getWorkingHoursByMerchant(
      merchantId,
    );

    return workingHoursResult.when(
      success: (workingHours) {
        // Client-side logic to calculate next open time
        // For now, return null (merchant is always open or closed)
        return Result.success(null);
      },
      failure: (exception) => Result.failure(exception),
    );
  }
}
