import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';
import '../../../domain/entities/working_hours.dart';

abstract class WorkingHoursState extends Equatable {
  const WorkingHoursState();

  @override
  List<Object?> get props => [];
}

/// İlk durum
class WorkingHoursInitial extends WorkingHoursState {}

/// Yükleniyor
class WorkingHoursLoading extends WorkingHoursState {}

/// Çalışma saatleri yüklendi
class WorkingHoursLoaded extends WorkingHoursState {
  final List<WorkingHours> workingHours;
  final bool isOpen;
  final (String dayName, TimeOfDay openTime)? nextOpenTime;

  const WorkingHoursLoaded({
    required this.workingHours,
    required this.isOpen,
    this.nextOpenTime,
  });

  @override
  List<Object?> get props => [workingHours, isOpen, nextOpenTime];
}

/// Açık/kapalı durumu kontrol edildi
class MerchantOpenStatusChecked extends WorkingHoursState {
  final bool isOpen;

  const MerchantOpenStatusChecked(this.isOpen);

  @override
  List<Object?> get props => [isOpen];
}

/// Sonraki açılış zamanı hesaplandı
class NextOpenTimeLoaded extends WorkingHoursState {
  final String dayName;
  final TimeOfDay openTime;

  const NextOpenTimeLoaded({
    required this.dayName,
    required this.openTime,
  });

  @override
  List<Object?> get props => [dayName, openTime];
}

/// Hata durumu
class WorkingHoursError extends WorkingHoursState {
  final String message;

  const WorkingHoursError(this.message);

  @override
  List<Object?> get props => [message];
}

/// Çalışma saatleri bulunamadı (henüz tanımlanmamış)
class WorkingHoursNotFound extends WorkingHoursState {}

