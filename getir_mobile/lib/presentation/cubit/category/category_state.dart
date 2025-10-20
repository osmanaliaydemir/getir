part of 'category_cubit.dart';

/// CategoryState - Kategori state'leri
abstract class CategoryState extends Equatable {
  const CategoryState();

  @override
  List<Object?> get props => [];
}

/// İlk başlangıç state'i
class CategoryInitial extends CategoryState {}

/// Kategoriler yüklenirken
class CategoryLoading extends CategoryState {}

/// Kategoriler başarıyla yüklendi
class CategoryLoaded extends CategoryState {
  final List<ServiceCategory> categories;

  const CategoryLoaded(this.categories);

  @override
  List<Object?> get props => [categories];
}

/// Kategori yükleme hatası
class CategoryError extends CategoryState {
  final String message;

  const CategoryError(this.message);

  @override
  List<Object?> get props => [message];
}
