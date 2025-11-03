import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../bloc/product/product_bloc.dart';
import '../../bloc/cart/cart_bloc.dart';

class ProductDetailPage extends StatefulWidget {
  final String productId;

  const ProductDetailPage({super.key, required this.productId});

  @override
  State<ProductDetailPage> createState() => _ProductDetailPageState();
}

class _ProductDetailPageState extends State<ProductDetailPage> {
  int _quantity = 1;
  String? _selectedVariantId;
  // ignore: unused_field
  String? _selectedVariantName; // Reserved for displaying selected variant
  List<String> _selectedOptionIds = [];
  List<String> _selectedOptionNames = [];
  double _totalPrice = 0.0;

  @override
  void initState() {
    super.initState();
    // Load product details
    context.read<ProductBloc>().add(LoadProductById(widget.productId));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      body: BlocBuilder<ProductBloc, ProductState>(
        builder: (context, state) {
          if (state is ProductLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is ProductError) {
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(Icons.error_outline, size: 64, color: AppColors.error),
                  const SizedBox(height: 16),
                  Text(
                    state.message,
                    style: AppTypography.bodyLarge.copyWith(
                      color: AppColors.error,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () {
                      context.read<ProductBloc>().add(
                        LoadProductById(widget.productId),
                      );
                    },
                    child: Text(l10n.retry),
                  ),
                ],
              ),
            );
          }

          if (state is ProductLoaded) {
            final product = state.product;
            _calculateTotalPrice(product);
            return _buildProductDetail(product, l10n);
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildProductDetail(dynamic product, AppLocalizations l10n) {
    return Column(
      children: [
        // Product images
        Expanded(flex: 3, child: _buildProductImages(product)),
        // Product details
        Expanded(flex: 4, child: _buildProductDetails(product, l10n)),
      ],
    );
  }

  Widget _buildProductImages(dynamic product) {
    return Container(
      width: double.infinity,
      decoration: BoxDecoration(color: Colors.grey[100]),
      child: Stack(
        children: [
          // Main product image
          Center(
            child: CachedNetworkImage(
              imageUrl: product.imageUrl ?? '',
              width: double.infinity,
              height: double.infinity,
              fit: BoxFit.cover,
              placeholder: (context, url) => Container(
                color: Colors.grey[200],
                child: const Center(
                  child: CircularProgressIndicator(
                    valueColor: AlwaysStoppedAnimation<Color>(
                      AppColors.primary,
                    ),
                  ),
                ),
              ),
              errorWidget: (context, url, error) => Container(
                color: Colors.grey[200],
                child: const Center(
                  child: Icon(Icons.broken_image, size: 64, color: Colors.grey),
                ),
              ),
            ),
          ),
          // Back button
          Positioned(
            top: 50,
            left: 16,
            child: Container(
              decoration: BoxDecoration(
                color: Colors.black.withOpacity(0.5),
                shape: BoxShape.circle,
              ),
              child: IconButton(
                icon: const Icon(Icons.arrow_back, color: Colors.white),
                onPressed: () => Navigator.pop(context),
              ),
            ),
          ),
          // Favorite button
          Positioned(
            top: 50,
            right: 16,
            child: Container(
              decoration: BoxDecoration(
                color: Colors.black.withOpacity(0.5),
                shape: BoxShape.circle,
              ),
              child: IconButton(
                icon: const Icon(Icons.favorite_border, color: Colors.white),
                onPressed: () {
                  // TODO: Implement add to favorites functionality
                  // Requires FavoritesBloc integration
                },
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildProductDetails(dynamic product, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.only(
          topLeft: Radius.circular(24),
          topRight: Radius.circular(24),
        ),
      ),
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Product name and price
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        product.name ?? '',
                        style: AppTypography.headlineSmall.copyWith(
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        product.description ?? '',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(width: 16),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    if (product.hasDiscount == true) ...[
                      Text(
                        '₺${product.price ?? 0.0}',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.textSecondary,
                          decoration: TextDecoration.lineThrough,
                        ),
                      ),
                      const SizedBox(height: 4),
                    ],
                    Text(
                      '₺${product.finalPrice ?? product.price ?? 0.0}',
                      style: AppTypography.headlineSmall.copyWith(
                        color: AppColors.primary,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    if (product.hasDiscount == true) ...[
                      const SizedBox(height: 4),
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
                          '%${product.discountPercentage?.toInt() ?? 0} ${l10n.discount}',
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
              ],
            ),

            const SizedBox(height: 24),

            // Rating and reviews
            if (product.rating != null && product.rating > 0) ...[
              Row(
                children: [
                  Icon(Icons.star, color: Colors.amber, size: 20),
                  const SizedBox(width: 4),
                  Text(
                    '${product.rating}',
                    style: AppTypography.bodyLarge.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(width: 8),
                  Text(
                    '(${product.reviewCount ?? 0} ${l10n.reviews})',
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),
            ],

            // Variants
            if (product.variants != null && product.variants.isNotEmpty) ...[
              Text(
                l10n.variants,
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 12),
              Wrap(
                spacing: 8,
                runSpacing: 8,
                children: product.variants.map<Widget>((variant) {
                  final isSelected = _selectedVariantId == variant.id;
                  return GestureDetector(
                    onTap: () {
                      setState(() {
                        _selectedVariantId = variant.id;
                        _selectedVariantName = variant.name;
                      });
                    },
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 8,
                      ),
                      decoration: BoxDecoration(
                        color: isSelected ? AppColors.primary : AppColors.white,
                        border: Border.all(
                          color: isSelected
                              ? AppColors.primary
                              : AppColors.textSecondary,
                        ),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        variant.name,
                        style: AppTypography.bodyMedium.copyWith(
                          color: isSelected
                              ? AppColors.white
                              : AppColors.textPrimary,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                  );
                }).toList(),
              ),
              const SizedBox(height: 24),
            ],

            // Options
            if (product.options != null && product.options.isNotEmpty) ...[
              ...product.options.map<Widget>((option) {
                return Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      option.name,
                      style: AppTypography.bodyLarge.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    if (option.isRequired)
                      Text(
                        ' (${l10n.required})',
                        style: AppTypography.bodyMedium.copyWith(
                          color: AppColors.error,
                        ),
                      ),
                    const SizedBox(height: 12),
                    if (option.type == 'single')
                      ...option.values.map<Widget>((value) {
                        return RadioListTile<String>(
                          title: Text(value.name),
                          subtitle: value.price > 0
                              ? Text('+₺${value.price}')
                              : null,
                          value: value.id,
                          groupValue: _selectedOptionIds.isNotEmpty
                              ? _selectedOptionIds.first
                              : null,
                          onChanged: (String? newValue) {
                            setState(() {
                              _selectedOptionIds = newValue != null
                                  ? [newValue]
                                  : [];
                              _selectedOptionNames = newValue != null
                                  ? [value.name]
                                  : [];
                            });
                          },
                          activeColor: AppColors.primary,
                        );
                      }).toList()
                    else
                      ...option.values.map<Widget>((value) {
                        final isSelected = _selectedOptionIds.contains(
                          value.id,
                        );
                        return CheckboxListTile(
                          title: Text(value.name),
                          subtitle: value.price > 0
                              ? Text('+₺${value.price}')
                              : null,
                          value: isSelected,
                          onChanged: (bool? newValue) {
                            setState(() {
                              if (newValue == true) {
                                _selectedOptionIds.add(value.id);
                                _selectedOptionNames.add(value.name);
                              } else {
                                _selectedOptionIds.remove(value.id);
                                _selectedOptionNames.remove(value.name);
                              }
                            });
                          },
                          activeColor: AppColors.primary,
                        );
                      }).toList(),
                    const SizedBox(height: 16),
                  ],
                );
              }).toList(),
            ],

            // Nutritional info
            if (product.nutritionalInfo != null &&
                product.nutritionalInfo.isNotEmpty) ...[
              Text(
                l10n.nutritionalInfo,
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(height: 12),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.grey[50],
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Column(
                  children: product.nutritionalInfo.entries.map<Widget>((
                    entry,
                  ) {
                    return Padding(
                      padding: const EdgeInsets.symmetric(vertical: 4),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(entry.key, style: AppTypography.bodyMedium),
                          Text(
                            entry.value.toString(),
                            style: AppTypography.bodyMedium.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ],
                      ),
                    );
                  }).toList(),
                ),
              ),
              const SizedBox(height: 24),
            ],

            // Quantity selector and add to cart
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.grey.withOpacity(0.1),
                    spreadRadius: 1,
                    blurRadius: 4,
                    offset: const Offset(0, -2),
                  ),
                ],
              ),
              child: Row(
                children: [
                  // Quantity selector
                  Container(
                    decoration: BoxDecoration(
                      border: Border.all(color: AppColors.textSecondary),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        IconButton(
                          onPressed: _quantity > 1
                              ? () {
                                  setState(() {
                                    _quantity--;
                                  });
                                }
                              : null,
                          icon: const Icon(Icons.remove),
                        ),
                        Container(
                          width: 50,
                          padding: const EdgeInsets.symmetric(vertical: 8),
                          child: Text(
                            '$_quantity',
                            textAlign: TextAlign.center,
                            style: AppTypography.bodyLarge.copyWith(
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        IconButton(
                          onPressed: () {
                            setState(() {
                              _quantity++;
                            });
                          },
                          icon: const Icon(Icons.add),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),
                  // Total price
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        Text(
                          l10n.total,
                          style: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                        Text(
                          '₺${_totalPrice.toStringAsFixed(2)}',
                          style: AppTypography.headlineSmall.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(width: 16),
                  // Add to cart button
                  Expanded(
                    flex: 2,
                    child: BlocListener<CartBloc, CartState>(
                      listener: (context, state) {
                        if (state is CartItemAdded) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(l10n.addedToCart),
                              backgroundColor: AppColors.success,
                            ),
                          );
                        } else if (state is CartError) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            SnackBar(
                              content: Text(state.message),
                              backgroundColor: AppColors.error,
                            ),
                          );
                        }
                      },
                      child: ElevatedButton(
                        onPressed: product.isAvailable == true
                            ? () {
                                context.read<CartBloc>().add(
                                  AddToCart(
                                    merchantId: product.merchantId,
                                    productId: product.id,
                                    quantity: _quantity,
                                    productName: product.name,
                                    price: product.finalPrice,
                                    category: product.category,
                                  ),
                                );
                              }
                            : null,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primary,
                          foregroundColor: AppColors.white,
                          padding: const EdgeInsets.symmetric(vertical: 16),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(12),
                          ),
                        ),
                        child: Text(
                          product.isAvailable == true
                              ? l10n.addToCart
                              : l10n.outOfStock,
                          style: const TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),
          ],
        ),
      ),
    );
  }

  void _calculateTotalPrice(dynamic product) {
    double basePrice = product.finalPrice ?? product.price ?? 0.0;

    // Add variant price
    if (_selectedVariantId != null && product.variants != null) {
      final variant = product.variants.firstWhere(
        (v) => v.id == _selectedVariantId,
        orElse: () => null,
      );
      if (variant != null) {
        basePrice = variant.price;
      }
    }

    // Add option prices
    double optionPrice = 0.0;
    if (_selectedOptionIds.isNotEmpty && product.options != null) {
      for (final option in product.options) {
        for (final value in option.values) {
          if (_selectedOptionIds.contains(value.id)) {
            optionPrice += value.price;
          }
        }
      }
    }

    _totalPrice = (basePrice + optionPrice) * _quantity;
  }
}
