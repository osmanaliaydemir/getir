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
}
