part of 'favorites_bloc.dart';

abstract class FavoritesEvent extends Equatable {
  @override
  List<Object?> get props => [];
}

class LoadFavorites extends FavoritesEvent {}

class AddToFavorites extends FavoritesEvent {
  final String productId;

  AddToFavorites(this.productId);

  @override
  List<Object?> get props => [productId];
}

class RemoveFromFavorites extends FavoritesEvent {
  final String productId;

  RemoveFromFavorites(this.productId);

  @override
  List<Object?> get props => [productId];
}

// 🔄 Pagination Events
class LoadMoreFavorites extends FavoritesEvent {}

class RefreshFavorites extends FavoritesEvent {}
