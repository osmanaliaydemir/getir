import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../presentation/pages/profile/language_settings_page.dart';
import '../../domain/repositories/language_repository.dart';
import '../datasources/language_datasource.dart';

class LanguageRepositoryImpl implements ILanguageRepository {
  final LanguageDataSource _dataSource;

  LanguageRepositoryImpl(this._dataSource);

  @override
  Future<Result<SupportedLanguage>> getCurrentLanguage() async {
    try {
      final language = await _dataSource.getCurrentLanguage();
      return Result.success(language);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get current language: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<SupportedLanguage>> changeLanguage(
    SupportedLanguage language,
  ) async {
    try {
      final updatedLanguage = await _dataSource.changeLanguage(language);
      return Result.success(updatedLanguage);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to change language: ${e.toString()}'),
      );
    }
  }
}
