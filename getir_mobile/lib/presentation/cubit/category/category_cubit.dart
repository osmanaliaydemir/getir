import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/service_category.dart';
import '../../../domain/entities/service_category_type.dart';
import '../../../domain/repositories/service_category_repository.dart';
import '../../../core/services/logger_service.dart';

part 'category_state.dart';

/// CategoryCubit - Servis kategorilerini yönetir
/// Ana sayfada kullanılacak kategorileri API'den çeker
class CategoryCubit extends Cubit<CategoryState> {
  final IServiceCategoryRepository _repository;

  CategoryCubit(this._repository) : super(CategoryInitial());

  /// Tüm aktif kategorileri yükle
  Future<void> loadAllActiveCategories() async {
    emit(CategoryLoading());

    try {
      final result = await _repository.getAllActiveCategories();

      result.when(
        success: (categories) {
          logger.info(
            'Categories loaded successfully',
            tag: 'CategoryCubit',
            context: {'count': categories.length},
          );
          emit(CategoryLoaded(categories));
        },
        failure: (exception) {
          logger.error(
            'Failed to load categories',
            tag: 'CategoryCubit',
            error: exception,
          );
          emit(CategoryError(exception.toString()));
        },
      );
    } catch (e, stackTrace) {
      logger.error(
        'Unexpected error loading categories',
        tag: 'CategoryCubit',
        error: e,
        stackTrace: stackTrace,
      );
      emit(CategoryError('Kategoriler yüklenemedi: ${e.toString()}'));
    }
  }

  /// Türüne göre kategorileri yükle
  Future<void> loadCategoriesByType(ServiceCategoryType type) async {
    emit(CategoryLoading());

    try {
      final result = await _repository.getActiveServiceCategoriesByType(type);

      result.when(
        success: (categories) {
          logger.info(
            'Categories by type loaded successfully',
            tag: 'CategoryCubit',
            context: {'type': type.value, 'count': categories.length},
          );
          emit(CategoryLoaded(categories));
        },
        failure: (exception) {
          logger.error(
            'Failed to load categories by type',
            tag: 'CategoryCubit',
            error: exception,
            context: {'type': type.value},
          );
          emit(CategoryError(exception.toString()));
        },
      );
    } catch (e, stackTrace) {
      logger.error(
        'Unexpected error loading categories by type',
        tag: 'CategoryCubit',
        error: e,
        stackTrace: stackTrace,
        context: {'type': type.value},
      );
      emit(CategoryError('Kategoriler yüklenemedi: ${e.toString()}'));
    }
  }

  /// Kategorileri yeniden yükle
  Future<void> refreshCategories() async {
    await loadAllActiveCategories();
  }
}
