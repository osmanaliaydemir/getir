import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../bloc/address/address_bloc.dart';
import '../../bloc/cart/cart_bloc.dart';
import '../../bloc/order/order_bloc.dart';
import '../../bloc/merchant/merchant_bloc.dart';
import '../../bloc/working_hours/working_hours_bloc.dart';
import '../../bloc/working_hours/working_hours_event.dart';
import '../../bloc/working_hours/working_hours_state.dart';
import '../../../domain/entities/address.dart';
import '../../../domain/entities/order.dart';
import '../../../domain/entities/working_hours.dart';
import '../../../data/datasources/order_datasource.dart';

class CheckoutPage extends StatefulWidget {
  const CheckoutPage({super.key});

  @override
  State<CheckoutPage> createState() => _CheckoutPageState();
}

class _CheckoutPageState extends State<CheckoutPage> {
  UserAddress? _selectedAddress;
  PaymentMethod _selectedPaymentMethod = PaymentMethod.cash;
  final TextEditingController _notesController = TextEditingController();
  final TextEditingController _couponController = TextEditingController();

  // Yeni state değişkenleri
  bool _ringDoorbell = true; // Zile basma opsiyonu
  bool _isApplyingCoupon = false;
  bool _scheduleForLater = false; // Gelecek tarihli sipariş
  DateTime? _scheduledTime; // Planl anan teslimat zamanı
  String? _merchantId; // Merchant ID
  bool _workingHoursLoaded = false;

  @override
  void initState() {
    super.initState();
    // Load addresses and cart
    context.read<AddressBloc>().add(LoadUserAddresses());
    context.read<CartBloc>().add(LoadCart());
  }

  @override
  void dispose() {
    _notesController.dispose();
    _couponController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.checkout),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body: BlocListener<OrderBloc, OrderState>(
        listener: (context, state) {
          if (state is OrderCreated) {
            // Navigate to order confirmation with GoRouter
            context.goNamed('order-confirmation', extra: state.order);
          } else if (state is OrderError) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text(state.message),
                backgroundColor: AppColors.error,
              ),
            );
          }
        },
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: BlocBuilder<CartBloc, CartState>(
            builder: (context, cartState) {
              // Get merchantId from cart
              if (cartState is CartLoaded &&
                  cartState.cart.items.isNotEmpty &&
                  _merchantId == null) {
                _merchantId = cartState.cart.items.first.merchantId;
                // Load working hours
                if (!_workingHoursLoaded && _merchantId != null) {
                  Future.microtask(() {
                    context.read<WorkingHoursBloc>().add(
                      LoadWorkingHours(_merchantId!),
                    );
                    context.read<MerchantBloc>().add(
                      LoadMerchantById(_merchantId!),
                    );
                  });
                  _workingHoursLoaded = true;
                }
              }

              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Merchant Status Warning (if closed)
                  BlocBuilder<WorkingHoursBloc, WorkingHoursState>(
                    builder: (context, whState) {
                      if (whState is WorkingHoursLoaded && !whState.isOpen) {
                        return _buildMerchantClosedWarning(
                          l10n,
                          whState.nextOpenTime,
                        );
                      }
                      return const SizedBox.shrink();
                    },
                  ),

                  // Delivery Address Section
                  Semantics(
                    header: true,
                    label: l10n.deliveryAddress,
                    child: _buildSectionTitle(l10n.deliveryAddress),
                  ),
                  const SizedBox(height: 12),
                  _buildAddressSection(l10n),

                  const SizedBox(height: 24),

                  // Payment Method Section
                  Semantics(
                    header: true,
                    label: l10n.paymentMethod,
                    child: _buildSectionTitle(l10n.paymentMethod),
                  ),
                  const SizedBox(height: 12),
                  _buildPaymentMethodSection(l10n),

                  const SizedBox(height: 24),

                  // Order Notes Section
                  Semantics(
                    header: true,
                    label: l10n.orderNotes,
                    child: _buildSectionTitle(l10n.orderNotes),
                  ),
                  const SizedBox(height: 12),
                  _buildOrderNotesSection(l10n),

                  const SizedBox(height: 24),

                  // Delivery Details Section (YENİ)
                  Semantics(
                    header: true,
                    label: 'Teslimat Detayları',
                    child: _buildSectionTitle('Teslimat Detayları'),
                  ),
                  const SizedBox(height: 12),
                  _buildDeliveryDetailsSection(l10n),

                  const SizedBox(height: 24),

                  // Coupon Section (YENİ)
                  Semantics(
                    header: true,
                    label: 'Kampanya ve İndirim Kodu',
                    child: _buildSectionTitle('Kampanya ve İndirim Kodu'),
                  ),
                  const SizedBox(height: 12),
                  _buildCouponSection(l10n),

                  const SizedBox(height: 24),

                  // Order Summary Section
                  Semantics(
                    header: true,
                    label: l10n.orderSummary,
                    child: _buildSectionTitle(l10n.orderSummary),
                  ),
                  const SizedBox(height: 12),
                  _buildOrderSummarySection(l10n),

                  const SizedBox(height: 32),

                  // Place Order Button
                  Semantics(
                    button: true,
                    label: l10n.placeOrder,
                    enabled:
                        (context.read<OrderBloc>().state is! OrderLoading) &&
                        _selectedAddress != null,
                    child: _buildPlaceOrderButton(l10n),
                  ),
                ],
              );
            },
          ),
        ),
      ),
    );
  }

  Widget _buildSectionTitle(String title) {
    return Text(
      title,
      style: AppTypography.headlineSmall.copyWith(fontWeight: FontWeight.bold),
    );
  }

  Widget _buildAddressSection(AppLocalizations l10n) {
    return BlocBuilder<AddressBloc, AddressState>(
      builder: (context, state) {
        if (state is AddressLoading) {
          return Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.textSecondary),
            ),
            child: const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            ),
          );
        }

        if (state is AddressError) {
          return Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.textSecondary),
            ),
            child: Column(
              children: [
                Icon(Icons.error_outline, color: AppColors.error, size: 32),
                const SizedBox(height: 8),
                Text(
                  state.message,
                  style: AppTypography.bodyMedium.copyWith(
                    color: AppColors.error,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 12),
                ElevatedButton(
                  onPressed: () {
                    context.read<AddressBloc>().add(LoadUserAddresses());
                  },
                  child: Text(l10n.retry),
                ),
              ],
            ),
          );
        }

        if (state is AddressesLoaded) {
          final addresses = state.addresses;

          if (addresses.isEmpty) {
            return Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: AppColors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: AppColors.textSecondary),
              ),
              child: Column(
                children: [
                  Icon(
                    Icons.location_off,
                    color: AppColors.textSecondary,
                    size: 32,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    l10n.noAddressesFound,
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: () {
                      Navigator.pushNamed(context, '/address-management');
                    },
                    child: Text(l10n.addAddress),
                  ),
                ],
              ),
            );
          }

          // Set default address if none selected
          if (_selectedAddress == null) {
            final defaultAddress = addresses.firstWhere(
              (address) => address.isDefault,
              orElse: () => addresses.first,
            );
            WidgetsBinding.instance.addPostFrameCallback((_) {
              setState(() {
                _selectedAddress = defaultAddress;
              });
            });
          }

          return Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.textSecondary),
            ),
            child: Column(
              children: [
                // Selected address
                if (_selectedAddress != null) ...[
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: _getAddressTypeColor(
                            _selectedAddress!.type,
                          ).withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Icon(
                          _getAddressTypeIcon(_selectedAddress!.type),
                          color: _getAddressTypeColor(_selectedAddress!.type),
                          size: 20,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              _selectedAddress!.title,
                              style: AppTypography.bodyLarge.copyWith(
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              _selectedAddress!.fullAddress,
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.textSecondary,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                ],

                // Change address button
                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    onPressed: () {
                      _showAddressSelectionDialog(addresses, l10n);
                    },
                    icon: const Icon(Icons.edit_location),
                    label: Text(l10n.changeAddress),
                  ),
                ),
              ],
            ),
          );
        }

        return const SizedBox.shrink();
      },
    );
  }

  Widget _buildPaymentMethodSection(AppLocalizations l10n) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.textSecondary),
      ),
      child: Column(
        children: PaymentMethod.values.map((method) {
          final isSelected = _selectedPaymentMethod == method;
          return RadioListTile<PaymentMethod>(
            title: Row(
              children: [
                Icon(
                  _getPaymentMethodIcon(method),
                  color: isSelected
                      ? AppColors.primary
                      : AppColors.textSecondary,
                ),
                const SizedBox(width: 12),
                Text(method.displayName),
              ],
            ),
            subtitle: Text(_getPaymentMethodDescription(method, l10n)),
            value: method,
            groupValue: _selectedPaymentMethod,
            onChanged: (PaymentMethod? value) {
              setState(() {
                _selectedPaymentMethod = value!;
              });
            },
            activeColor: AppColors.primary,
          );
        }).toList(),
      ),
    );
  }

  Widget _buildOrderNotesSection(AppLocalizations l10n) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.textSecondary),
      ),
      child: TextField(
        controller: _notesController,
        decoration: InputDecoration(
          hintText: l10n.orderNotesHint,
          border: InputBorder.none,
          contentPadding: EdgeInsets.zero,
        ),
        maxLines: 3,
      ),
    );
  }

  /// YENİ: Teslimat Detayları Section
  Widget _buildDeliveryDetailsSection(AppLocalizations l10n) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.textSecondary),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Teslimat Süresi
          Row(
            children: [
              const Icon(Icons.schedule, color: AppColors.primary, size: 20),
              const SizedBox(width: 8),
              Text(
                'Tahmini Teslimat:',
                style: AppTypography.bodyMedium.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
              const Spacer(),
              Text(
                '25-35 dakika',
                style: AppTypography.bodyMedium.copyWith(
                  color: AppColors.success,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),

          const SizedBox(height: 16),
          const Divider(),
          const SizedBox(height: 12),

          // İleri Tarihli Sipariş (Mağaza kapalıysa)
          // TODO: Merchant isOpen durumuna göre göster
          // if (!merchant.isOpen) ...

          // Zile Basma Opsiyonu
          CheckboxListTile(
            value: _ringDoorbell,
            onChanged: (value) {
              setState(() {
                _ringDoorbell = value ?? true;
              });
            },
            title: const Text('Kapı zili çalınsın'),
            subtitle: const Text('Kurye kapı zilinizi çalacaktır'),
            contentPadding: EdgeInsets.zero,
            controlAffinity: ListTileControlAffinity.leading,
            activeColor: AppColors.primary,
          ),
        ],
      ),
    );
  }

  /// YENİ: Kampanya ve Kupon Kodu Section
  Widget _buildCouponSection(AppLocalizations l10n) {
    return BlocBuilder<CartBloc, CartState>(
      builder: (context, state) {
        final hasCoupon =
            state is CartLoaded &&
            state.cart.couponCode != null &&
            state.cart.couponCode!.isNotEmpty;

        return Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: AppColors.textSecondary),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Kupon kodu input
              if (!hasCoupon) ...[
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _couponController,
                        decoration: InputDecoration(
                          hintText: 'Kampanya kodu girin',
                          hintStyle: AppTypography.bodyMedium.copyWith(
                            color: AppColors.textSecondary,
                          ),
                          prefixIcon: const Icon(
                            Icons.local_offer,
                            color: AppColors.primary,
                          ),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                            borderSide: const BorderSide(
                              color: AppColors.textSecondary,
                            ),
                          ),
                          focusedBorder: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                            borderSide: const BorderSide(
                              color: AppColors.primary,
                              width: 2,
                            ),
                          ),
                          contentPadding: const EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 12,
                          ),
                        ),
                        textCapitalization: TextCapitalization.characters,
                      ),
                    ),
                    const SizedBox(width: 12),
                    ElevatedButton(
                      onPressed: _isApplyingCoupon
                          ? null
                          : () {
                              if (_couponController.text.trim().isNotEmpty) {
                                setState(() => _isApplyingCoupon = true);
                                context.read<CartBloc>().add(
                                  ApplyCoupon(_couponController.text.trim()),
                                );
                                Future.delayed(const Duration(seconds: 1), () {
                                  if (mounted) {
                                    setState(() => _isApplyingCoupon = false);
                                  }
                                });
                              }
                            },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: AppColors.white,
                        padding: const EdgeInsets.symmetric(
                          horizontal: 20,
                          vertical: 16,
                        ),
                      ),
                      child: _isApplyingCoupon
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                valueColor: AlwaysStoppedAnimation<Color>(
                                  AppColors.white,
                                ),
                              ),
                            )
                          : const Text('Uygula'),
                    ),
                  ],
                ),
              ] else ...[
                // Aktif kupon gösterimi
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: AppColors.success.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: AppColors.success, width: 1),
                  ),
                  child: Row(
                    children: [
                      const Icon(
                        Icons.check_circle,
                        color: AppColors.success,
                        size: 20,
                      ),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Kampanya Uygulandı',
                              style: AppTypography.bodySmall.copyWith(
                                color: AppColors.success,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            Text(
                              state.cart.couponCode!.toUpperCase(),
                              style: AppTypography.bodyLarge.copyWith(
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                      ),
                      IconButton(
                        icon: const Icon(Icons.close, size: 20),
                        color: AppColors.error,
                        onPressed: () {
                          context.read<CartBloc>().add(RemoveCoupon());
                          _couponController.clear();
                        },
                        tooltip: 'Kampanyayı kaldır',
                      ),
                    ],
                  ),
                ),
              ],
            ],
          ),
        );
      },
    );
  }

  Widget _buildOrderSummarySection(AppLocalizations l10n) {
    return BlocBuilder<CartBloc, CartState>(
      builder: (context, state) {
        if (state is CartLoaded) {
          final cart = state.cart;
          return Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.white,
              borderRadius: BorderRadius.circular(12),
              border: Border.all(color: AppColors.textSecondary),
            ),
            child: Column(
              children: [
                _buildPriceRow(l10n.subtotal, cart.subtotal),
                _buildPriceRow(l10n.deliveryFee, cart.deliveryFee),
                if (cart.discountAmount != null && cart.discountAmount! > 0)
                  _buildPriceRow(
                    l10n.discount,
                    -(cart.discountAmount ?? 0.0),
                    isDiscount: true,
                  ),
                const Divider(),
                _buildPriceRow(l10n.total, cart.total, isTotal: true),
              ],
            ),
          );
        }

        return Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: AppColors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: AppColors.textSecondary),
          ),
          child: const Center(
            child: CircularProgressIndicator(
              valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
            ),
          ),
        );
      },
    );
  }

  Widget _buildPriceRow(
    String label,
    double amount, {
    bool isDiscount = false,
    bool isTotal = false,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: isTotal ? FontWeight.w600 : FontWeight.normal,
              color: isDiscount ? AppColors.success : AppColors.textPrimary,
            ),
          ),
          Text(
            '${isDiscount ? '-' : ''}₺${amount.toStringAsFixed(2)}',
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: isTotal ? FontWeight.bold : FontWeight.w600,
              color: isDiscount
                  ? AppColors.success
                  : (isTotal ? AppColors.primary : AppColors.textPrimary),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPlaceOrderButton(AppLocalizations l10n) {
    return BlocBuilder<OrderBloc, OrderState>(
      builder: (context, state) {
        final isLoading = state is OrderLoading;

        return SizedBox(
          width: double.infinity,
          child: ElevatedButton(
            onPressed: isLoading || _selectedAddress == null
                ? null
                : _placeOrder,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: AppColors.white,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
            child: isLoading
                ? const CircularProgressIndicator(
                    valueColor: AlwaysStoppedAnimation<Color>(AppColors.white),
                  )
                : Text(
                    l10n.placeOrder,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
          ),
        );
      },
    );
  }

  void _showAddressSelectionDialog(
    List<UserAddress> addresses,
    AppLocalizations l10n,
  ) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.selectAddress),
        content: SizedBox(
          width: double.maxFinite,
          child: ListView.builder(
            shrinkWrap: true,
            itemCount: addresses.length,
            itemBuilder: (context, index) {
              final address = addresses[index];
              final isSelected = _selectedAddress?.id == address.id;

              return ListTile(
                leading: Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: _getAddressTypeColor(address.type).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Icon(
                    _getAddressTypeIcon(address.type),
                    color: _getAddressTypeColor(address.type),
                    size: 20,
                  ),
                ),
                title: Text(address.title),
                subtitle: Text(address.fullAddress),
                trailing: isSelected
                    ? const Icon(Icons.check, color: AppColors.primary)
                    : null,
                onTap: () {
                  setState(() {
                    _selectedAddress = address;
                  });
                  Navigator.pop(context);
                },
              );
            },
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(l10n.cancel),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              Navigator.pushNamed(context, '/address-management');
            },
            child: Text(l10n.addAddress),
          ),
        ],
      ),
    );
  }

  void _placeOrder() {
    final l10n = AppLocalizations.of(context);
    // Address required
    if (_selectedAddress == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(l10n.selectAddress),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    // Get cart data
    final cartState = context.read<CartBloc>().state;
    if (cartState is! CartLoaded) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(l10n.somethingWentWrong),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    final cart = cartState.cart;

    // Basic validations: empty cart
    if (cart.items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(l10n.emptyCart),
          backgroundColor: AppColors.error,
        ),
      );
      return;
    }

    // Create order request
    final orderRequest = CreateOrderRequest(
      merchantId: cart.items.isNotEmpty ? cart.items.first.merchantId : '',
      deliveryAddressId: _selectedAddress!.id,
      paymentMethod: _selectedPaymentMethod,
      couponCode: cart.couponCode,
      notes: _notesController.text.isEmpty ? null : _notesController.text,
      items: cart.items.map((item) {
        return CreateOrderItemRequest(
          productId: item.productId,
          quantity: item.quantity,
          selectedVariantId: item.selectedVariantId,
          selectedOptionIds: item.selectedOptionIds,
        );
      }).toList(),
    );

    // Create order
    context.read<OrderBloc>().add(CreateOrder(orderRequest));
  }

  IconData _getAddressTypeIcon(AddressType type) {
    switch (type) {
      case AddressType.home:
        return Icons.home;
      case AddressType.work:
        return Icons.work;
      case AddressType.other:
        return Icons.location_on;
    }
  }

  Color _getAddressTypeColor(AddressType type) {
    switch (type) {
      case AddressType.home:
        return Colors.green;
      case AddressType.work:
        return Colors.blue;
      case AddressType.other:
        return Colors.orange;
    }
  }

  IconData _getPaymentMethodIcon(PaymentMethod method) {
    switch (method) {
      case PaymentMethod.cash:
        return Icons.money;
      case PaymentMethod.card:
        return Icons.credit_card;
      case PaymentMethod.online:
        return Icons.payment;
    }
  }

  String _getPaymentMethodDescription(
    PaymentMethod method,
    AppLocalizations l10n,
  ) {
    switch (method) {
      case PaymentMethod.cash:
        return l10n.cashPaymentDescription;
      case PaymentMethod.card:
        return l10n.cardPaymentDescription;
      case PaymentMethod.online:
        return l10n.onlinePaymentDescription;
    }
  }

  /// Merchant kapalıysa uyarı göster
  Widget _buildMerchantClosedWarning(
    AppLocalizations l10n,
    (String, TimeOfDay)? nextOpenTime,
  ) {
    return Container(
      margin: const EdgeInsets.only(bottom: 24),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.orange.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.orange.shade300, width: 2),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                Icons.warning_amber_rounded,
                color: Colors.orange.shade700,
                size: 28,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  'Bu işletme şu an kapalı',
                  style: AppTypography.bodyLarge.copyWith(
                    color: Colors.orange.shade900,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          if (nextOpenTime != null) ...[
            Text(
              'Açılış: ${nextOpenTime.$1} ${WorkingHoursHelper.formatTimeOfDay(nextOpenTime.$2)}',
              style: AppTypography.bodyMedium.copyWith(
                color: Colors.orange.shade800,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 16),
            // Gelecek tarihli sipariş seçeneği
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: Colors.orange.shade200),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Checkbox(
                        value: _scheduleForLater,
                        onChanged: (value) {
                          setState(() {
                            _scheduleForLater = value ?? false;
                            if (_scheduleForLater) {
                              // Set default scheduled time to next open time
                              final now = DateTime.now();
                              _scheduledTime = DateTime(
                                now.year,
                                now.month,
                                now.day,
                                nextOpenTime.$2.hour,
                                nextOpenTime.$2.minute,
                              );
                              // If it's today but already past that time, schedule for next week
                              if (_scheduledTime!.isBefore(now)) {
                                _scheduledTime = _scheduledTime!.add(
                                  const Duration(days: 1),
                                );
                              }
                            } else {
                              _scheduledTime = null;
                            }
                          });
                        },
                        activeColor: AppColors.primary,
                      ),
                      Expanded(
                        child: Text(
                          'Gelecek tarihe sipariş ver',
                          style: AppTypography.bodyMedium.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ),
                  if (_scheduleForLater && _scheduledTime != null) ...[
                    const SizedBox(height: 8),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: AppColors.primaryLight.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(6),
                      ),
                      child: Row(
                        children: [
                          Icon(
                            Icons.schedule,
                            color: AppColors.primary,
                            size: 20,
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'Teslimat: ${_formatScheduledTime(_scheduledTime!)}',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.primary,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      'Siparişiniz açılış saatinde hazırlanmaya başlayacak',
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                        fontStyle: FontStyle.italic,
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ] else ...[
            Text(
              'Bu işletme için çalışma saatleri henüz belirlenmemiş.',
              style: AppTypography.bodyMedium.copyWith(
                color: Colors.orange.shade800,
              ),
            ),
          ],
        ],
      ),
    );
  }

  /// Planlanmış zamanı formatla
  String _formatScheduledTime(DateTime dateTime) {
    final now = DateTime.now();
    final diff = dateTime.difference(now);

    String dateStr;
    if (diff.inDays == 0) {
      dateStr = 'Bugün';
    } else if (diff.inDays == 1) {
      dateStr = 'Yarın';
    } else {
      dateStr = '${dateTime.day}/${dateTime.month}/${dateTime.year}';
    }

    final timeStr =
        '${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';

    return '$dateStr $timeStr';
  }
}
