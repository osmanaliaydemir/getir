import 'package:get_it/get_it.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_performance/firebase_performance.dart';
import '../services/local_storage_service.dart';
import '../services/secure_encryption_service.dart'; // ✅ Upgraded to AES-256
import '../services/search_history_service.dart';
import '../services/analytics_service.dart';
import '../services/logger_service.dart';
import '../services/signalr_service.dart';
import '../services/network_service.dart';
import '../config/environment_config.dart';
import '../interceptors/token_refresh_interceptor.dart';

// DataSources (interface + implementation in same files)
import '../../data/datasources/auth_datasource.dart';
import '../../data/datasources/auth_datasource_impl.dart';
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
import '../../data/datasources/favorites_datasource.dart';
import '../../data/datasources/orders_datasource.dart';
import '../../data/datasources/language_datasource.dart';
// Repository Interfaces (Domain) - Using I prefix
import '../../domain/repositories/auth_repository.dart';
import '../../domain/repositories/merchant_repository.dart';
import '../../domain/repositories/product_repository.dart';
import '../../domain/repositories/cart_repository.dart';
import '../../domain/repositories/address_repository.dart';
import '../../domain/repositories/order_repository.dart';
import '../../domain/repositories/profile_repository.dart';
import '../../domain/repositories/notification_repository.dart';
import '../../domain/repositories/notifications_feed_repository.dart';
import '../../domain/repositories/working_hours_repository.dart';
import '../../domain/repositories/review_repository.dart';
import '../../domain/repositories/favorites_repository.dart';
import '../../domain/repositories/orders_repository.dart';
import '../../domain/repositories/language_repository.dart';
import '../../domain/repositories/notification_preferences_repository.dart';

// Repository Implementations (Data)
import '../../data/repositories/auth_repository_impl.dart';
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
import '../../data/repositories/favorites_repository_impl.dart';
import '../../data/repositories/orders_repository_impl.dart';
import '../../data/repositories/language_repository_impl.dart';
import '../../data/repositories/notification_preferences_repository_impl.dart';

// Services (replaces 49 Use Case classes)
import '../../domain/services/auth_service.dart';
import '../../domain/services/merchant_service.dart';
import '../../domain/services/product_service.dart';
import '../../domain/services/cart_service.dart';
import '../../domain/services/address_service.dart';
import '../../domain/services/order_service.dart';
import '../../domain/services/profile_service.dart';
import '../../domain/services/notification_service.dart';
import '../../domain/services/working_hours_service.dart';
import '../../domain/services/review_service.dart';
import '../../domain/services/favorites_service.dart';
import '../../domain/services/orders_service.dart';
import '../../domain/services/language_service.dart';
import '../../domain/services/notification_preferences_service.dart';
import '../../domain/services/notifications_feed_service.dart';

// Cubits (Global State)
import '../cubits/network/network_cubit.dart';
import '../cubits/language/language_cubit.dart';
import '../cubits/theme/theme_cubit.dart';
import '../cubits/notification_badge/notification_badge_cubit.dart';

// BLoCs
import '../../presentation/bloc/auth/auth_bloc.dart';
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
import '../../presentation/bloc/favorites/favorites_bloc.dart';
import '../../presentation/bloc/orders/orders_bloc.dart';
import '../../presentation/bloc/language/language_bloc.dart' as lang_bloc;

final GetIt getIt = GetIt.instance;

/// Configure all dependencies
Future<void> configureDependencies() async {
  // Register external dependencies first
  final prefs = await SharedPreferences.getInstance();
  getIt.registerSingleton<SharedPreferences>(prefs);

  // Register core services
  getIt.registerLazySingleton(() => SecureEncryptionService()); // ✅ AES-256-GCM
  getIt.registerLazySingleton(() => LocalStorageService());

  // Register Firebase services (must be before Analytics)
  // ⚠️ TEMPORARY: Check if Firebase is initialized before registering services
  bool firebaseInitialized = false;
  try {
    // Try to access Firebase - this will throw if not initialized
    Firebase.app();
    firebaseInitialized = true;
    debugPrint('✅ [DI] Firebase detected, registering Firebase services');
  } catch (e) {
    debugPrint('⚠️ [DI] Firebase not initialized, using no-op services');
  }

  if (firebaseInitialized) {
    getIt.registerLazySingleton(() => FirebaseAnalytics.instance);
    getIt.registerLazySingleton(() => FirebaseCrashlytics.instance);
    getIt.registerLazySingleton(() => FirebasePerformance.instance);

    // Register Analytics service (must be before Logger)
    getIt.registerLazySingleton(
      () => AnalyticsService(
        getIt<FirebaseAnalytics>(),
        getIt<FirebaseCrashlytics>(),
        getIt<FirebasePerformance>(),
      ),
    );
  } else {
    // Register no-op Analytics service
    getIt.registerLazySingleton(
      () => _NoOpAnalyticsService() as AnalyticsService,
    );
  }

  // Register Logger service (CRITICAL: Must be before all other services!)
  getIt.registerLazySingleton<LoggerService>(
    () => LoggerService(getIt<AnalyticsService>()),
  );

  // Register SignalR service (Singleton for shared hub connections)
  getIt.registerLazySingleton(
    () => SignalRService(getIt<SecureEncryptionService>()),
  );

  // Register Network service (needed before NetworkCubit)
  final networkService = NetworkService();
  getIt.registerSingleton<NetworkService>(networkService);

  // Register Dio
  final dio = _createDio(getIt<SecureEncryptionService>());
  getIt.registerSingleton<Dio>(dio);

  // Register all datasources, repositories, use cases, BLoCs, and Cubits
  _registerDatasources();
  _registerRepositories();
  _registerServices();
  _registerCubits(prefs, networkService);
  _registerBlocs();
  _registerOtherServices(prefs);
}

void _registerDatasources() {
  final dio = getIt<Dio>();

  // Register all datasources as interfaces (using concrete classes temporarily)
  getIt.registerLazySingleton<AuthDataSource>(
    () => AuthDataSourceImpl(dio, getIt(), getIt<SecureEncryptionService>()),
  );
  getIt.registerLazySingleton<MerchantDataSource>(
    () => MerchantDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<ProductDataSource>(
    () => ProductDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<CartDataSource>(() => CartDataSourceImpl(dio));
  getIt.registerLazySingleton<IAddressDataSource>(
    () => AddressDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<IOrderDataSource>(() => OrderDataSourceImpl(dio));
  getIt.registerLazySingleton<ProfileDataSource>(
    () => ProfileDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<NotificationPreferencesDataSource>(
    () => NotificationPreferencesDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<NotificationsFeedDataSource>(
    () => NotificationsFeedDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<WorkingHoursDataSource>(
    () => WorkingHoursDataSourceImpl(dio: dio),
  );
  getIt.registerLazySingleton<ReviewDataSource>(
    () => ReviewDataSourceImpl(dio: dio),
  );
  getIt.registerLazySingleton<FavoritesDataSource>(
    () => FavoritesDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<OrdersDataSource>(
    () => OrdersDataSourceImpl(dio),
  );
  getIt.registerLazySingleton<LanguageDataSource>(
    () => LanguageDataSourceImpl(dio),
  );
}

void _registerRepositories() {
  // Register repositories as interfaces (for proper DI)
  getIt.registerLazySingleton<IAuthRepository>(
    () => AuthRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IMerchantRepository>(
    () => MerchantRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IProductRepository>(
    () => ProductRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<ICartRepository>(
    () => CartRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IAddressRepository>(
    () => AddressRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IOrderRepository>(
    () => OrderRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IProfileRepository>(
    () => ProfileRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<INotificationRepository>(
    () => NotificationRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<INotificationsFeedRepository>(
    () => NotificationsFeedRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IWorkingHoursRepository>(
    () => WorkingHoursRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IReviewRepository>(
    () => ReviewRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IFavoritesRepository>(
    () => FavoritesRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<IOrdersRepository>(
    () => OrdersRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<ILanguageRepository>(
    () => LanguageRepositoryImpl(getIt()),
  );
  getIt.registerLazySingleton<INotificationPreferencesRepository>(
    () => NotificationPreferencesRepositoryImpl(getIt()),
  );
}

void _registerOtherServices(SharedPreferences prefs) {
  getIt.registerLazySingleton(() => SearchHistoryService(prefs));
}

/// Registers domain services (replaces 49 UseCase registrations!)
void _registerServices() {
  getIt.registerFactory(() => AuthService(getIt()));
  getIt.registerFactory(() => MerchantService(getIt()));
  getIt.registerFactory(() => ProductService(getIt()));
  getIt.registerFactory(() => CartService(getIt()));
  getIt.registerFactory(() => AddressService(getIt()));
  getIt.registerFactory(() => OrderService(getIt()));
  getIt.registerFactory(() => ProfileService(getIt()));
  getIt.registerFactory(() => NotificationService(getIt()));
  getIt.registerFactory(() => WorkingHoursService(getIt()));
  getIt.registerFactory(() => ReviewService(getIt()));
  getIt.registerFactory(() => FavoritesService(getIt()));
  getIt.registerFactory(() => OrdersService(getIt()));
  getIt.registerFactory(() => LanguageService(getIt()));
  getIt.registerFactory(() => NotificationPreferencesService(getIt()));
  getIt.registerFactory(() => NotificationsFeedService(getIt()));
}

void _registerCubits(SharedPreferences prefs, NetworkService networkService) {
  // Global state management Cubits (replaces Providers)
  getIt.registerLazySingleton(() => NetworkCubit(networkService));
  getIt.registerLazySingleton(() => LanguageCubit(prefs));
  getIt.registerLazySingleton(() => ThemeCubit(prefs));
  getIt.registerLazySingleton(() => NotificationBadgeCubit());
}

void _registerBlocs() {
  // AuthBloc (2 dependencies instead of 10!)
  getIt.registerFactory(
    () => AuthBloc(getIt<AuthService>(), getIt<AnalyticsService>()),
  );

  // MerchantBloc
  getIt.registerFactory(() => MerchantBloc(getIt<MerchantService>()));

  // ProductBloc
  getIt.registerFactory(() => ProductBloc(getIt<ProductService>()));

  // CartBloc
  getIt.registerFactory(
    () => CartBloc(getIt<CartService>(), getIt<AnalyticsService>()),
  );

  // AddressBloc
  getIt.registerFactory(() => AddressBloc(getIt<AddressService>()));

  // OrderBloc
  getIt.registerFactory(
    () => OrderBloc(getIt<OrderService>(), getIt<AnalyticsService>()),
  );

  // ProfileBloc
  getIt.registerFactory(() => ProfileBloc(getIt<ProfileService>()));

  // NotificationPreferencesBloc
  getIt.registerFactory(
    () => NotificationPreferencesBloc(getIt<NotificationPreferencesService>()),
  );

  // SearchBloc
  getIt.registerFactory(
    () => SearchBloc(
      getIt<MerchantService>(),
      getIt<ProductService>(),
      getIt<SearchHistoryService>(),
    ),
  );

  // NotificationsFeedBloc
  getIt.registerFactory(() => NotificationsFeedBloc(service: getIt()));

  // ReviewBloc
  getIt.registerFactory(() => ReviewBloc(getIt<ReviewService>()));

  // WorkingHoursBloc
  getIt.registerFactory(() => WorkingHoursBloc(getIt<WorkingHoursService>()));

  // FavoritesBloc
  getIt.registerFactory(() => FavoritesBloc(getIt<FavoritesService>()));

  // OrdersBloc
  getIt.registerFactory(() => OrdersBloc(getIt<OrdersService>()));

  // LanguageBloc
  getIt.registerFactory(() => lang_bloc.LanguageBloc(getIt<LanguageService>()));
}

/// Create Dio instance with interceptors
Dio _createDio(SecureEncryptionService encryption) {
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

  // Add interceptors (order matters!)
  dio.interceptors.addAll([
    _AuthInterceptor(encryption),
    TokenRefreshInterceptor(dio, encryption), // ✅ Auto token refresh on 401
    _LoggingInterceptor(),
    _RetryInterceptor(dio: dio),
    _ResponseAdapterInterceptor(),
  ]);

  return dio;
}

class _AuthInterceptor extends Interceptor {
  final SecureEncryptionService _encryptionService;

  _AuthInterceptor(this._encryptionService);

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    final token = await _encryptionService.getAccessToken();
    debugPrint(
      '🔑 [AuthInterceptor] Token: ${token != null && token.isNotEmpty ? "${token.substring(0, token.length > 20 ? 20 : token.length)}..." : "NULL or EMPTY (length: ${token?.length ?? 0})"}',
    );
    if (token != null && token.isNotEmpty) {
      options.headers['Authorization'] = 'Bearer $token';
      debugPrint('✅ [AuthInterceptor] Authorization header added');
    } else {
      debugPrint(
        '⚠️ [AuthInterceptor] No token found, request will be sent without auth',
      );
    }
    handler.next(options);
  }
}

class _LoggingInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    if (kDebugMode) {
      logger.debug('${options.method} ${options.uri}', tag: 'HTTP');
      logger.debug('Headers: ${options.headers}', tag: 'HTTP');
      if (options.data != null) {
        logger.debug('Body: ${options.data}', tag: 'HTTP');
      }
    }
    super.onRequest(options, handler);
  }

  @override
  void onResponse(Response response, ResponseInterceptorHandler handler) {
    if (kDebugMode) {
      logger.debug(
        '${response.statusCode} ${response.requestOptions.uri}',
        tag: 'HTTP',
      );
    }
    super.onResponse(response, handler);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    if (kDebugMode) {
      logger.error(
        '${err.type} ${err.requestOptions.uri}',
        tag: 'HTTP',
        error: err,
      );
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

/// No-op AnalyticsService when Firebase is not available
/// This allows the app to run without Firebase for development
class _NoOpAnalyticsService implements AnalyticsService {
  @override
  FirebaseAnalyticsObserver get observer {
    throw UnimplementedError(
      'FirebaseAnalyticsObserver not available without Firebase',
    );
  }

  @override
  Future<void> logScreenView({
    required String screenName,
    String? screenClass,
    Map<String, dynamic>? parameters,
  }) async {}

  @override
  Future<void> logButtonClick({
    required String buttonName,
    String? screenName,
    Map<String, dynamic>? parameters,
  }) async {}

  @override
  Future<void> logSearch({
    required String searchTerm,
    String? searchType,
    int? resultCount,
  }) async {}

  @override
  Future<void> logProductView({
    required String productId,
    required String productName,
    String? category,
    double? price,
    String? currency,
  }) async {}

  @override
  Future<void> logAddToCart({
    required String productId,
    required String productName,
    required double price,
    String? category,
    int quantity = 1,
    String? currency,
  }) async {}

  @override
  Future<void> logRemoveFromCart({
    required String productId,
    required String productName,
    required double price,
    int quantity = 1,
  }) async {}

  @override
  Future<void> logAddToFavorites({
    required String itemId,
    required String itemName,
    String? itemType,
  }) async {}

  @override
  Future<void> logBeginCheckout({
    required double value,
    required String currency,
    List<AnalyticsEventItem>? items,
    String? coupon,
  }) async {}

  @override
  Future<void> logAddPaymentInfo({
    required String paymentType,
    required double value,
    String? currency,
  }) async {}

  @override
  Future<void> logPurchase({
    required String orderId,
    required double total,
    required String currency,
    List<AnalyticsEventItem>? items,
    double? tax,
    double? shipping,
    String? coupon,
  }) async {}

  @override
  Future<void> logOrderCancelled({
    required String orderId,
    required double value,
    String? reason,
  }) async {}

  @override
  Future<void> logLogin({String? method}) async {}

  @override
  Future<void> logSignUp({String? method}) async {}

  @override
  Future<void> logLogout() async {}

  @override
  Future<void> logError({
    required dynamic error,
    StackTrace? stackTrace,
    String? reason,
    Map<String, dynamic>? context,
    bool fatal = false,
  }) async {
    debugPrint('❌ [Analytics-NoOp] Error: $error');
  }

  @override
  Future<void> setErrorContext({
    String? userId,
    String? screenName,
    Map<String, dynamic>? customKeys,
  }) async {}

  @override
  Future<Trace> startTrace(String traceName) async {
    return _NoOpTrace();
  }

  @override
  Future<void> stopTrace(Trace trace) async {}

  @override
  Future<T> measurePerformance<T>({
    required String traceName,
    required Future<T> Function() operation,
    Map<String, String>? attributes,
  }) async {
    return await operation();
  }

  @override
  Future<void> setUserId(String? userId) async {}

  @override
  Future<void> setUserProperty({
    required String name,
    required String? value,
  }) async {}

  @override
  Future<void> setUserDemographics({
    int? age,
    String? gender,
    String? interests,
  }) async {}

  @override
  Future<void> logCustomEvent({
    required String eventName,
    Map<String, dynamic>? parameters,
  }) async {}

  @override
  Future<void> setAnalyticsEnabled(bool enabled) async {}

  @override
  Future<void> setCrashlyticsEnabled(bool enabled) async {}

  @override
  Future<void> resetAnalyticsData() async {}
}

/// No-op Trace implementation
class _NoOpTrace implements Trace {
  @override
  Future<void> start() async {}

  @override
  Future<void> stop() async {}

  @override
  void putAttribute(String name, String value) {}

  @override
  void putMetric(String name, int value) {}

  @override
  void setMetric(String name, int value) {}

  @override
  void incrementMetric(String name, int value) {}

  @override
  int getMetric(String name) => 0;

  @override
  String getAttribute(String name) => '';

  @override
  Map<String, String> getAttributes() => {};

  @override
  void removeAttribute(String name) {}
}
