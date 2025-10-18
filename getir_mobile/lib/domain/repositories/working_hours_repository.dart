import '../../core/errors/result.dart';
import '../entities/working_hours.dart';

abstract class IWorkingHoursRepository {
  /// Merchant'ın tüm çalışma saatlerini getirir (7 gün)
  Future<Result<List<WorkingHours>>> getWorkingHoursByMerchant(
    String merchantId,
  );

  /// Merchant'ın şu an açık olup olmadığını kontrol eder
  Future<Result<bool>> isMerchantOpen(String merchantId, {DateTime? checkTime});

  /// Belirli bir çalışma saatini getirir
  Future<Result<WorkingHours>> getWorkingHoursById(String id);
}
