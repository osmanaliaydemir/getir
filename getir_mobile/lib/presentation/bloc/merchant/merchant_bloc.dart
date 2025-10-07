import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../domain/entities/merchant.dart';
import '../../../domain/usecases/merchant_usecases.dart';

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
  final GetMerchantsUseCase _getMerchantsUseCase;
  final GetMerchantByIdUseCase _getMerchantByIdUseCase;
  final SearchMerchantsUseCase _searchMerchantsUseCase;
  final GetNearbyMerchantsUseCase _getNearbyMerchantsUseCase;
  final GetNearbyMerchantsByCategoryUseCase
  _getNearbyMerchantsByCategoryUseCase;

  MerchantBloc({
    required GetMerchantsUseCase getMerchantsUseCase,
    required GetMerchantByIdUseCase getMerchantByIdUseCase,
    required SearchMerchantsUseCase searchMerchantsUseCase,
    required GetNearbyMerchantsUseCase getNearbyMerchantsUseCase,
    required GetNearbyMerchantsByCategoryUseCase
    getNearbyMerchantsByCategoryUseCase,
  }) : _getMerchantsUseCase = getMerchantsUseCase,
       _getMerchantByIdUseCase = getMerchantByIdUseCase,
       _searchMerchantsUseCase = searchMerchantsUseCase,
       _getNearbyMerchantsUseCase = getNearbyMerchantsUseCase,
       _getNearbyMerchantsByCategoryUseCase =
           getNearbyMerchantsByCategoryUseCase,
       super(MerchantInitial()) {
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
    try {
      final merchants = await _getMerchantsUseCase(
        page: event.page,
        limit: event.limit,
        search: event.search,
        category: event.category,
        latitude: event.latitude,
        longitude: event.longitude,
        radius: event.radius,
      );
      emit(MerchantsLoaded(merchants));
    } catch (e) {
      emit(MerchantError(e.toString()));
    }
  }

  Future<void> _onLoadMerchantById(
    LoadMerchantById event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());
    try {
      final merchant = await _getMerchantByIdUseCase(event.id);
      emit(MerchantLoaded(merchant));
    } catch (e) {
      emit(MerchantError(e.toString()));
    }
  }

  Future<void> _onSearchMerchants(
    SearchMerchants event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());
    try {
      final merchants = await _searchMerchantsUseCase(event.query);
      emit(MerchantsLoaded(merchants));
    } catch (e) {
      emit(MerchantError(e.toString()));
    }
  }

  Future<void> _onLoadNearbyMerchants(
    LoadNearbyMerchants event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());
    try {
      final merchants = await _getNearbyMerchantsUseCase(
        latitude: event.latitude,
        longitude: event.longitude,
        radius: event.radius,
      );
      emit(MerchantsLoaded(merchants));
    } catch (e) {
      emit(MerchantError(e.toString()));
    }
  }

  Future<void> _onLoadNearbyMerchantsByCategory(
    LoadNearbyMerchantsByCategory event,
    Emitter<MerchantState> emit,
  ) async {
    emit(MerchantLoading());
    try {
      final merchants = await _getNearbyMerchantsByCategoryUseCase(
        latitude: event.latitude,
        longitude: event.longitude,
        categoryType: event.categoryType,
        radius: event.radius,
      );
      emit(MerchantsLoaded(merchants));
    } catch (e) {
      emit(MerchantError(e.toString()));
    }
  }
}
