import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/services/language_service.dart';
import '../../pages/profile/language_settings_page.dart';

part 'language_event.dart';
part 'language_state.dart';

class LanguageBloc extends Bloc<LanguageEvent, LanguageState> {
  final LanguageService _languageService;

  LanguageBloc(this._languageService) : super(LanguageInitial()) {
    on<LoadLanguage>((event, emit) async {
      emit(LanguageLoading());

      final result = await _languageService.getCurrentLanguage();

      result.when(
        success: (language) => emit(LanguageLoaded(language)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(LanguageError(message));
        },
      );
    });

    on<ChangeLanguage>((event, emit) async {
      emit(LanguageLoading());

      final result = await _languageService.changeLanguage(event.language);

      result.when(
        success: (language) => emit(LanguageUpdated(language)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(LanguageError(message));
        },
      );
    });
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
