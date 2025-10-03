import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/navigation/app_router.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../bloc/auth/auth_bloc.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> {
  @override
  void initState() {
    super.initState();
    _initializeApp();
  }

  void _initializeApp() async {
    // Simulate app initialization
    await Future.delayed(const Duration(seconds: 2));
    
    if (mounted) {
      // Check authentication status
      context.read<AuthBloc>().add(AuthCheckAuthenticationRequested());
    }
  }

  @override
  Widget build(BuildContext context) {
    return BlocListener<AuthBloc, AuthState>(
      listener: (context, state) {
        if (state is AuthAuthenticated) {
          // User is authenticated, go to home
          AppNavigation.goToHome(context);
        } else if (state is AuthUnauthenticated) {
          // User is not authenticated, go to onboarding
          AppNavigation.goToOnboarding(context);
        } else if (state is AuthError) {
          // Handle error, go to onboarding
          AppNavigation.goToOnboarding(context);
        }
      },
      child: Scaffold(
        backgroundColor: AppColors.primary,
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // App Logo
              Container(
                width: 120,
                height: 120,
                decoration: BoxDecoration(
                  color: AppColors.white,
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: [
                    BoxShadow(
                      color: AppColors.shadowMedium,
                      blurRadius: 10,
                      offset: const Offset(0, 5),
                    ),
                  ],
                ),
                child: const Icon(
                  Icons.local_grocery_store,
                  size: 60,
                  color: AppColors.primary,
                ),
              ),
              const SizedBox(height: 32),
              
              // App Name
              Text(
                'Getir Mobile',
                style: AppTypography.displayMedium.copyWith(
                  color: AppColors.white,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              
              // App Description
              Text(
                'Hızlı Teslimat, Kolay Alışveriş',
                style: AppTypography.bodyLarge.copyWith(
                  color: AppColors.white.withValues(alpha: 0.8),
                ),
              ),
              const SizedBox(height: 48),
              
              // Loading Indicator
              const CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.white),
                strokeWidth: 3,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
