import 'package:equatable/equatable.dart';

abstract class WorkingHoursEvent extends Equatable {
  const WorkingHoursEvent();

  @override
  List<Object?> get props => [];
}

/// Merchant'ın çalışma saatlerini yükle
class LoadWorkingHours extends WorkingHoursEvent {
  final String merchantId;

  const LoadWorkingHours(this.merchantId);

  @override
  List<Object?> get props => [merchantId];
}

/// Merchant'ın açık olup olmadığını kontrol et
class CheckMerchantOpen extends WorkingHoursEvent {
  final String merchantId;
  final DateTime? checkTime;

  const CheckMerchantOpen(this.merchantId, {this.checkTime});

  @override
  List<Object?> get props => [merchantId, checkTime];
}

/// Sonraki açılış zamanını hesapla
class LoadNextOpenTime extends WorkingHoursEvent {
  final String merchantId;

  const LoadNextOpenTime(this.merchantId);

  @override
  List<Object?> get props => [merchantId];
}
