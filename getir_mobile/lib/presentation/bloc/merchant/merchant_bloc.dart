import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/merchant.dart';
import '../../../domain/services/merchant_service.dart';

// Events
abstract class MerchantEvent extends Equatable {
  const MerchantEvent();

  @override
  List<Object?> get props => [];
}

class LoadMerchants extends MerchantEvent {
  final int page;
  final int limit;
  final String? search;
  final String? category;
  final double? latitude;
  final double? longitude;
  final double? radius;

  const LoadMerchants({
    this.page = 1,
    this.limit = 20,
    this.search,
    this.category,
    this.latitude,
    this.longitude,
    this.radius,
  });

  @override
  List<Object?> get props => [
    page,
    limit,
    search,
    category,
    latitude,
    longitude,
    radius,
  ];
}

class LoadMerchantById extends MerchantEvent {
  final String id;

  const LoadMerchantById(this.id);

  @override
  List<Object> get props => [id];
}

class SearchMerchants extends MerchantEvent {
  final String query;

  const SearchMerchants(this.query);

  @override
  List<Object> get props => [query];
}

class LoadNearbyMerchants extends MerchantEvent {
  final double latitude;
  final double longitude;
  final double radius;

  const LoadNearbyMerchants({
    required this.latitude,
    required this.longitude,
    this.radius = 5.0,
  });

  @override
  List<Object> get props => [latitude, longitude, radius];
}

class LoadNearbyMerchantsByCategory extends MerchantEvent {
  final double latitude;
  final double longitude;
  final int
  categoryType; // ServiceCategoryType value (1=Restaurant, 2=Market, vb.)
  final double radius;

  const LoadNearbyMerchantsByCategory({
    required this.latitude,
    required this.longitude,
    required this.categoryType,
    this.radius = 5.0,
  });

  @override
  List<Object> get props => [latitude, longitude, categoryType, radius];
}

// States
abstract class MerchantState extends Equatable {
  const MerchantState();

  @override
  List<Object?> get props => [];
}

class MerchantInitial extends MerchantState {}

class MerchantLoading extends MerchantState {}

class MerchantLoaded extends MerchantState {
  final Merchant merchant;

  const MerchantLoaded(this.merchant);

  @override
  List<Object> get props => [merchant];
}

class MerchantsLoaded extends MerchantState {
  final List<Merchant> merchants;

  const MerchantsLoaded(this.merchants);

  @override
  List<Object> get props => [merchants];
}

class MerchantError extends MerchantState {
  final String message;

  const MerchantError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class MerchantBloc extends Bloc<MerchantEvent, MerchantState> {
  final MerchantService _merchantService;

  MerchantBloc(this._merchantService) : super(MerchantInitial()) {
    on<LoadMerchants>(_onLoadMerchants);
    on<LoadMerchantById>(_onLoadMerchantById);
    on<SearchMerchants>(_onSearchMerchants);
    on<LoadNearbyMerchants>(_onLoadNearbyMerchants);
    on<LoadNearbyMerchantsByCategory>(_onLoadNearbyMerchantsByCategory);
  }

  Future<void> _onLoadMerchants(
    LoadMerchants event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());

    final result = await _merchantService.getMerchants(
      page: event.page,
      limit: event.limit,
      search: event.search,
      category: event.category,
    );

    result.when(
      success: (merchants) => emit(MerchantsLoaded(merchants)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(MerchantError(message));
      },
    );
  }

  Future<void> _onLoadMerchantById(
    LoadMerchantById event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());

    final result = await _merchantService.getMerchantById(event.id);

    result.when(
      success: (merchant) => emit(MerchantLoaded(merchant)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(MerchantError(message));
      },
    );
  }

  Future<void> _onSearchMerchants(
    SearchMerchants event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());

    final result = await _merchantService.searchMerchants(event.query);

    result.when(
      success: (merchants) => emit(MerchantsLoaded(merchants)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(MerchantError(message));
      },
    );
  }

  Future<void> _onLoadNearbyMerchants(
    LoadNearbyMerchants event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());

    final result = await _merchantService.getNearbyMerchants(
      latitude: event.latitude,
      longitude: event.longitude,
      radius: event.radius,
    );

    result.when(
      success: (merchants) => emit(MerchantsLoaded(merchants)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(MerchantError(message));
      },
    );
  }

  Future<void> _onLoadNearbyMerchantsByCategory(
    LoadNearbyMerchantsByCategory event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());

    final result = await _merchantService.getNearbyMerchantsByCategory(
      latitude: event.latitude,
      longitude: event.longitude,
      categoryType: event.categoryType,
      radius: event.radius,
    );

    result.when(
      success: (merchants) => emit(MerchantsLoaded(merchants)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(MerchantError(message));
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
}
