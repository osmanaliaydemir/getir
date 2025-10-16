part of 'language_bloc.dart';

abstract class LanguageState extends Equatable {
  @override
  List<Object?> get props => [];
}

class LanguageInitial extends LanguageState {}

class LanguageLoading extends LanguageState {}

class LanguageLoaded extends LanguageState {
  final SupportedLanguage currentLanguage;
  LanguageLoaded(this.currentLanguage);

  @override
  List<Object?> get props => [currentLanguage];
}

class LanguageUpdated extends LanguageState {
  final SupportedLanguage language;
  LanguageUpdated(this.language);

  @override
  List<Object?> get props => [language];
}

class LanguageError extends LanguageState {
  final String message;
  LanguageError(this.message);

  @override
  List<Object?> get props => [message];
}
