part of 'language_cubit.dart';

/// LanguageState
///
/// Represents current app language/locale
class LanguageState extends Equatable {
  final Locale locale;

  const LanguageState({required this.locale});

  String get languageCode => locale.languageCode;

  @override
  List<Object?> get props => [locale];
}
