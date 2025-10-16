part of 'language_bloc.dart';

abstract class LanguageEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadLanguage extends LanguageEvent {}

class ChangeLanguage extends LanguageEvent {
  final SupportedLanguage language;

  ChangeLanguage(this.language);

  @override
  List<Object?> get props => [language];
}
