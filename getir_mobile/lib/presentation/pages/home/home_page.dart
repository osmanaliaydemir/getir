import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:geolocator/geolocator.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../core/constants/app_dimensions.dart';
import '../../../core/cubits/language/language_cubit.dart';
import '../../../domain/entities/service_category_type.dart';
import '../../widgets/common/language_selector.dart';
import '../../widgets/merchant/merchant_card.dart';
import '../../widgets/merchant/merchant_card_skeleton.dart';
import '../../../core/widgets/error_state_widget.dart';
import '../../bloc/merchant/merchant_bloc.dart';
import '../merchant/category_merchants_page.dart';
import '../../../core/services/logger_service.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  Position? _currentPosition;

  @override
  void initState() {
    super.initState();
    _getCurrentLocation();
  }

  Future<void> _getCurrentLocation() async {
    try {
      LocationPermission permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          return;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        return;
      }

      Position position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );

      setState(() {
        _currentPosition = position;
      });

      // Load nearby merchants
      context.read<MerchantBloc>().add(
        LoadNearbyMerchants(
          latitude: position.latitude,
          longitude: position.longitude,
          radius: AppDimensions.nearbyMerchantRadiusKm,
        ),
      );
    } catch (e, stackTrace) {
      // Log error
      logger.error(
        'Failed to get location',
        tag: 'HomePage',
        error: e,
        stackTrace: stackTrace,
      );

      // Show user-friendly error message
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text(
              'Konum alınamadı. Lütfen konum izinlerini kontrol edin.',
            ),
            duration: Duration(seconds: 3),
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.home),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
        actions: [
          BlocBuilder<LanguageCubit, LanguageState>(
            builder: (context, state) {
              return LanguageSelector(
                currentLanguage: state.languageCode,
                onLanguageChanged: (languageCode) {
                  context.read<LanguageCubit>().changeLanguageByCode(
                    languageCode,
                  );
                },
              );
            },
          ),
          const SizedBox(width: 8),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(AppDimensions.spacingL),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Location Header
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(AppDimensions.spacingL),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(
                  AppDimensions.cardBorderRadius,
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.grey.withOpacity(AppDimensions.shadowOpacity),
                    spreadRadius: AppDimensions.shadowSpreadRadius,
                    blurRadius: AppDimensions.shadowBlurRadius,
                    offset: const Offset(0, AppDimensions.shadowOffsetY),
                  ),
                ],
              ),
              child: Row(
                children: [
                  const Icon(
                    Icons.location_on,
                    color: AppColors.primary,
                    size: AppDimensions.iconM,
                  ),
                  const SizedBox(width: AppDimensions.spacingM),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          l10n.selectLocation,
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Kadıköy, İstanbul',
                          style: AppTypography.bodyLarge.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const Icon(
                    Icons.keyboard_arrow_down,
                    color: AppColors.textSecondary,
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),

            // Search Bar
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.grey.withOpacity(0.1),
                    spreadRadius: 1,
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: TextField(
                decoration: InputDecoration(
                  hintText: l10n.searchHint,
                  border: InputBorder.none,
                  prefixIcon: const Icon(
                    Icons.search,
                    color: AppColors.textSecondary,
                  ),
                ),
              ),
            ),

            const SizedBox(height: 24),

            // Categories Section
            Text(
              l10n.categories,
              style: AppTypography.headlineSmall.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),

            // Categories Grid
            GridView.count(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              crossAxisCount: AppDimensions.categoriesGridCount,
              crossAxisSpacing: AppDimensions.spacingL,
              mainAxisSpacing: AppDimensions.spacingL,
              children: [
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.market,
                  icon: Icons.local_grocery_store,
                  label: 'Market',
                  color: Colors.orange,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.restaurant,
                  icon: Icons.restaurant,
                  label: 'Restoran',
                  color: Colors.red,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.pharmacy,
                  icon: Icons.local_pharmacy,
                  label: 'Eczane',
                  color: Colors.green,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.water,
                  icon: Icons.water_drop,
                  label: 'Su',
                  color: Colors.blue,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.cafe,
                  icon: Icons.local_cafe,
                  label: 'Kafe',
                  color: Colors.brown,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.bakery,
                  icon: Icons.bakery_dining,
                  label: 'Pastane',
                  color: Colors.pink,
                ),
                _buildCategoryItem(
                  context: context,
                  categoryType: ServiceCategoryType.other,
                  icon: Icons.more_horiz,
                  label: 'Diğer',
                  color: Colors.grey,
                ),
              ],
            ),

            const SizedBox(height: 32),

            // Popular Products Section
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  l10n.popularProducts,
                  style: AppTypography.headlineSmall.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
                ),
                TextButton(
                  onPressed: () {},
                  child: Text(
                    l10n.viewAll,
                    style: const TextStyle(
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Popular Products List
            SizedBox(
              height: AppDimensions.popularProductsHeight,
              child: ListView.builder(
                scrollDirection: Axis.horizontal,
                itemCount: 5,
                itemBuilder: (context, index) {
                  return Container(
                    width: AppDimensions.productCardWidth,
                    margin: const EdgeInsets.only(
                      right: AppDimensions.spacingL,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(
                        AppDimensions.cardBorderRadius,
                      ),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.grey.withOpacity(
                            AppDimensions.shadowOpacity,
                          ),
                          spreadRadius: AppDimensions.shadowSpreadRadius,
                          blurRadius: AppDimensions.shadowBlurRadius,
                          offset: const Offset(0, AppDimensions.shadowOffsetY),
                        ),
                      ],
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          flex: 3,
                          child: Container(
                            width: double.infinity,
                            decoration: BoxDecoration(
                              color: Colors.grey[200],
                              borderRadius: const BorderRadius.only(
                                topLeft: Radius.circular(
                                  AppDimensions.cardBorderRadius,
                                ),
                                topRight: Radius.circular(
                                  AppDimensions.cardBorderRadius,
                                ),
                              ),
                            ),
                            child: const Icon(
                              Icons.image,
                              size: AppDimensions.iconXl,
                              color: Colors.grey,
                            ),
                          ),
                        ),
                        Expanded(
                          flex: 2,
                          child: Padding(
                            padding: const EdgeInsets.all(
                              AppDimensions.spacingM,
                            ),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Ürün ${index + 1}',
                                  style: AppTypography.bodyMedium.copyWith(
                                    fontWeight: FontWeight.w600,
                                  ),
                                  maxLines: 2,
                                  overflow: TextOverflow.ellipsis,
                                ),
                                const SizedBox(height: AppDimensions.spacingXs),
                                Text(
                                  '₺${(index + 1) * 10}.00',
                                  style: AppTypography.bodyMedium.copyWith(
                                    color: AppColors.primary,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                const Spacer(),
                                SizedBox(
                                  width: double.infinity,
                                  child: ElevatedButton(
                                    onPressed: () {},
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: AppColors.primary,
                                      foregroundColor: Colors.white,
                                      padding: const EdgeInsets.symmetric(
                                        vertical: 8,
                                      ),
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(8),
                                      ),
                                    ),
                                    child: Text(
                                      l10n.addToCart,
                                      style: const TextStyle(fontSize: 12),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ],
                    ),
                  );
                },
              ),
            ),

            const SizedBox(height: 32),

            // Nearby Merchants Section
            Text(
              l10n.nearbyMerchants,
              style: AppTypography.headlineSmall.copyWith(
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),

            // Merchants List
            BlocBuilder<MerchantBloc, MerchantState>(
              builder: (context, state) {
                if (state is MerchantLoading) {
                  return const MerchantListSkeleton(
                    itemCount: 5,
                    showCategoryBadge: true,
                  );
                }

                if (state is MerchantError) {
                  return ErrorStateWidget(
                    errorType: _getErrorTypeFromMessage(state.message),
                    customMessage: state.message,
                    onRetry: _currentPosition != null
                        ? () {
                            context.read<MerchantBloc>().add(
                              LoadNearbyMerchants(
                                latitude: _currentPosition!.latitude,
                                longitude: _currentPosition!.longitude,
                                radius: 5.0,
                              ),
                            );
                          }
                        : null,
                  );
                }

                if (state is MerchantsLoaded) {
                  final merchants = state.merchants;

                  if (merchants.isEmpty) {
                    return Center(
                      child: Padding(
                        padding: const EdgeInsets.all(32),
                        child: Column(
                          children: [
                            Icon(
                              Icons.store_outlined,
                              size: 48,
                              color: AppColors.textSecondary,
                            ),
                            const SizedBox(height: 16),
                            Text(
                              l10n.noMerchantsFound,
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  }

                  return ListView.builder(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: merchants.length,
                    itemBuilder: (context, index) {
                      final merchant = merchants[index];
                      return MerchantCard(
                        merchant: merchant,
                        showCategoryBadge: true, // Ana sayfada badge göster
                      );
                    },
                  );
                }

                return const SizedBox.shrink();
              },
            ),

            const SizedBox(height: 100), // Bottom padding for navigation
          ],
        ),
      ),
    );
  }

  Widget _buildCategoryItem({
    required BuildContext context,
    required ServiceCategoryType categoryType,
    required IconData icon,
    required String label,
    required Color color,
  }) {
    return GestureDetector(
      onTap: () {
        // Konum yoksa hata göster
        if (_currentPosition == null) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Lütfen konum izni verin'),
              backgroundColor: AppColors.error,
            ),
          );
          return;
        }

        // Kategori sayfasına git
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => CategoryMerchantsPage(
              categoryType: categoryType,
              categoryName: label,
              latitude: _currentPosition!.latitude,
              longitude: _currentPosition!.longitude,
            ),
          ),
        );
      },
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.1),
              spreadRadius: 1,
              blurRadius: 4,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: color.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon, color: color, size: 24),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              style: AppTypography.bodySmall.copyWith(
                fontWeight: FontWeight.w500,
              ),
              textAlign: TextAlign.center,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }

  ErrorType _getErrorTypeFromMessage(String message) {
    final lowerMessage = message.toLowerCase();

    if (lowerMessage.contains('network') ||
        lowerMessage.contains('connection') ||
        lowerMessage.contains('internet') ||
        lowerMessage.contains('bağlantı')) {
      return ErrorType.network;
    } else if (lowerMessage.contains('500') ||
        lowerMessage.contains('502') ||
        lowerMessage.contains('503') ||
        lowerMessage.contains('server') ||
        lowerMessage.contains('sunucu')) {
      return ErrorType.server;
    } else if (lowerMessage.contains('404') ||
        lowerMessage.contains('not found') ||
        lowerMessage.contains('bulunamadı')) {
      return ErrorType.notFound;
    } else if (lowerMessage.contains('401') ||
        lowerMessage.contains('403') ||
        lowerMessage.contains('unauthorized') ||
        lowerMessage.contains('yetkisiz')) {
      return ErrorType.unauthorized;
    }

    return ErrorType.generic;
  }
}
