import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'core/navigation/app_router.dart';
import 'core/theme/app_theme.dart';
import 'presentation/bloc/auth/auth_bloc.dart';
import 'domain/usecases/auth_usecases.dart';
import 'data/repositories/auth_repository_impl.dart';
import 'data/datasources/auth_datasource_impl.dart';

void main() {
  runApp(const GetirApp());
}

class GetirApp extends StatelessWidget {
  const GetirApp({super.key});

  @override
  Widget build(BuildContext context) {
    // Create repository instance
    final authRepository = AuthRepositoryImpl(AuthDataSourceImpl());
    
    return MultiBlocProvider(
      providers: [
        BlocProvider<AuthBloc>(
          create: (context) => AuthBloc(
            loginUseCase: LoginUseCase(authRepository),
            registerUseCase: RegisterUseCase(authRepository),
            logoutUseCase: LogoutUseCase(authRepository),
            refreshTokenUseCase: RefreshTokenUseCase(authRepository),
            forgotPasswordUseCase: ForgotPasswordUseCase(authRepository),
            resetPasswordUseCase: ResetPasswordUseCase(authRepository),
            getCurrentUserUseCase: GetCurrentUserUseCase(authRepository),
            checkAuthenticationUseCase: CheckAuthenticationUseCase(authRepository),
            checkTokenValidityUseCase: CheckTokenValidityUseCase(authRepository),
          ),
        ),
      ],
      child: MaterialApp.router(
        title: 'Getir Mobile',
        debugShowCheckedModeBanner: false,
        theme: AppTheme.lightTheme,
        darkTheme: AppTheme.darkTheme,
        themeMode: ThemeMode.system,
        routerConfig: AppRouter.router,
      ),
    );
  }
}