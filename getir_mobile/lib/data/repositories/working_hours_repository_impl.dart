import '../../domain/entities/working_hours.dart';
import '../../domain/repositories/working_hours_repository.dart';
import '../datasources/working_hours_datasource.dart';

class WorkingHoursRepositoryImpl implements WorkingHoursRepository {
  final WorkingHoursDataSource _dataSource;

  WorkingHoursRepositoryImpl(this._dataSource);

  @override
  Future<List<WorkingHours>> getWorkingHoursByMerchant(
    String merchantId,
  ) async {
    try {
      return await _dataSource.getWorkingHoursByMerchant(merchantId);
    } catch (e) {
      throw Exception('Repository: Failed to fetch working hours: $e');
    }
  }

  @override
  Future<bool> isMerchantOpen(String merchantId, {DateTime? checkTime}) async {
    try {
      return await _dataSource.isMerchantOpen(merchantId, checkTime: checkTime);
    } catch (e) {
      throw Exception('Repository: Failed to check merchant status: $e');
    }
  }

  @override
  Future<WorkingHours> getWorkingHoursById(String id) async {
    try {
      return await _dataSource.getWorkingHoursById(id);
    } catch (e) {
      throw Exception('Repository: Failed to fetch working hours: $e');
    }
  }
}
