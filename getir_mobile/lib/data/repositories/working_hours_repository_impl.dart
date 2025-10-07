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
    return await _dataSource.getWorkingHoursByMerchant(merchantId);
  }

  @override
  Future<bool> isMerchantOpen(String merchantId, {DateTime? checkTime}) async {
    return await _dataSource.isMerchantOpen(merchantId, checkTime: checkTime);
  }

  @override
  Future<WorkingHours> getWorkingHoursById(String id) async {
    return await _dataSource.getWorkingHoursById(id);
  }
}
