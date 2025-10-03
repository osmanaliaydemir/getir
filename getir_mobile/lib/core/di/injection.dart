import 'package:get_it/get_it.dart';
import 'package:injectable/injectable.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../data/repositories/auth_repository_impl.dart';
import '../../data/datasources/auth_datasource.dart';
import '../../data/datasources/auth_datasource_impl.dart';

final GetIt getIt = GetIt.instance;

@InjectableInit()
Future<void> configureDependencies() async {
  // Register repositories
  getIt.registerLazySingleton<AuthRepository>(
    () => AuthRepositoryImpl(getIt<AuthDataSource>()),
  );
  
  // Register data sources
  getIt.registerLazySingleton<AuthDataSource>(
    () => AuthDataSourceImpl(),
  );
}
