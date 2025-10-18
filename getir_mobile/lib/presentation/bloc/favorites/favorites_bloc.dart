import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../core/models/pagination_model.dart';
import '../../../domain/entities/favorite_product.dart';
import '../../../domain/services/favorites_service.dart';

part 'favorites_event.dart';
part 'favorites_state.dart';

class FavoritesBloc extends Bloc<FavoritesEvent, FavoritesState> {
  final FavoritesService _favoritesService;

  FavoritesBloc(this._favoritesService) : super(FavoritesInitial()) {
    on<LoadFavorites>(_onLoadFavorites);
    on<AddToFavorites>(_onAddToFavorites);
    on<RemoveFromFavorites>(_onRemoveFromFavorites);
    on<LoadMoreFavorites>(_onLoadMoreFavorites);
    on<RefreshFavorites>(_onRefreshFavorites);
  }

  Future<void> _onLoadFavorites(
    LoadFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    emit(FavoritesLoading());

    final result = await _favoritesService.getFavorites();

    result.when(
      success: (favorites) => emit(FavoritesLoaded(favorites)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(FavoritesError(message));
      },
    );
  }

  Future<void> _onAddToFavorites(
    AddToFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    final result = await _favoritesService.addToFavorites(event.productId);

    result.when(
      success: (_) {
        // Reload favorites after adding
        add(LoadFavorites());
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(FavoritesError(message));
      },
    );
  }

  Future<void> _onRemoveFromFavorites(
    RemoveFromFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    final result = await _favoritesService.removeFromFavorites(event.productId);

    result.when(
      success: (_) {
        // Reload favorites after removing
        add(LoadFavorites());
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(FavoritesError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }

  // 🔄 ============= PAGINATION HANDLERS =============

  Future<void> _onLoadMoreFavorites(
    LoadMoreFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    if (state is! FavoritesLoaded) return;
    final currentState = state as FavoritesLoaded;

    if (currentState.pagination == null ||
        !currentState.pagination!.hasNextPage ||
        currentState.pagination!.isLoading) {
      return;
    }

    final loadingPagination = currentState.pagination!.setLoading(true);
    emit(
      FavoritesLoaded(currentState.favorites, pagination: loadingPagination),
    );

    final nextPage = currentState.pagination!.nextPage;
    final result = await _favoritesService.getFavorites(page: nextPage);

    result.when(
      success: (newFavorites) {
        final updatedFavorites = [...currentState.favorites, ...newFavorites];
        final updatedPagination = currentState.pagination!
            .addItems(newFavorites)
            .copyWith(
              currentPage: nextPage,
              hasNextPage: newFavorites.length >= 20,
              isLoading: false,
            );
        emit(FavoritesLoaded(updatedFavorites, pagination: updatedPagination));
      },
      failure: (exception) {
        final errorPagination = currentState.pagination!.setLoading(false);
        emit(
          FavoritesLoaded(currentState.favorites, pagination: errorPagination),
        );
      },
    );
  }

  Future<void> _onRefreshFavorites(
    RefreshFavorites event,
    Emitter<FavoritesState> emit,
  ) async {
    final result = await _favoritesService.getFavorites();

    result.when(
      success: (favorites) {
        final pagination = PaginationModel<FavoriteProduct>(
          items: favorites,
          currentPage: 1,
          totalPages: 999,
          totalItems: favorites.length,
          hasNextPage: favorites.length >= 20,
          hasPreviousPage: false,
          isLoading: false,
          isRefreshing: false,
        );
        emit(FavoritesLoaded(favorites, pagination: pagination));
      },
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(FavoritesError(message));
      },
    );
  }
}
