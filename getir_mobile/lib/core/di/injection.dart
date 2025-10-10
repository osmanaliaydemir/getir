import 'package:get_it/get_it.dart';
import 'package:dio/dio.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:flutter/foundation.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_performance/firebase_performance.dart';
import '../services/local_storage_service.dart';
import '../services/encryption_service.dart';
import '../services/search_history_service.dart';
import '../services/analytics_service.dart';
import '../services/logger_service.dart';
import '../services/signalr_service.dart';
import '../services/network_service.dart';
import '../config/environment_config.dart';

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

final GetIt getIt = GetIt.instance;

/// Configure all dependencies
Future<void> configureDependencies() async {
  // Register external dependencies first
  final prefs = await SharedPreferences.getInstance();
  getIt.registerSingleton<SharedPreferences>(prefs);

  // Register core services
  getIt.registerLazySingleton(() => EncryptionService());
  getIt.registerLazySingleton(() => LocalStorageService());

  // Register Firebase services
  getIt.registerLazySingleton(() => FirebaseAnalytics.instance);
  getIt.registerLazySingleton(() => FirebaseCrashlytics.instance);
  getIt.registerLazySingleton(() => FirebasePerformance.instance);

  // Register Analytics service
  getIt.registerLazySingleton(
    () => AnalyticsService(
      getIt<FirebaseAnalytics>(),
      getIt<FirebaseCrashlytics>(),
      getIt<FirebasePerformance>(),
    ),
  );

  // Register Logger service
  getIt.registerLazySingleton(() => LoggerService(getIt<AnalyticsService>()));

  // Register SignalR service (Singleton for shared hub connections)
  getIt.registerLazySingleton(() => SignalRService(getIt<EncryptionService>()));

  // Register Network service (needed before NetworkCubit)
  final networkService = NetworkService();
  getIt.registerSingleton<NetworkService>(networkService);

  // Register Dio
  final dio = _createDio(getIt<EncryptionService>());
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

  // Register all datasources
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
}

void _registerRepositories() {
  getIt.registerLazySingleton(() => AuthRepositoryImpl(getIt()));
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
    () => NotificationPreferencesBloc(getIt<NotificationService>()),
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
  getIt.registerFactory(() => NotificationsFeedBloc(repository: getIt()));

  // ReviewBloc
  getIt.registerFactory(() => ReviewBloc(getIt<ReviewService>()));

  // WorkingHoursBloc
  getIt.registerFactory(() => WorkingHoursBloc(getIt<WorkingHoursService>()));
}

/// Create Dio instance with interceptors
Dio _createDio(EncryptionService encryption) {
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
