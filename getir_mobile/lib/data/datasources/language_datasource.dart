import 'package:dio/dio.dart';
import '../../presentation/pages/profile/language_settings_page.dart';

abstract class LanguageDataSource {
  Future<SupportedLanguage> getCurrentLanguage();
  Future<SupportedLanguage> changeLanguage(SupportedLanguage language);
}

class LanguageDataSourceImpl implements LanguageDataSource {
  final Dio _dio;
  LanguageDataSourceImpl(this._dio);

  @override
  Future<SupportedLanguage> getCurrentLanguage() async {
    try {
      final response = await _dio.get('/api/v1/user/language');

      // Handle ApiResponse format
      final responseData = response.data;
      String languageCode = 'tr'; // Default to Turkish

      if (responseData is Map<String, dynamic>) {
        if (responseData['success'] == true && responseData['value'] != null) {
          languageCode =
              responseData['value']['languageCode'] as String? ?? 'tr';
        }
      } else if (responseData is Map<String, dynamic>) {
        languageCode = responseData['languageCode'] as String? ?? 'tr';
      }

      return SupportedLanguage.fromCode(languageCode);
    } catch (e) {
      // Return default language if API fails
      return SupportedLanguage.turkish;
    }
  }

  @override
  Future<SupportedLanguage> changeLanguage(SupportedLanguage language) async {
    final response = await _dio.put(
      '/api/v1/user/language',
      data: {'languageCode': language.code},
    );

    // Handle ApiResponse format
    final responseData = response.data;
    String languageCode = language.code;

    if (responseData is Map<String, dynamic>) {
      if (responseData['success'] == true && responseData['value'] != null) {
        languageCode =
            responseData['value']['languageCode'] as String? ?? language.code;
      }
    } else if (responseData is Map<String, dynamic>) {
      languageCode = responseData['languageCode'] as String? ?? language.code;
    }

    return SupportedLanguage.fromCode(languageCode);
  }
}
