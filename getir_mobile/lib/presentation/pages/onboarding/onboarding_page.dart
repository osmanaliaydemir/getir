import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/route_constants.dart';
import '../../../core/services/local_storage_service.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';

class OnboardingPage extends StatefulWidget {
  const OnboardingPage({super.key});

  @override
  State<OnboardingPage> createState() => _OnboardingPageState();
}

class _OnboardingPageState extends State<OnboardingPage> {
  final PageController _pageController = PageController();
  int _currentPage = 0;

  final List<OnboardingItem> _onboardingItems = [
    OnboardingItem(
      title: 'Hızlı Teslimat',
      description:
          'İhtiyacın olan ürünleri dakikalar içinde kapına getiriyoruz.',
      icon: Icons.local_shipping,
      color: AppColors.primary,
    ),
    OnboardingItem(
      title: 'Geniş Ürün Yelpazesi',
      description:
          'Market, yemek, su ve eczane ürünlerini tek yerden sipariş et.',
      icon: Icons.store,
      color: AppColors.secondary,
    ),
    OnboardingItem(
      title: 'Güvenli Ödeme',
      description: 'Nakit ödeme ile güvenli ve kolay alışveriş yap.',
      icon: Icons.payment,
      color: AppColors.accent,
    ),
  ];

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  void _nextPage() {
    debugPrint(
      '🔍 [Onboarding] Next button tapped, current page: $_currentPage',
    );
    if (_currentPage < _onboardingItems.length - 1) {
      _pageController.nextPage(
        duration: const Duration(milliseconds: 300),
        curve: Curves.easeInOut,
      );
    } else {
      debugPrint('🔍 [Onboarding] Last page, navigating to login');
      _goToLogin();
    }
  }

  void _skipOnboarding() {
    debugPrint('🔍 [Onboarding] Skip button tapped');
    _goToLogin();
  }

  void _goToLogin() async {
    debugPrint('🔍 [Onboarding] Navigating to login page...');
    try {
      // Mark onboarding as seen
      final storage = LocalStorageService();
      await storage.storeUserData('has_seen_onboarding', 'true');
      debugPrint('✅ [Onboarding] Onboarding marked as seen');

      // Navigate to login using GoRouter
      if (mounted) {
        context.go(RouteConstants.login);
        debugPrint('✅ [Onboarding] Navigation to login successful');
      }
    } catch (e, stackTrace) {
      debugPrint('❌ [Onboarding] Navigation failed: $e');
      debugPrint('StackTrace: $stackTrace');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // Skip Button
            Align(
              alignment: Alignment.topRight,
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: TextButton(
                  onPressed: _skipOnboarding,
                  child: Text(
                    'Atla',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ),
              ),
            ),

            // PageView
            Expanded(
              child: PageView.builder(
                controller: _pageController,
                onPageChanged: (index) {
                  setState(() {
                    _currentPage = index;
                  });
                },
                itemCount: _onboardingItems.length,
                itemBuilder: (context, index) {
                  final item = _onboardingItems[index];
                  return _buildOnboardingPage(item);
                },
              ),
            ),

            // Page Indicators
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: List.generate(
                _onboardingItems.length,
                (index) => Container(
                  margin: const EdgeInsets.symmetric(horizontal: 4),
                  width: _currentPage == index ? 24 : 8,
                  height: 8,
                  decoration: BoxDecoration(
                    color: _currentPage == index
                        ? AppColors.primary
                        : AppColors.greyLight,
                    borderRadius: BorderRadius.circular(4),
                  ),
                ),
              ),
            ),
            const SizedBox(height: 32),

            // Next/Get Started Button
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 24),
              child: SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: _nextPage,
                  child: Text(
                    _currentPage == _onboardingItems.length - 1
                        ? 'Başlayalım'
                        : 'Devam Et',
                    style: AppTypography.buttonText,
                  ),
                ),
              ),
            ),
            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  Widget _buildOnboardingPage(OnboardingItem item) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 32),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          // Icon
          Container(
            width: 120,
            height: 120,
            decoration: BoxDecoration(
              color: item.color.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(60),
            ),
            child: Icon(item.icon, size: 60, color: item.color),
          ),
          const SizedBox(height: 48),

          // Title
          Text(
            item.title,
            style: AppTypography.headlineLarge.copyWith(
              color: AppColors.textPrimary,
              fontWeight: FontWeight.bold,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 16),

          // Description
          Text(
            item.description,
            style: AppTypography.bodyLarge.copyWith(
              color: AppColors.textSecondary,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

class OnboardingItem {
  final String title;
  final String description;
  final IconData icon;
  final Color color;

  const OnboardingItem({
    required this.title,
    required this.description,
    required this.icon,
    required this.color,
  });
}
