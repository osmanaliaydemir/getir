import '../../core/errors/result.dart';
import '../../presentation/pages/profile/language_settings_page.dart';

abstract class ILanguageRepository {
  Future<Result<SupportedLanguage>> getCurrentLanguage();
  Future<Result<SupportedLanguage>> changeLanguage(SupportedLanguage language);
}
