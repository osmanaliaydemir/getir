import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

/// Merchant çalışma saatleri entity
/// Her gün için ayrı bir WorkingHours kaydı olur
class WorkingHours extends Equatable {
  final String id;
  final String merchantId;
  final int
  dayOfWeek; // 0=Monday, 1=Tuesday, ..., 6=Sunday (DayOfWeek enum değeri)
  final TimeOfDay? openTime;
  final TimeOfDay? closeTime;
  final bool isClosed;
  final DateTime createdAt;

  const WorkingHours({
    required this.id,
    required this.merchantId,
    required this.dayOfWeek,
    this.openTime,
    this.closeTime,
    required this.isClosed,
    required this.createdAt,
  });

  @override
  List<Object?> get props => [
    id,
    merchantId,
    dayOfWeek,
    openTime,
    closeTime,
    isClosed,
    createdAt,
  ];

  /// Belirli bir zamanda açık mı kontrol eder
  bool isOpenAt(DateTime dateTime) {
    if (isClosed || openTime == null || closeTime == null) {
      return false;
    }

    final currentTime = TimeOfDay.fromDateTime(dateTime);
    final currentMinutes = currentTime.hour * 60 + currentTime.minute;
    final openMinutes = openTime!.hour * 60 + openTime!.minute;
    final closeMinutes = closeTime!.hour * 60 + closeTime!.minute;

    // Gece yarısını geçen durumlar için (örn: 22:00 - 02:00)
    if (closeMinutes < openMinutes) {
      return currentMinutes >= openMinutes || currentMinutes < closeMinutes;
    }

    return currentMinutes >= openMinutes && currentMinutes < closeMinutes;
  }

  /// Şu an açık mı kontrol eder
  bool isOpenNow() {
    return isOpenAt(DateTime.now());
  }

  /// Gün ismini döner (Türkçe)
  String getDayName() {
    return WorkingHoursHelper.getDayName(dayOfWeek);
  }

  /// Açılış-kapanış saatini formatlar
  String getFormattedHours() {
    if (isClosed || openTime == null || closeTime == null) {
      return 'Kapalı';
    }
    return '${WorkingHoursHelper.formatTimeOfDay(openTime!)} - ${WorkingHoursHelper.formatTimeOfDay(closeTime!)}';
  }

  WorkingHours copyWith({
    String? id,
    String? merchantId,
    int? dayOfWeek,
    TimeOfDay? openTime,
    TimeOfDay? closeTime,
    bool? isClosed,
    DateTime? createdAt,
  }) {
    return WorkingHours(
      id: id ?? this.id,
      merchantId: merchantId ?? this.merchantId,
      dayOfWeek: dayOfWeek ?? this.dayOfWeek,
      openTime: openTime ?? this.openTime,
      closeTime: closeTime ?? this.closeTime,
      isClosed: isClosed ?? this.isClosed,
      createdAt: createdAt ?? this.createdAt,
    );
  }
}

/// WorkingHours için helper metodlar
class WorkingHoursHelper {
  /// DayOfWeek değerini Türkçe gün ismine çevirir
  /// Backend: 0=Sunday, 1=Monday, ..., 6=Saturday (C# DayOfWeek)
  /// Flutter: 1=Monday, 2=Tuesday, ..., 7=Sunday (DateTime.weekday)
  static String getDayName(int dayOfWeek) {
    const dayNames = [
      'Pazar', // 0
      'Pazartesi', // 1
      'Salı', // 2
      'Çarşamba', // 3
      'Perşembe', // 4
      'Cuma', // 5
      'Cumartesi', // 6
    ];
    return dayNames[dayOfWeek % 7];
  }

  /// TimeOfDay'i 'HH:mm' formatında string'e çevirir
  static String formatTimeOfDay(TimeOfDay time) {
    final hour = time.hour.toString().padLeft(2, '0');
    final minute = time.minute.toString().padLeft(2, '0');
    return '$hour:$minute';
  }

  /// 'HH:mm:ss' veya 'HH:mm:ss.ffffff' formatındaki string'i TimeOfDay'e çevirir
  /// Backend TimeSpan formatı: "09:00:00" veya "09:00:00.0000000"
  static TimeOfDay? parseTimeSpan(String? timeSpan) {
    if (timeSpan == null || timeSpan.isEmpty) return null;

    try {
      final parts = timeSpan.split(':');
      if (parts.length >= 2) {
        final hour = int.parse(parts[0]);
        final minute = int.parse(parts[1]);
        return TimeOfDay(hour: hour, minute: minute);
      }
    } catch (e) {
      debugPrint('Error parsing TimeSpan: $timeSpan, error: $e');
    }
    return null;
  }

  /// TimeOfDay'i TimeSpan formatına çevirir (API request için)
  static String timeOfDayToTimeSpan(TimeOfDay time) {
    return '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}:00';
  }

  /// Bugünkü çalışma saatlerini listeden bulur
  static WorkingHours? getTodayWorkingHours(
    List<WorkingHours> workingHoursList,
  ) {
    final today = DateTime.now();
    // C# DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
    // Dart DateTime.weekday: Monday=1, ..., Sunday=7
    final backendDayOfWeek = today.weekday % 7; // Convert to C# DayOfWeek

    return workingHoursList.firstWhere(
      (wh) => wh.dayOfWeek == backendDayOfWeek,
      orElse: () => workingHoursList.first, // Fallback
    );
  }

  /// Sonraki açılış zamanını hesaplar
  /// Returns: (dayName, openTime) veya null if always closed
  static (String dayName, TimeOfDay openTime)? getNextOpenTime(
    List<WorkingHours> workingHoursList,
  ) {
    if (workingHoursList.isEmpty) return null;

    final now = DateTime.now();
    final currentDayOfWeek = now.weekday % 7;
    final currentTime = TimeOfDay.fromDateTime(now);
    final currentMinutes = currentTime.hour * 60 + currentTime.minute;

    // Önce bugün açılış var mı kontrol et
    final today = workingHoursList.firstWhere(
      (wh) => wh.dayOfWeek == currentDayOfWeek,
      orElse: () => workingHoursList.first,
    );

    if (!today.isClosed && today.openTime != null) {
      final openMinutes = today.openTime!.hour * 60 + today.openTime!.minute;
      if (currentMinutes < openMinutes) {
        return (today.getDayName(), today.openTime!);
      }
    }

    // Sonraki 7 gün için kontrol et
    for (int i = 1; i <= 7; i++) {
      final nextDayOfWeek = (currentDayOfWeek + i) % 7;
      final nextDay = workingHoursList.firstWhere(
        (wh) => wh.dayOfWeek == nextDayOfWeek,
        orElse: () => workingHoursList.first,
      );

      if (!nextDay.isClosed && nextDay.openTime != null) {
        return (nextDay.getDayName(), nextDay.openTime!);
      }
    }

    return null; // Hiç açılış yok
  }

  /// Bugün açık mı kontrol eder
  static bool isOpenToday(List<WorkingHours> workingHoursList) {
    final today = getTodayWorkingHours(workingHoursList);
    if (today == null) return false;
    return today.isOpenNow();
  }

  /// Çalışma saatlerini gün sırasına göre sıralar (Pazartesi'den başlayarak)
  static List<WorkingHours> sortByDay(List<WorkingHours> workingHoursList) {
    final sorted = List<WorkingHours>.from(workingHoursList);
    sorted.sort((a, b) {
      // Pazartesi'yi başa almak için: 1,2,3,4,5,6,0
      final aDay = a.dayOfWeek == 0 ? 7 : a.dayOfWeek;
      final bDay = b.dayOfWeek == 0 ? 7 : b.dayOfWeek;
      return aDay.compareTo(bDay);
    });
    return sorted;
  }
}
