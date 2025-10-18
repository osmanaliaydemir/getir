part of 'favorites_bloc.dart';

abstract class FavoritesState extends Equatable {
  @override
  List<Object?> get props => [];
}

class FavoritesInitial extends FavoritesState {}

class FavoritesLoading extends FavoritesState {}

class FavoritesLoaded extends FavoritesState {
  final List<FavoriteProduct> favorites;
  final PaginationModel<FavoriteProduct>? pagination;

  FavoritesLoaded(this.favorites, {this.pagination});

  @override
  List<Object?> get props => [favorites, pagination];

  bool get hasPagination => pagination != null;
  bool get canLoadMore => pagination?.hasNextPage ?? false;
}

class FavoritesError extends FavoritesState {
  final String message;
  FavoritesError(this.message);

  @override
  List<Object?> get props => [message];
}
