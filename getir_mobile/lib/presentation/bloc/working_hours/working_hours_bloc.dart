import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/working_hours.dart';
import '../../../domain/usecases/working_hours_usecases.dart';
import 'working_hours_event.dart';
import 'working_hours_state.dart';

class WorkingHoursBloc extends Bloc<WorkingHoursEvent, WorkingHoursState> {
  final GetWorkingHoursUseCase _getWorkingHoursUseCase;
  final CheckIfMerchantOpenUseCase _checkIfMerchantOpenUseCase;
  final GetNextOpenTimeUseCase _getNextOpenTimeUseCase;

  WorkingHoursBloc({
    required GetWorkingHoursUseCase getWorkingHoursUseCase,
    required CheckIfMerchantOpenUseCase checkIfMerchantOpenUseCase,
    required GetNextOpenTimeUseCase getNextOpenTimeUseCase,
  })  : _getWorkingHoursUseCase = getWorkingHoursUseCase,
        _checkIfMerchantOpenUseCase = checkIfMerchantOpenUseCase,
        _getNextOpenTimeUseCase = getNextOpenTimeUseCase,
        super(WorkingHoursInitial()) {
    on<LoadWorkingHours>(_onLoadWorkingHours);
    on<CheckMerchantOpen>(_onCheckMerchantOpen);
    on<LoadNextOpenTime>(_onLoadNextOpenTime);
  }

  Future<void> _onLoadWorkingHours(
    LoadWorkingHours event,
    Emitter<WorkingHoursState> emit,
  ) async {
    emit(WorkingHoursLoading());

    final result = await _getWorkingHoursUseCase(event.merchantId);

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

        emit(WorkingHoursLoaded(
          workingHours: WorkingHoursHelper.sortByDay(workingHours),
          isOpen: isOpen,
          nextOpenTime: nextOpenTime,
        ));
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

    final result = await _checkIfMerchantOpenUseCase(
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

    final result = await _getNextOpenTimeUseCase(event.merchantId);

    result.when(
      success: (nextOpenTime) {
        if (nextOpenTime == null) {
          emit(const WorkingHoursError('Çalışma saatleri bulunamadı'));
          return;
        }

        emit(NextOpenTimeLoaded(
          dayName: nextOpenTime.$1,
          openTime: nextOpenTime.$2,
        ));
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
}

