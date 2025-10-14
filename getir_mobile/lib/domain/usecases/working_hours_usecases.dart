import 'package:flutter/material.dart';
import '../entities/working_hours.dart';
import '../repositories/working_hours_repository.dart';

/// Merchant'ın çalışma saatlerini getirir
class GetWorkingHoursUseCase {
  final WorkingHoursRepository _repository;

  GetWorkingHoursUseCase(this._repository);

  Future<List<WorkingHours>> call(String merchantId) async {
    return await _repository.getWorkingHoursByMerchant(merchantId);
  }
}

/// Merchant'ın açık olup olmadığını kontrol eder
class CheckIfMerchantOpenUseCase {
  final WorkingHoursRepository _repository;

  CheckIfMerchantOpenUseCase(this._repository);

  Future<bool> call(String merchantId, {DateTime? checkTime}) async {
    return await _repository.isMerchantOpen(merchantId, checkTime: checkTime);
  }
}

/// Merchant'ın sonraki açılış zamanını hesaplar
class GetNextOpenTimeUseCase {
  final WorkingHoursRepository _repository;

  GetNextOpenTimeUseCase(this._repository);

  Future<(String dayName, TimeOfDay openTime)?> call(String merchantId) async {
    final workingHours = await _repository.getWorkingHoursByMerchant(
      merchantId,
    );
    if (workingHours.isEmpty) return null;

    return WorkingHoursHelper.getNextOpenTime(workingHours);
  }
}

/// Merchant'ın bugünkü çalışma saatlerini getirir
class GetTodayWorkingHoursUseCase {
  final WorkingHoursRepository _repository;

  GetTodayWorkingHoursUseCase(this._repository);

  Future<WorkingHours?> call(String merchantId) async {
    final workingHours = await _repository.getWorkingHoursByMerchant(
      merchantId,
    );
    if (workingHours.isEmpty) return null;

    return WorkingHoursHelper.getTodayWorkingHours(workingHours);
  }
}
