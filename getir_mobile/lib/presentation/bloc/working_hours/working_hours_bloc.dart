import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/working_hours.dart';
import '../../../domain/services/working_hours_service.dart';
import 'working_hours_event.dart';
import 'working_hours_state.dart';

class WorkingHoursBloc extends Bloc<WorkingHoursEvent, WorkingHoursState> {
  final WorkingHoursService _workingHoursService;

  WorkingHoursBloc(this._workingHoursService) : super(WorkingHoursInitial()) {
    on<LoadWorkingHours>(_onLoadWorkingHours);
    on<CheckMerchantOpen>(_onCheckMerchantOpen);
    on<LoadNextOpenTime>(_onLoadNextOpenTime);
  }

  Future<void> _onLoadWorkingHours(
    LoadWorkingHours event,
    Emitter<WorkingHoursState> emit,
  ) async {
    emit(WorkingHoursLoading());

    final result = await _workingHoursService.getWorkingHours(event.merchantId);

    result.when(
      success: (workingHours) {
        if (workingHours.isEmpty) {
          emit(WorkingHoursNotFound());
          return;
        }

        // Açık/kapalı durumunu ve sonraki açılış zamanını hesapla
        final isOpen = WorkingHoursHelper.isOpenToday(workingHours);
        final nextOpenTime = isOpen
            ? null
            : WorkingHoursHelper.getNextOpenTime(workingHours);

        emit(
          WorkingHoursLoaded(
            workingHours: WorkingHoursHelper.sortByDay(workingHours),
            isOpen: isOpen,
            nextOpenTime: nextOpenTime,
          ),
        );
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(WorkingHoursError(message));
      },
    );
  }

  Future<void> _onCheckMerchantOpen(
    CheckMerchantOpen event,
    Emitter<WorkingHoursState> emit,
  ) async {
    emit(WorkingHoursLoading());

    final result = await _workingHoursService.checkIfMerchantOpen(
      event.merchantId,
      checkTime: event.checkTime,
    );

    result.when(
      success: (isOpen) => emit(MerchantOpenStatusChecked(isOpen)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(WorkingHoursError(message));
      },
    );
  }

  Future<void> _onLoadNextOpenTime(
    LoadNextOpenTime event,
    Emitter<WorkingHoursState> emit,
  ) async {
    emit(WorkingHoursLoading());

    final result = await _workingHoursService.getNextOpenTime(event.merchantId);

    result.when(
      success: (nextOpenTime) {
        if (nextOpenTime == null) {
          emit(const WorkingHoursError('Çalışma saatleri bulunamadı'));
          return;
        }

        // Format the datetime for display
        final dayName = _getDayName(nextOpenTime.weekday);
        final openTime = TimeOfDay(
          hour: nextOpenTime.hour,
          minute: nextOpenTime.minute,
        );

        emit(NextOpenTimeLoaded(dayName: dayName, openTime: openTime));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(WorkingHoursError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }

  /// Helper to convert weekday number to Turkish day name
  String _getDayName(int weekday) {
    const days = [
      'Pazartesi',
      'Salı',
      'Çarşamba',
      'Perşembe',
      'Cuma',
      'Cumartesi',
      'Pazar',
    ];
    return days[weekday - 1];
  }
}
