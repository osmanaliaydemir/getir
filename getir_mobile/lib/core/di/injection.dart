import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_performance/firebase_performance.dart';
import '../services/local_storage_service.dart';
import '../services/encryption_service.dart';
import '../services/search_history_service.dart';
import '../config/environment_config.dart';
import 'injection.config.dart';

// Manual DI imports (temporary)
import '../../data/datasources/merchant_datasource.dart';
import '../../data/datasources/product_datasource.dart';
import '../../data/datasources/cart_datasource.dart';
import '../../data/datasources/address_datasource.dart';
import '../../data/datasources/order_datasource.dart';
import '../../data/datasources/profile_datasource.dart';
import '../../data/datasources/notification_preferences_datasource.dart';
import '../../data/datasources/notifications_feed_datasource.dart';
import '../../data/datasources/working_hours_datasource.dart';
import '../../data/datasources/review_datasource.dart';
import '../../data/repositories/merchant_repository_impl.dart';
import '../../data/repositories/product_repository_impl.dart';
import '../../data/repositories/cart_repository_impl.dart';
import '../../data/repositories/address_repository_impl.dart';
import '../../data/repositories/order_repository_impl.dart';
import '../../data/repositories/profile_repository_impl.dart';
import '../../data/repositories/notification_repository_impl.dart';
import '../../data/repositories/notifications_feed_repository_impl.dart';
import '../../data/repositories/working_hours_repository_impl.dart';
import '../../data/repositories/review_repository_impl.dart';

// Use Cases
import '../../domain/usecases/merchant_usecases.dart';
import '../../domain/usecases/product_usecases.dart';
import '../../domain/usecases/cart_usecases.dart';
import '../../domain/usecases/address_usecases.dart';
import '../../domain/usecases/order_usecases.dart';
import '../../domain/usecases/profile_usecases.dart';
import '../../domain/usecases/notification_usecases.dart';
import '../../domain/usecases/working_hours_usecases.dart';
import '../../domain/usecases/review_usecases.dart';

// BLoCs
import '../../presentation/bloc/merchant/merchant_bloc.dart';
import '../../presentation/bloc/product/product_bloc.dart';
import '../../presentation/bloc/cart/cart_bloc.dart';
import '../../presentation/bloc/address/address_bloc.dart';
import '../../presentation/bloc/order/order_bloc.dart';
import '../../presentation/bloc/profile/profile_bloc.dart';
import '../../presentation/bloc/notification_preferences/notification_preferences_bloc.dart';
import '../../presentation/bloc/search/search_bloc.dart';
import '../../presentation/bloc/notifications_feed/notifications_feed_bloc.dart';
import '../../presentation/bloc/working_hours/working_hours_bloc.dart';
import '../../presentation/bloc/review/review_bloc.dart';

final GetIt getIt = GetIt.instance;

/// Configure all dependencies
@InjectableInit(
  initializerName: 'init',
  preferRelativeImports: true,
  asExtension: true,
)
Future<void> configureDependencies() async {
  await getIt.init(); // Injectable-generated registrations
  registerManualDependencies(); // Manual registrations
}

/// Register non-injectable dependencies manually
void registerManualDependencies() {
  // Dio is already registered via AppModule
  final dio = getIt<Dio>();
  final prefs = getIt<SharedPreferences>();

  // Register remaining datasources
  getIt.registerLazySingleton(() => MerchantDataSourceImpl(dio));
  getIt.registerLazySingleton(() => ProductDataSourceImpl(dio));
  getIt.registerLazySingleton(() => CartDataSourceImpl(dio));
  getIt.registerLazySingleton(() => AddressDataSourceImpl(dio));
  getIt.registerLazySingleton(() => OrderDataSourceImpl(dio));
  getIt.registerLazySingleton(() => ProfileDataSourceImpl(dio));
  getIt.registerLazySingleton(() => NotificationPreferencesDataSourceImpl(dio));
  getIt.registerLazySingleton(() => NotificationsFeedDataSourceImpl(dio));
  getIt.registerLazySingleton(() => WorkingHoursDataSourceImpl(dio: dio));
  getIt.registerLazySingleton(() => ReviewDataSourceImpl(dio: dio));

  // Register repositories
  getIt.registerLazySingleton(() => MerchantRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => ProductRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => CartRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => AddressRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => OrderRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => ProfileRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => NotificationRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => NotificationsFeedRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => WorkingHoursRepositoryImpl(getIt()));
  getIt.registerLazySingleton(() => ReviewRepositoryImpl(getIt()));

  // Register SearchHistoryService
  getIt.registerLazySingleton(() => SearchHistoryService(prefs));

  // Register Use Cases
  _registerUseCases();

  // Register BLoCs
  _registerBlocs();
}

void _registerUseCases() {
  // Merchant Use Cases
  getIt.registerFactory(() => GetMerchantsUseCase(getIt()));
  getIt.registerFactory(() => GetMerchantByIdUseCase(getIt()));
  getIt.registerFactory(() => SearchMerchantsUseCase(getIt()));
  getIt.registerFactory(() => GetNearbyMerchantsUseCase(getIt()));
  getIt.registerFactory(() => GetNearbyMerchantsByCategoryUseCase(getIt()));

  // Product Use Cases
  getIt.registerFactory(() => GetProductsUseCase(getIt()));
  getIt.registerFactory(() => GetProductByIdUseCase(getIt()));
  getIt.registerFactory(() => GetProductsByMerchantUseCase(getIt()));
  getIt.registerFactory(() => SearchProductsUseCase(getIt()));
  getIt.registerFactory(() => GetCategoriesUseCase(getIt()));

  // Cart Use Cases
  getIt.registerFactory(() => GetCartUseCase(getIt()));
  getIt.registerFactory(() => AddToCartUseCase(getIt()));
  getIt.registerFactory(() => UpdateCartItemUseCase(getIt()));
  getIt.registerFactory(() => RemoveFromCartUseCase(getIt()));
  getIt.registerFactory(() => ClearCartUseCase(getIt()));
  getIt.registerFactory(() => ApplyCouponUseCase(getIt()));
  getIt.registerFactory(() => RemoveCouponUseCase(getIt()));

  // Address Use Cases
  getIt.registerFactory(() => GetUserAddressesUseCase(getIt()));
  getIt.registerFactory(() => GetAddressByIdUseCase(getIt()));
  getIt.registerFactory(() => CreateAddressUseCase(getIt()));
  getIt.registerFactory(() => UpdateAddressUseCase(getIt()));
  getIt.registerFactory(() => DeleteAddressUseCase(getIt()));
  getIt.registerFactory(() => SetDefaultAddressUseCase(getIt()));

  // Order Use Cases
  getIt.registerFactory(() => GetUserOrdersUseCase(getIt()));
  getIt.registerFactory(() => GetOrderByIdUseCase(getIt()));
  getIt.registerFactory(() => CreateOrderUseCase(getIt()));
  getIt.registerFactory(() => CancelOrderUseCase(getIt()));
  getIt.registerFactory(() => ProcessPaymentUseCase(getIt()));
  getIt.registerFactory(() => GetPaymentStatusUseCase(getIt()));

  // Profile Use Cases
  getIt.registerFactory(() => GetUserProfileUseCase(getIt()));
  getIt.registerFactory(() => UpdateUserProfileUseCase(getIt()));

  // Notification Use Cases
  getIt.registerFactory(() => GetNotificationPreferencesUseCase(getIt()));
  getIt.registerFactory(() => UpdateNotificationPreferencesUseCase(getIt()));

  // Working Hours Use Cases
  getIt.registerFactory(() => GetWorkingHoursUseCase(getIt()));
  getIt.registerFactory(() => CheckIfMerchantOpenUseCase(getIt()));
  getIt.registerFactory(() => GetNextOpenTimeUseCase(getIt()));

  // Review Use Cases
  getIt.registerFactory(() => SubmitReviewUseCase(getIt()));
  getIt.registerFactory(() => GetMerchantReviewsUseCase(getIt()));
  getIt.registerFactory(() => MarkReviewAsHelpfulUseCase(getIt()));
}

void _registerBlocs() {
  // MerchantBloc
  getIt.registerFactory(
    () => MerchantBloc(
      getMerchantsUseCase: getIt(),
      getMerchantByIdUseCase: getIt(),
      searchMerchantsUseCase: getIt(),
      getNearbyMerchantsUseCase: getIt(),
      getNearbyMerchantsByCategoryUseCase: getIt(),
    ),
  );

  // ProductBloc
  getIt.registerFactory(
    () => ProductBloc(
      getProductsUseCase: getIt(),
      getProductByIdUseCase: getIt(),
      getProductsByMerchantUseCase: getIt(),
      searchProductsUseCase: getIt(),
      getCategoriesUseCase: getIt(),
    ),
  );

  // CartBloc
  getIt.registerFactory(
    () => CartBloc(
      getCartUseCase: getIt(),
      addToCartUseCase: getIt(),
      updateCartItemUseCase: getIt(),
      removeFromCartUseCase: getIt(),
      clearCartUseCase: getIt(),
      applyCouponUseCase: getIt(),
      removeCouponUseCase: getIt(),
      analytics: getIt(),
    ),
  );

  // AddressBloc
  getIt.registerFactory(
    () => AddressBloc(
      getUserAddressesUseCase: getIt(),
      getAddressByIdUseCase: getIt(),
      createAddressUseCase: getIt(),
      updateAddressUseCase: getIt(),
      deleteAddressUseCase: getIt(),
      setDefaultAddressUseCase: getIt(),
    ),
  );

  // OrderBloc
  getIt.registerFactory(
    () => OrderBloc(
      getUserOrdersUseCase: getIt(),
      getOrderByIdUseCase: getIt(),
      createOrderUseCase: getIt(),
      cancelOrderUseCase: getIt(),
      processPaymentUseCase: getIt(),
      getPaymentStatusUseCase: getIt(),
      analytics: getIt(),
    ),
  );

  // ProfileBloc
  getIt.registerFactory(
    () => ProfileBloc(
      getUserProfileUseCase: getIt(),
      updateUserProfileUseCase: getIt(),
    ),
  );

  // NotificationPreferencesBloc
  getIt.registerFactory(
    () => NotificationPreferencesBloc(
      getUseCase: getIt(),
      updateUseCase: getIt(),
    ),
  );

  // SearchBloc
  getIt.registerFactory(
    () => SearchBloc(
      searchMerchantsUseCase: getIt(),
      searchProductsUseCase: getIt(),
      searchHistoryService: getIt(),
    ),
  );

  // NotificationsFeedBloc
  getIt.registerFactory(() => NotificationsFeedBloc(repository: getIt()));

  // WorkingHoursBloc
  getIt.registerFactory(
    () => WorkingHoursBloc(
      getWorkingHoursUseCase: getIt(),
      checkIfMerchantOpenUseCase: getIt(),
      getNextOpenTimeUseCase: getIt(),
    ),
  );

  // ReviewBloc
  getIt.registerFactory(
    () => ReviewBloc(
      submitReviewUseCase: getIt(),
      getMerchantReviewsUseCase: getIt(),
      markReviewAsHelpfulUseCase: getIt(),
    ),
  );
}

/// Module for external dependencies
@module
abstract class AppModule {
  /// Provide Dio instance with interceptors
  @lazySingleton
  Dio provideDio(LocalStorageService storage, EncryptionService encryption) {
    final dio = Dio(
      BaseOptions(
        baseUrl: EnvironmentConfig.apiBaseUrl,
        connectTimeout: Duration(milliseconds: EnvironmentConfig.apiTimeout),
        receiveTimeout: Duration(milliseconds: EnvironmentConfig.apiTimeout),
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'X-API-Key': EnvironmentConfig.apiKey,
        },
      ),
    );

    // Add interceptors
    dio.interceptors.addAll([
      _AuthInterceptor(encryption),
      _LoggingInterceptor(),
      _RetryInterceptor(dio: dio),
      _ResponseAdapterInterceptor(),
    ]);

    return dio;
  }

  /// Provide SharedPreferences instance
  @preResolve
  Future<SharedPreferences> provideSharedPreferences() {
    return SharedPreferences.getInstance();
  }

  /// Provide Firebase Analytics instance
  @lazySingleton
  FirebaseAnalytics provideFirebaseAnalytics() {
    return FirebaseAnalytics.instance;
  }

  /// Provide Firebase Crashlytics instance
  @lazySingleton
  FirebaseCrashlytics provideFirebaseCrashlytics() {
    return FirebaseCrashlytics.instance;
  }

  /// Provide Firebase Performance instance
  @lazySingleton
  FirebasePerformance provideFirebasePerformance() {
    return FirebasePerformance.instance;
  }
}

class _AuthInterceptor extends Interceptor {
  final EncryptionService _encryptionService;

  _AuthInterceptor(this._encryptionService);

  @override
  void onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _encryptionService.getAccessToken();
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
    }
    super.onRequest(options, handler);
  }
}

class _LoggingInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('➡️ ${options.method} ${options.uri}');
      debugPrint('Headers: ${options.headers}');
      if (options.data != null) debugPrint('Body: ${options.data}');
    }
    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('✅ ${response.statusCode} ${response.requestOptions.uri}');
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (kDebugMode) {
      debugPrint('❌ ${err.type} ${err.requestOptions.uri}');
    }
    super.onError(err, handler);
  }
}

class _RetryInterceptor extends Interceptor {
  _RetryInterceptor({required this.dio});
  final Dio dio;
  final int maxRetries = 2;
  final Duration retryDelay = const Duration(milliseconds: 400);

  @override
  Future onError(DioException err, ErrorInterceptorHandler handler) async {
    int attempt = (err.requestOptions.extra['retry_attempt'] as int?) ?? 0;

    final shouldRetry =
        err.type == DioExceptionType.connectionError ||
        err.type == DioExceptionType.receiveTimeout ||
        err.type == DioExceptionType.sendTimeout;

    if (shouldRetry && attempt < maxRetries) {
      await Future.delayed(retryDelay * (attempt + 1));
      final req = err.requestOptions;
      req.extra['retry_attempt'] = attempt + 1;
      try {
        final response = await dio.fetch(req);
        return handler.resolve(response);
      } catch (e) {
        return handler.next(err);
      }
    }

    return handler.next(err);
  }
}

class _ResponseAdapterInterceptor extends Interceptor {
  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    final dynamic body = response.data;
    if (body is Map<String, dynamic>) {
      final normalized = <String, dynamic>{}
        ..addAll(body)
        ..putIfAbsent('statusCode', () => response.statusCode);
      response.data = normalized['data'] ?? normalized;
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    handler.next(err);
  }
}
