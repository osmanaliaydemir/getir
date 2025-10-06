import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../core/navigation/app_navigation.dart';
import '../../bloc/order/order_bloc.dart';
import '../../../domain/entities/order.dart';

class OrderDetailPage extends StatefulWidget {
  final String orderId;
  
  const OrderDetailPage({
    super.key,
    required this.orderId,
  });

  @override
  State<OrderDetailPage> createState() => _OrderDetailPageState();
}

class _OrderDetailPageState extends State<OrderDetailPage> {
  @override
  void initState() {
    super.initState();
    // Load order details
    context.read<OrderBloc>().add(LoadOrderById(widget.orderId));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.orderDetails),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.share),
            onPressed: () {
              // TODO: Share order details
            },
          ),
        ],
      ),
      body: BlocBuilder<OrderBloc, OrderState>(
        builder: (context, state) {
          if (state is OrderLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is OrderError) {
            return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
                  Icon(
                    Icons.error_outline,
                    size: 64,
                    color: AppColors.error,
                  ),
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
                      context.read<OrderBloc>().add(LoadOrderById(widget.orderId));
                    },
                    child: Text(l10n.retry),
                  ),
                ],
              ),
            );
          }

          if (state is OrderLoaded) {
            final order = state.order;
            return _buildOrderDetail(order, l10n);
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildOrderDetail(dynamic order, AppLocalizations l10n) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Order status card
          _buildStatusCard(order, l10n),
          
          const SizedBox(height: 16),
          
          // Order info card
          _buildOrderInfoCard(order, l10n),
          
          const SizedBox(height: 16),
          
          // Delivery address card
          _buildDeliveryAddressCard(order, l10n),
          
          const SizedBox(height: 16),
          
          // Order items card
          _buildOrderItemsCard(order, l10n),
          
          const SizedBox(height: 16),
          
          // Payment info card
          _buildPaymentInfoCard(order, l10n),
          
          const SizedBox(height: 16),
          
          // Action buttons
          _buildActionButtons(order, l10n),
          
          const SizedBox(height: 32),
        ],
      ),
    );
  }

  Widget _buildStatusCard(dynamic order, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          // Status icon
          Container(
            width: 80,
            height: 80,
            decoration: BoxDecoration(
              color: _getStatusColor(order.status).withOpacity(0.1),
              shape: BoxShape.circle,
            ),
            child: Icon(
              _getStatusIcon(order.status),
              color: _getStatusColor(order.status),
              size: 40,
            ),
          ),
          
          const SizedBox(height: 16),
          
          // Status text
          Text(
            _getStatusText(order.status, l10n),
            style: AppTypography.headlineSmall.copyWith(
              fontWeight: FontWeight.bold,
              color: _getStatusColor(order.status),
            ),
          ),
          
          const SizedBox(height: 8),
          
          // Status description
          Text(
            _getStatusDescription(order.status, l10n),
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
            textAlign: TextAlign.center,
          ),
          
          const SizedBox(height: 16),
          
          // Progress indicator
          _buildProgressIndicator(order.status),
        ],
      ),
    );
  }

  Widget _buildOrderInfoCard(dynamic order, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            l10n.orderInformation,
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          _buildInfoRow(l10n.orderNumber, order.id, Icons.receipt_long),
          _buildInfoRow(l10n.orderDate, _formatDateTime(order.createdAt), Icons.calendar_today),
          _buildInfoRow(l10n.estimatedDelivery, _formatDateTime(order.estimatedDeliveryTime), Icons.access_time),
        ],
      ),
    );
  }

  Widget _buildDeliveryAddressCard(dynamic order, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                Icons.location_on,
              color: AppColors.primary,
                size: 20,
              ),
              const SizedBox(width: 8),
              Text(
                l10n.deliveryAddress,
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            order.deliveryAddress,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildOrderItemsCard(dynamic order, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            l10n.orderItems,
            style: AppTypography.bodyLarge.copyWith(
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 12),
          ...order.items.map<Widget>((item) => _buildOrderItem(item, l10n)).toList(),
          const Divider(),
          _buildPriceRow(l10n.subtotal, order.subtotal),
          _buildPriceRow(l10n.deliveryFee, order.deliveryFee),
          if (order.discountAmount > 0)
            _buildPriceRow(
              l10n.discount,
              -order.discountAmount,
              isDiscount: true,
            ),
          _buildPriceRow(
            l10n.total,
            order.totalAmount,
            isTotal: true,
          ),
        ],
      ),
    );
  }

  Widget _buildPaymentInfoCard(dynamic order, AppLocalizations l10n) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
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
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(
                _getPaymentMethodIcon(order.paymentMethod),
                color: AppColors.primary,
                size: 20,
              ),
              const SizedBox(width: 8),
              Text(
                l10n.paymentMethod,
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
            Text(
            order.paymentMethod.displayName,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
            ),
            const SizedBox(height: 8),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            decoration: BoxDecoration(
              color: _getPaymentStatusColor(order.paymentStatus).withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text(
              _getPaymentStatusText(order.paymentStatus, l10n),
              style: AppTypography.bodySmall.copyWith(
                color: _getPaymentStatusColor(order.paymentStatus),
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildActionButtons(dynamic order, AppLocalizations l10n) {
    return Column(
      children: [
        if (order.status == OrderStatus.confirmed || order.status == OrderStatus.preparing)
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: () {
                AppNavigation.goToOrderTracking(context, order.id);
              },
              icon: const Icon(Icons.track_changes),
              label: Text(l10n.trackOrder),
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: AppColors.white,
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
          ),
        
        if (order.status == OrderStatus.confirmed)
          const SizedBox(height: 12),
        
        if (order.status == OrderStatus.confirmed)
          SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: () {
                _showCancelOrderDialog(context, order, l10n);
              },
              icon: const Icon(Icons.cancel_outlined),
              label: Text(l10n.cancelOrder),
              style: OutlinedButton.styleFrom(
                foregroundColor: AppColors.error,
                side: const BorderSide(color: AppColors.error),
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
            ),
          ),
      ],
    );
  }

  Widget _buildProgressIndicator(dynamic status) {
    final steps = [
      OrderStatus.pending,
      OrderStatus.confirmed,
      OrderStatus.preparing,
      OrderStatus.onTheWay,
      OrderStatus.delivered,
    ];
    
    final currentIndex = steps.indexOf(status);
    
    return Row(
      children: steps.asMap().entries.map((entry) {
        final index = entry.key;
        final isCompleted = index <= currentIndex;
        
        return Expanded(
          child: Row(
            children: [
              Container(
                width: 24,
                height: 24,
                decoration: BoxDecoration(
                  color: isCompleted ? AppColors.primary : AppColors.textSecondary,
                  shape: BoxShape.circle,
                ),
                child: isCompleted
                    ? const Icon(
                        Icons.check,
                        color: AppColors.white,
                        size: 16,
                      )
                    : null,
              ),
              if (index < steps.length - 1)
                Expanded(
                  child: Container(
                    height: 2,
                    color: isCompleted ? AppColors.primary : AppColors.textSecondary,
                  ),
                ),
            ],
          ),
        );
      }).toList(),
    );
  }

  Widget _buildOrderItem(dynamic item, AppLocalizations l10n) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          // Product image
          ClipRRect(
            borderRadius: BorderRadius.circular(6),
            child: Container(
              width: 50,
              height: 50,
              color: Colors.grey[200],
              child: item.productImageUrl != null && item.productImageUrl!.isNotEmpty
                  ? Image.network(
                      item.productImageUrl!,
                      fit: BoxFit.cover,
                      errorBuilder: (context, error, stackTrace) {
                        return const Icon(
                          Icons.image,
                          size: 20,
                          color: Colors.grey,
                        );
                      },
                    )
                  : const Icon(
                      Icons.image,
                      size: 20,
                      color: Colors.grey,
                    ),
            ),
          ),
          const SizedBox(width: 12),
          // Product info
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.productName,
                  style: AppTypography.bodyMedium.copyWith(
                    fontWeight: FontWeight.w500,
                  ),
                ),
                if (item.selectedVariantName != null) ...[
                  const SizedBox(height: 2),
                  Text(
                    item.selectedVariantName!,
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
                if (item.selectedOptionNames.isNotEmpty) ...[
                  const SizedBox(height: 2),
                  Text(
                    item.selectedOptionNames.join(', '),
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                ],
              ],
            ),
          ),
          // Quantity and price
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                '${item.quantity}x',
                style: AppTypography.bodySmall.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                '₺${item.totalPrice.toStringAsFixed(2)}',
                style: AppTypography.bodyMedium.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value, IconData icon) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          Icon(
            icon,
            color: AppColors.primary,
            size: 16,
          ),
          const SizedBox(width: 8),
          Text(
            label,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
          const Spacer(),
          Text(
            value,
            style: AppTypography.bodyMedium.copyWith(
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildPriceRow(String label, double amount, {bool isDiscount = false, bool isTotal = false}) {
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
              color: isDiscount ? AppColors.success : (isTotal ? AppColors.primary : AppColors.textPrimary),
            ),
            ),
          ],
        ),
    );
  }

  void _showCancelOrderDialog(BuildContext context, dynamic order, AppLocalizations l10n) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.cancelOrder),
        content: Text(l10n.cancelOrderMessage),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text(l10n.cancel),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              context.read<OrderBloc>().add(CancelOrder(order.id));
            },
            child: Text(
              l10n.cancelOrder,
              style: const TextStyle(color: AppColors.error),
            ),
          ),
        ],
      ),
    );
  }

  Color _getStatusColor(dynamic status) {
    switch (status) {
      case OrderStatus.pending:
        return Colors.orange;
      case OrderStatus.confirmed:
        return Colors.blue;
      case OrderStatus.preparing:
        return Colors.purple;
      case OrderStatus.onTheWay:
        return Colors.indigo;
      case OrderStatus.delivered:
        return Colors.green;
      case OrderStatus.cancelled:
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  IconData _getStatusIcon(dynamic status) {
    switch (status) {
      case OrderStatus.pending:
        return Icons.pending;
      case OrderStatus.confirmed:
        return Icons.check_circle;
      case OrderStatus.preparing:
        return Icons.restaurant;
      case OrderStatus.onTheWay:
        return Icons.delivery_dining;
      case OrderStatus.delivered:
        return Icons.check_circle_outline;
      case OrderStatus.cancelled:
        return Icons.cancel;
      default:
        return Icons.help;
    }
  }

  String _getStatusText(dynamic status, AppLocalizations l10n) {
    switch (status) {
      case OrderStatus.pending:
        return l10n.pending;
      case OrderStatus.confirmed:
        return l10n.confirmed;
      case OrderStatus.preparing:
        return l10n.preparing;
      case OrderStatus.onTheWay:
        return l10n.onTheWay;
      case OrderStatus.delivered:
        return l10n.delivered;
      case OrderStatus.cancelled:
        return l10n.cancelled;
      default:
        return l10n.unknown;
    }
  }

  String _getStatusDescription(dynamic status, AppLocalizations l10n) {
    switch (status) {
      case OrderStatus.pending:
        return l10n.pendingDescription;
      case OrderStatus.confirmed:
        return l10n.confirmedDescription;
      case OrderStatus.preparing:
        return l10n.preparingDescription;
      case OrderStatus.onTheWay:
        return l10n.onTheWayDescription;
      case OrderStatus.delivered:
        return l10n.deliveredDescription;
      case OrderStatus.cancelled:
        return l10n.cancelledDescription;
      default:
        return l10n.unknownDescription;
    }
  }

  IconData _getPaymentMethodIcon(dynamic method) {
    switch (method) {
      case PaymentMethod.cash:
        return Icons.money;
      case PaymentMethod.card:
        return Icons.credit_card;
      case PaymentMethod.online:
        return Icons.payment;
      default:
        return Icons.payment;
    }
  }

  Color _getPaymentStatusColor(dynamic status) {
    switch (status) {
      case PaymentStatus.pending:
        return Colors.orange;
      case PaymentStatus.paid:
        return Colors.green;
      case PaymentStatus.failed:
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  String _getPaymentStatusText(dynamic status, AppLocalizations l10n) {
    switch (status) {
      case PaymentStatus.pending:
        return l10n.paymentPending;
      case PaymentStatus.paid:
        return l10n.paymentCompleted;
      case PaymentStatus.failed:
        return l10n.paymentFailed;
      default:
        return l10n.unknown;
    }
  }

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';
  }
}