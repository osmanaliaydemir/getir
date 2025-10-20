import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:geolocator/geolocator.dart';
import 'package:shimmer/shimmer.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../core/constants/app_dimensions.dart';
import '../../../core/cubits/language/language_cubit.dart';
import '../../../core/di/injection.dart';
import '../../../domain/entities/service_category.dart';
import '../../../domain/entities/service_category_type.dart';
import '../../../domain/entities/product.dart';
import '../../widgets/common/language_selector.dart';
import '../../widgets/merchant/merchant_card.dart';
import '../../widgets/merchant/merchant_card_skeleton.dart';
import '../../../core/widgets/error_state_widget.dart';
import '../../bloc/merchant/merchant_bloc.dart';
import '../../bloc/product/product_bloc.dart';
import '../../bloc/cart/cart_bloc.dart';
import '../../cubit/category/category_cubit.dart';
import '../../../core/services/logger_service.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(
          create: (context) =>
              getIt<CategoryCubit>()..loadAllActiveCategories(),
        ),
      ],
      child: const _HomePageContent(),
    );
  }
}

class _HomePageContent extends StatefulWidget {
  const _HomePageContent();

  @override
  State<_HomePageContent> createState() => _HomePageContentState();
}

class _HomePageContentState extends State<_HomePageContent> {
  Position? _currentPosition;

  @override
  void initState() {
    super.initState();
    _getCurrentLocation();
    // Load popular products
    context.read<ProductBloc>().add(const LoadPopularProducts(limit: 5));
    // Load cart
    context.read<CartBloc>().add(LoadCart());
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
      if (mounted) {
        context.read<MerchantBloc>().add(
          LoadNearbyMerchants(
            latitude: position.latitude,
            longitude: position.longitude,
            radius: AppDimensions.nearbyMerchantRadiusKm,
          ),
        );
      }
    } catch (e, stackTrace) {
      logger.error(
        'Failed to get location',
        tag: 'HomePage',
        error: e,
        stackTrace: stackTrace,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(AppLocalizations.of(context).locationError),
            duration: const Duration(seconds: 3),
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
      body: RefreshIndicator(
        onRefresh: () async {
          context.read<CategoryCubit>().refreshCategories();
          context.read<ProductBloc>().add(const LoadPopularProducts(limit: 5));
          if (_currentPosition != null) {
            context.read<MerchantBloc>().add(
              LoadNearbyMerchants(
                latitude: _currentPosition!.latitude,
                longitude: _currentPosition!.longitude,
                radius: AppDimensions.nearbyMerchantRadiusKm,
              ),
            );
          }
        },
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppDimensions.spacingL),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Location Header
              _buildLocationHeader(l10n),
              const SizedBox(height: 24),

              // Search Bar
              _buildSearchBar(l10n),
              const SizedBox(height: 24),

              // Categories Section - API'den çekiliyor ✅
              _buildCategoriesSection(l10n),
              const SizedBox(height: 32),

              // Popular Products Section (şimdilik mock)
              _buildPopularProductsSection(l10n),
              const SizedBox(height: 32),

              // Nearby Merchants Section - API'den çekiliyor ✅
              _buildNearbyMerchantsSection(l10n),
              const SizedBox(height: 100), // Bottom padding for navigation
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildLocationHeader(AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(AppDimensions.spacingL),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(AppDimensions.cardBorderRadius),
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
          const Icon(Icons.keyboard_arrow_down, color: AppColors.textSecondary),
        ],
      ),
    );
  }

  Widget _buildSearchBar(AppLocalizations l10n) {
    return Container(
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
          prefixIcon: const Icon(Icons.search, color: AppColors.textSecondary),
        ),
      ),
    );
  }

  Widget _buildCategoriesSection(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.categories,
          style: AppTypography.headlineSmall.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        BlocBuilder<CategoryCubit, CategoryState>(
          builder: (context, state) {
            if (state is CategoryLoading) {
              return _buildCategoriesLoading();
            } else if (state is CategoryError) {
              return _buildCategoriesError(state.message);
            } else if (state is CategoryLoaded) {
              return _buildCategoriesGrid(state.categories);
            }
            return _buildCategoriesLoading();
          },
        ),
      ],
    );
  }

  Widget _buildCategoriesLoading() {
    return GridView.count(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisCount: AppDimensions.categoriesGridCount,
      crossAxisSpacing: AppDimensions.spacingL,
      mainAxisSpacing: AppDimensions.spacingL,
      children: List.generate(6, (index) => _CategoryShimmer()),
    );
  }

  Widget _buildCategoriesError(String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            const Icon(Icons.error_outline, color: AppColors.error, size: 48),
            const SizedBox(height: 8),
            Text(
              message,
              style: const TextStyle(color: AppColors.error),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () {
                context.read<CategoryCubit>().refreshCategories();
              },
              child: Text(AppLocalizations.of(context).retry),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCategoriesGrid(List<ServiceCategory> categories) {
    if (categories.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Text(AppLocalizations.of(context).noCategoriesFound),
        ),
      );
    }

    return GridView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: AppDimensions.categoriesGridCount,
        crossAxisSpacing: AppDimensions.spacingL,
        mainAxisSpacing: AppDimensions.spacingL,
      ),
      itemCount: categories.length,
      itemBuilder: (context, index) {
        final category = categories[index];
        return _buildCategoryItem(context: context, category: category);
      },
    );
  }

  Widget _buildCategoryItem({
    required BuildContext context,
    required ServiceCategory category,
  }) {
    // Icon ve renk eşleştirmesi (fallback)
    final iconData = _getCategoryIcon(category.type);
    final color = _getCategoryColor(category.type);

    return TweenAnimationBuilder<double>(
      duration: const Duration(milliseconds: 300),
      tween: Tween(begin: 0.0, end: 1.0),
      builder: (context, value, child) {
        return Transform.scale(
          scale: 0.8 + (value * 0.2),
          child: Opacity(opacity: value, child: child),
        );
      },
      child: GestureDetector(
        onTapDown: (_) {},
        onTapUp: (_) {},
        onTapCancel: () {},
        onTap: () {
          debugPrint(
            '📱 [HomePage] Category tapped: ${category.name} (Type: ${category.type.value})',
          );

          if (_currentPosition == null) {
            debugPrint('⚠️ [HomePage] No location - showing permission error');
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(
                  AppLocalizations.of(context).locationPermissionRequired,
                ),
                backgroundColor: AppColors.error,
              ),
            );
            return;
          }

          debugPrint('🚀 [HomePage] Navigating to CategoryMerchantsPage');
          debugPrint('   Category: ${category.name}');
          debugPrint('   Type: ${category.type.value}');
          debugPrint(
            '   Location: ${_currentPosition!.latitude}, ${_currentPosition!.longitude}',
          );

          try {
            // GoRouter ile navigate et (bottom navbar ile uyumlu)
            context.push(
              '/category-merchants/${category.type.value}'
              '?name=${Uri.encodeQueryComponent(category.name)}'
              '&lat=${_currentPosition!.latitude}'
              '&lng=${_currentPosition!.longitude}',
            );
          } catch (e, stackTrace) {
            debugPrint('❌ [HomePage] Navigation error: $e');
            logger.error(
              'Failed to navigate to category page',
              tag: 'HomePage',
              error: e,
              stackTrace: stackTrace,
            );
          }
        },
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
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
              // İkon veya iconUrl varsa göster (with caching)
              if (category.iconUrl != null && category.iconUrl!.isNotEmpty)
                Hero(
                  tag: 'category_${category.id}',
                  child: Image.network(
                    category.iconUrl!,
                    width: 40,
                    height: 40,
                    loadingBuilder: (context, child, loadingProgress) {
                      if (loadingProgress == null) return child;
                      return SizedBox(
                        width: 40,
                        height: 40,
                        child: Center(
                          child: CircularProgressIndicator(
                            value: loadingProgress.expectedTotalBytes != null
                                ? loadingProgress.cumulativeBytesLoaded /
                                      loadingProgress.expectedTotalBytes!
                                : null,
                            strokeWidth: 2,
                          ),
                        ),
                      );
                    },
                    errorBuilder: (context, error, stackTrace) {
                      return _buildFallbackIcon(iconData, color);
                    },
                    cacheWidth: 80, // Optimize memory
                    cacheHeight: 80,
                  ),
                )
              else
                Hero(
                  tag: 'category_${category.id}',
                  child: _buildFallbackIcon(iconData, color),
                ),
              const SizedBox(height: 8),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 4),
                child: Text(
                  category.name, // ✅ API'den gelen isim
                  style: AppTypography.bodySmall.copyWith(
                    fontWeight: FontWeight.w500,
                  ),
                  textAlign: TextAlign.center,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
              if (category.merchantCount > 0)
                Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Text(
                    '${category.merchantCount} mağaza',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                      fontSize: 10,
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFallbackIcon(IconData icon, Color color) {
    return Container(
      width: 40,
      height: 40,
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Icon(icon, color: color, size: 24),
    );
  }

  IconData _getCategoryIcon(ServiceCategoryType type) {
    switch (type) {
      case ServiceCategoryType.restaurant:
        return Icons.restaurant;
      case ServiceCategoryType.market:
        return Icons.local_grocery_store;
      case ServiceCategoryType.pharmacy:
        return Icons.local_pharmacy;
      case ServiceCategoryType.water:
        return Icons.water_drop;
      case ServiceCategoryType.cafe:
        return Icons.local_cafe;
      case ServiceCategoryType.bakery:
        return Icons.bakery_dining;
      case ServiceCategoryType.other:
        return Icons.more_horiz;
    }
  }

  Color _getCategoryColor(ServiceCategoryType type) {
    switch (type) {
      case ServiceCategoryType.restaurant:
        return Colors.red;
      case ServiceCategoryType.market:
        return Colors.orange;
      case ServiceCategoryType.pharmacy:
        return Colors.green;
      case ServiceCategoryType.water:
        return Colors.blue;
      case ServiceCategoryType.cafe:
        return Colors.brown;
      case ServiceCategoryType.bakery:
        return Colors.pink;
      case ServiceCategoryType.other:
        return Colors.grey;
    }
  }

  Widget _buildPopularProductsSection(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
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
        BlocBuilder<ProductBloc, ProductState>(
          builder: (context, state) {
            if (state is ProductLoading) {
              return _buildPopularProductsLoading();
            } else if (state is ProductError) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Text(
                    'Popüler ürünler yüklenemedi',
                    style: const TextStyle(color: AppColors.error),
                  ),
                ),
              );
            } else if (state is ProductsLoaded) {
              if (state.products.isEmpty) {
                return const SizedBox.shrink();
              }
              return _buildPopularProductsList(state.products);
            }
            return const SizedBox.shrink();
          },
        ),
      ],
    );
  }

  Widget _buildPopularProductsLoading() {
    return SizedBox(
      height: 230,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: 3,
        itemBuilder: (context, index) {
          return Container(
            width: AppDimensions.productCardWidth,
            margin: const EdgeInsets.only(right: AppDimensions.spacingL),
            child: Shimmer.fromColors(
              baseColor: Colors.grey[300]!,
              highlightColor: Colors.grey[100]!,
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(
                    AppDimensions.cardBorderRadius,
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildPopularProductsList(List<Product> products) {
    return SizedBox(
      height: 250,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: products.length,
        itemBuilder: (context, index) {
          final product = products[index];
          return Container(
            width: AppDimensions.productCardWidth,
            margin: const EdgeInsets.only(right: AppDimensions.spacingL),
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
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisSize: MainAxisSize.min,
              children: [
                // Product Image
                Container(
                  height: 120,
                  width: double.infinity,
                  decoration: BoxDecoration(
                    color: Colors.grey[200],
                    borderRadius: const BorderRadius.only(
                      topLeft: Radius.circular(AppDimensions.cardBorderRadius),
                      topRight: Radius.circular(AppDimensions.cardBorderRadius),
                    ),
                  ),
                  child: product.imageUrl.isNotEmpty
                      ? Image.network(
                          product.imageUrl,
                          fit: BoxFit.cover,
                          errorBuilder: (context, error, stackTrace) {
                            return const Icon(
                              Icons.image,
                              size: AppDimensions.iconXl,
                              color: Colors.grey,
                            );
                          },
                        )
                      : const Icon(
                          Icons.image,
                          size: AppDimensions.iconXl,
                          color: Colors.grey,
                        ),
                ),
                // Product Info
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(AppDimensions.spacingM),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Text(
                          product.name,
                          style: AppTypography.bodyMedium.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                        const SizedBox(height: 4),
                        // Rating
                        if (product.rating != null && product.rating! > 0)
                          Row(
                            children: [
                              const Icon(
                                Icons.star,
                                size: 14,
                                color: Colors.amber,
                              ),
                              const SizedBox(width: 4),
                              Text(
                                product.rating!.toStringAsFixed(1),
                                style: AppTypography.bodySmall.copyWith(
                                  fontWeight: FontWeight.w500,
                                ),
                              ),
                              if (product.reviewCount != null &&
                                  product.reviewCount! > 0)
                                Text(
                                  ' (${product.reviewCount})',
                                  style: AppTypography.bodySmall.copyWith(
                                    color: AppColors.textSecondary,
                                    fontSize: 10,
                                  ),
                                ),
                            ],
                          ),
                        const Spacer(),
                        Text(
                          '₺${product.finalPrice.toStringAsFixed(2)}',
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 4),
                        _AddToCartButton(product: product),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildNearbyMerchantsSection(AppLocalizations l10n) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          l10n.nearbyMerchants,
          style: AppTypography.headlineSmall.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 16),
        BlocBuilder<MerchantBloc, MerchantState>(
          builder: (context, state) {
            if (state is MerchantLoading) {
              return _buildMerchantsLoading();
            } else if (state is MerchantError) {
              return ErrorStateWidget(
                errorType: _getErrorTypeFromMessage(state.message),
                onRetry: () {
                  if (_currentPosition != null) {
                    context.read<MerchantBloc>().add(
                      LoadNearbyMerchants(
                        latitude: _currentPosition!.latitude,
                        longitude: _currentPosition!.longitude,
                        radius: AppDimensions.nearbyMerchantRadiusKm,
                      ),
                    );
                  }
                },
              );
            } else if (state is MerchantsLoaded) {
              if (state.merchants.isEmpty) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Text(AppLocalizations.of(context).noMerchantsNearby),
                  ),
                );
              }
              return _buildMerchantsList(state.merchants);
            }
            return const SizedBox.shrink();
          },
        ),
      ],
    );
  }

  Widget _buildMerchantsLoading() {
    return Column(
      children: List.generate(
        3,
        (index) => Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: MerchantCardSkeleton(),
        ),
      ),
    );
  }

  Widget _buildMerchantsList(merchants) {
    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: merchants.length,
      itemBuilder: (context, index) {
        final merchant = merchants[index];
        return Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: MerchantCard(merchant: merchant),
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

/// Kategori skeleton/shimmer widget
class _CategoryShimmer extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Shimmer.fromColors(
      baseColor: Colors.grey[300]!,
      highlightColor: Colors.grey[100]!,
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(12),
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(8),
              ),
            ),
            const SizedBox(height: 8),
            Container(
              width: 60,
              height: 12,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(4),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// Animasyonlu Sepete Ekle Butonu
class _AddToCartButton extends StatefulWidget {
  final Product product;

  const _AddToCartButton({required this.product});

  @override
  State<_AddToCartButton> createState() => _AddToCartButtonState();
}

class _AddToCartButtonState extends State<_AddToCartButton>
    with SingleTickerProviderStateMixin {
  late AnimationController _animationController;
  late Animation<double> _scaleAnimation;
  bool _isAdding = false;

  @override
  void initState() {
    super.initState();
    _animationController = AnimationController(
      duration: const Duration(milliseconds: 200),
      vsync: this,
    );
    _scaleAnimation = Tween<double>(begin: 1.0, end: 0.9).animate(
      CurvedAnimation(parent: _animationController, curve: Curves.easeInOut),
    );
  }

  @override
  void dispose() {
    _animationController.dispose();
    super.dispose();
  }

  Future<void> _addToCart() async {
    if (_isAdding) return;

    setState(() => _isAdding = true);

    // Animasyon başlat
    await _animationController.forward();
    await _animationController.reverse();

    // Cart BLoC'a event gönder
    if (!mounted) return;

    context.read<CartBloc>().add(
      AddToCart(
        merchantId: widget.product.merchantId,
        productId: widget.product.id,
        quantity: 1,
        productName: widget.product.name,
        price: widget.product.finalPrice,
        category: widget.product.category,
      ),
    );

    // Başarı feedback'i
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.check_circle, color: Colors.white),
            const SizedBox(width: 8),
            Expanded(
              child: Text(
                '${widget.product.name} sepete eklendi!',
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ],
        ),
        backgroundColor: Colors.green,
        duration: const Duration(seconds: 2),
        behavior: SnackBarBehavior.floating,
        margin: const EdgeInsets.all(16),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      ),
    );

    setState(() => _isAdding = false);
  }

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartBloc, CartState>(
      builder: (context, cartState) {
        // Sepette bu ürün var mı kontrol et
        int quantityInCart = 0;
        String? cartItemId;

        if (cartState is CartLoaded) {
          try {
            final cartItem = cartState.cart.items.firstWhere(
              (item) => item.productId == widget.product.id,
            );
            quantityInCart = cartItem.quantity;
            cartItemId = cartItem.id;
          } catch (e) {
            // Ürün sepette yok
          }
        }

        // Sepette varsa: artı/eksi butonları
        if (quantityInCart > 0) {
          return ScaleTransition(
            scale: _scaleAnimation,
            child: Container(
              height: 32,
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                children: [
                  // Eksi butonu
                  Expanded(
                    child: IconButton(
                      padding: EdgeInsets.zero,
                      onPressed: _isAdding
                          ? null
                          : () {
                              if (quantityInCart > 1) {
                                context.read<CartBloc>().add(
                                  UpdateCartItem(
                                    itemId: cartItemId!,
                                    quantity: quantityInCart - 1,
                                  ),
                                );
                              } else {
                                context.read<CartBloc>().add(
                                  RemoveFromCart(cartItemId!),
                                );
                              }
                            },
                      icon: Icon(
                        quantityInCart > 1
                            ? Icons.remove
                            : Icons.delete_outline,
                        color: Colors.white,
                        size: 16,
                      ),
                    ),
                  ),
                  // Adet
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8),
                    child: Text(
                      quantityInCart.toString(),
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                        fontSize: 14,
                      ),
                    ),
                  ),
                  // Artı butonu
                  Expanded(
                    child: IconButton(
                      padding: EdgeInsets.zero,
                      onPressed: _isAdding
                          ? null
                          : () {
                              context.read<CartBloc>().add(
                                UpdateCartItem(
                                  itemId: cartItemId!,
                                  quantity: quantityInCart + 1,
                                ),
                              );
                            },
                      icon: const Icon(
                        Icons.add,
                        color: Colors.white,
                        size: 16,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );
        }

        // Sepette yoksa: normal sepete ekle butonu
        return ScaleTransition(
          scale: _scaleAnimation,
          child: SizedBox(
            width: double.infinity,
            height: 32,
            child: ElevatedButton(
              onPressed: _isAdding ? null : _addToCart,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                disabledBackgroundColor: AppColors.primary.withOpacity(0.6),
                padding: const EdgeInsets.symmetric(vertical: 4),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              child: _isAdding
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                      ),
                    )
                  : Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.add_shopping_cart, size: 14),
                        const SizedBox(width: 4),
                        Text(
                          AppLocalizations.of(context).addToCart,
                          style: const TextStyle(fontSize: 11),
                        ),
                      ],
                    ),
            ),
          ),
        );
      },
    );
  }
}
