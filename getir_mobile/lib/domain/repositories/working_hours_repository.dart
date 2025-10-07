import '../entities/working_hours.dart';

abstract class WorkingHoursRepository {
  /// Merchant'ın tüm çalışma saatlerini getirir (7 gün)
  Future<List<WorkingHours>> getWorkingHoursByMerchant(String merchantId);

  /// Merchant'ın şu an açık olup olmadığını kontrol eder
  Future<bool> isMerchantOpen(String merchantId, {DateTime? checkTime});

  /// Belirli bir çalışma saatini getirir
  Future<WorkingHours> getWorkingHoursById(String id);
}
