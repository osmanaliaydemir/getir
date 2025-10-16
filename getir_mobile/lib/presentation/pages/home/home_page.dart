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
              height: 230, // Increased height to fix overflow
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
                      mainAxisSize: MainAxisSize.min, // Prevent overflow
                      children: [
                        // Image section - fixed height
                        Container(
                          height: 120,
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
                        // Content section - flexible
                        Expanded(
                          child: Padding(
                            padding: const EdgeInsets.all(
                              AppDimensions.spacingM,
                            ),
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                Text(
                                  'Ürün ${index + 1}',
                                  style: AppTypography.bodyMedium.copyWith(
                                    fontWeight: FontWeight.w600,
                                  ),
                                  maxLines: 1, // Reduced from 2 to 1
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
                                const SizedBox(height: AppDimensions.spacingS),
                                SizedBox(
                                  width: double.infinity,
                                  height: 32, // Fixed button height
                                  child: ElevatedButton(
                                    onPressed: () {},
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: AppColors.primary,
                                      foregroundColor: Colors.white,
                                      padding: const EdgeInsets.symmetric(
                                        vertical: 4, // Reduced padding
                                      ),
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(8),
                                      ),
                                    ),
                                    child: Text(
                                      l10n.addToCart,
                                      style: const TextStyle(
                                        fontSize: 11,
                                      ), // Reduced font size
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

            // Merchants List - TEMPORARY: Show mock data until API is ready
            _buildMockMerchantsList(l10n),

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

  Widget _buildMockMerchantsList(AppLocalizations l10n) {
    // Mock merchants data until API is ready
    final mockMerchants = [
      {
        'name': 'Migros Kadıköy',
        'description': 'Taze ürünler, hızlı teslimat',
        'rating': 4.5,
        'deliveryFee': 3.50,
        'estimatedDeliveryTime': 25,
        'distance': 0.8,
        'isOpen': true,
        'category': 'Market',
      },
      {
        'name': 'Burger King',
        'description': 'Lezzetli hamburgerler',
        'rating': 4.2,
        'deliveryFee': 4.00,
        'estimatedDeliveryTime': 35,
        'distance': 1.2,
        'isOpen': true,
        'category': 'Restoran',
      },
      {
        'name': 'Eczane Plus',
        'description': 'İlaçlar ve sağlık ürünleri',
        'rating': 4.7,
        'deliveryFee': 2.50,
        'estimatedDeliveryTime': 20,
        'distance': 0.5,
        'isOpen': true,
        'category': 'Eczane',
      },
    ];

    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: mockMerchants.length,
      itemBuilder: (context, index) {
        final merchant = mockMerchants[index];
        return Container(
          margin: const EdgeInsets.only(bottom: 16),
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
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              children: [
                // Merchant logo placeholder
                Container(
                  width: 60,
                  height: 60,
                  decoration: BoxDecoration(
                    color: Colors.grey[200],
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(Icons.store, color: Colors.grey, size: 30),
                ),
                const SizedBox(width: 16),
                // Merchant info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        merchant['name'] as String,
                        style: AppTypography.bodyLarge.copyWith(
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                      const SizedBox(height: 4),
                      Text(
                        merchant['description'] as String,
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          Icon(Icons.star, color: Colors.amber, size: 16),
                          const SizedBox(width: 4),
                          Text(
                            '${merchant['rating']}',
                            style: AppTypography.bodySmall.copyWith(
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                          const SizedBox(width: 16),
                          Icon(
                            Icons.access_time,
                            color: AppColors.textSecondary,
                            size: 16,
                          ),
                          const SizedBox(width: 4),
                          Text(
                            '${merchant['estimatedDeliveryTime']} dk',
                            style: AppTypography.bodySmall.copyWith(
                              color: AppColors.textSecondary,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
                // Delivery info
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 8,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: AppColors.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(
                        merchant['category'] as String,
                        style: AppTypography.bodySmall.copyWith(
                          color: AppColors.primary,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      '₺${merchant['deliveryFee']}',
                      style: AppTypography.bodyMedium.copyWith(
                        fontWeight: FontWeight.w600,
                        color: AppColors.primary,
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        );
      },
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
