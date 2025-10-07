// GENERATED CODE - DO NOT MODIFY BY HAND

// **************************************************************************
// InjectableConfigGenerator
// **************************************************************************

// ignore_for_file: type=lint
// coverage:ignore-file

// ignore_for_file: no_leading_underscores_for_library_prefixes
import 'package:dio/dio.dart' as _i361;
import 'package:get_it/get_it.dart' as _i174;
import 'package:injectable/injectable.dart' as _i526;
import 'package:shared_preferences/shared_preferences.dart' as _i460;

import '../../data/datasources/auth_datasource.dart' as _i51;
import '../../data/datasources/auth_datasource_impl.dart' as _i974;
import '../../data/repositories/auth_repository_impl.dart' as _i895;
import '../../domain/repositories/auth_repository.dart' as _i1073;
import '../../domain/usecases/auth_usecases.dart' as _i766;
import '../../presentation/bloc/auth/auth_bloc.dart' as _i605;
import '../services/encryption_service.dart' as _i180;
import '../services/local_storage_service.dart' as _i527;
import '../services/signalr_service.dart' as _i320;
import 'injection.dart' as _i464;

extension GetItInjectableX on _i174.GetIt {
  // initializes the registration of main-scope dependencies inside of GetIt
  Future<_i174.GetIt> init({
    String? environment,
    _i526.EnvironmentFilter? environmentFilter,
  }) async {
    final gh = _i526.GetItHelper(this, environment, environmentFilter);
    final appModule = _$AppModule();
    await gh.factoryAsync<_i460.SharedPreferences>(
      () => appModule.provideSharedPreferences(),
      preResolve: true,
    );
    gh.lazySingleton<_i180.EncryptionService>(() => _i180.EncryptionService());
    gh.lazySingleton<_i527.LocalStorageService>(
      () => _i527.LocalStorageService(),
    );
    gh.lazySingleton<_i320.SignalRService>(
      () => _i320.SignalRService(gh<_i180.EncryptionService>()),
    );
    gh.lazySingleton<_i361.Dio>(
      () => appModule.provideDio(
        gh<_i527.LocalStorageService>(),
        gh<_i180.EncryptionService>(),
      ),
    );
    gh.lazySingleton<_i51.AuthDataSource>(
      () => _i974.AuthDataSourceImpl(
        gh<_i361.Dio>(),
        gh<_i460.SharedPreferences>(),
      ),
    );
    gh.lazySingleton<_i1073.AuthRepository>(
      () => _i895.AuthRepositoryImpl(gh<_i51.AuthDataSource>()),
    );
    gh.factory<_i766.LoginUseCase>(
      () => _i766.LoginUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.RegisterUseCase>(
      () => _i766.RegisterUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.LogoutUseCase>(
      () => _i766.LogoutUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.RefreshTokenUseCase>(
      () => _i766.RefreshTokenUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.ForgotPasswordUseCase>(
      () => _i766.ForgotPasswordUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.ResetPasswordUseCase>(
      () => _i766.ResetPasswordUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.GetCurrentUserUseCase>(
      () => _i766.GetCurrentUserUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.CheckAuthenticationUseCase>(
      () => _i766.CheckAuthenticationUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i766.CheckTokenValidityUseCase>(
      () => _i766.CheckTokenValidityUseCase(gh<_i1073.AuthRepository>()),
    );
    gh.factory<_i605.AuthBloc>(
      () => _i605.AuthBloc(
        gh<_i766.LoginUseCase>(),
        gh<_i766.RegisterUseCase>(),
        gh<_i766.LogoutUseCase>(),
        gh<_i766.RefreshTokenUseCase>(),
        gh<_i766.ForgotPasswordUseCase>(),
        gh<_i766.ResetPasswordUseCase>(),
        gh<_i766.GetCurrentUserUseCase>(),
        gh<_i766.CheckAuthenticationUseCase>(),
        gh<_i766.CheckTokenValidityUseCase>(),
      ),
    );
    return this;
  }
}

class _$AppModule extends _i464.AppModule {}
