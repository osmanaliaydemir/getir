import '../../core/errors/result.dart';
import '../../presentation/pages/profile/language_settings_page.dart';
import '../repositories/language_repository.dart';

/// Language Service
///
/// Centralized service for language operations.
class LanguageService {
  final ILanguageRepository _repository;

  const LanguageService(this._repository);

  Future<Result<SupportedLanguage>> getCurrentLanguage() async {
    return await _repository.getCurrentLanguage();
  }

  Future<Result<SupportedLanguage>> changeLanguage(
    SupportedLanguage language,
  ) async {
    return await _repository.changeLanguage(language);
  }
}
