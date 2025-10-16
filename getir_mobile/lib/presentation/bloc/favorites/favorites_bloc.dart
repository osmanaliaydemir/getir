import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/favorite_product.dart';
import '../../../domain/services/favorites_service.dart';

part 'favorites_event.dart';
part 'favorites_state.dart';

class FavoritesBloc extends Bloc<FavoritesEvent, FavoritesState> {
  final FavoritesService _favoritesService;

  FavoritesBloc(this._favoritesService) : super(FavoritesInitial()) {
    on<LoadFavorites>((event, emit) async {
      emit(FavoritesLoading());

      final result = await _favoritesService.getFavorites();

      result.when(
        success: (favorites) => emit(FavoritesLoaded(favorites)),
        failure: (exception) {
          final message = _getErrorMessage(exception);
          emit(FavoritesError(message));
        },
      );
    });

    on<AddToFavorites>((event, emit) async {
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
    });

    on<RemoveFromFavorites>((event, emit) async {
      final result = await _favoritesService.removeFromFavorites(
        event.productId,
      );

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
    });
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
