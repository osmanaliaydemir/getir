import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../widgets/order/order_card_skeleton.dart';
import '../../../core/widgets/error_state_widget.dart';
import '../../../core/navigation/app_navigation.dart';
import '../../bloc/order/order_bloc.dart';
import '../../../domain/entities/order.dart';

class OrdersPage extends StatefulWidget {
  const OrdersPage({super.key});

  @override
  State<OrdersPage> createState() => _OrdersPageState();
}

class _OrdersPageState extends State<OrdersPage> with TickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);

    // Load user orders
    context.read<OrderBloc>().add(LoadUserOrders());
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.orders),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
        bottom: TabBar(
          controller: _tabController,
          labelColor: AppColors.white,
          unselectedLabelColor: AppColors.white.withOpacity(0.7),
          indicatorColor: AppColors.white,
          tabs: [
            Tab(text: l10n.all),
            Tab(text: l10n.active),
            Tab(text: l10n.completed),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _buildOrdersList(OrderStatusFilter.all, l10n),
          _buildOrdersList(OrderStatusFilter.active, l10n),
          _buildOrdersList(OrderStatusFilter.completed, l10n),
        ],
      ),
    );
  }

  Widget _buildOrdersList(OrderStatusFilter filter, AppLocalizations l10n) {
    return BlocBuilder<OrderBloc, OrderState>(
      builder: (context, state) {
        if (state is OrderLoading) {
          return const OrderListSkeleton(itemCount: 5);
        }

        if (state is OrderError) {
          return ErrorStateWidget(
            errorType: _getErrorTypeFromMessage(state.message),
            customMessage: _getUserFriendlyErrorMessage(state.message),
            onRetry: () {
              context.read<OrderBloc>().add(LoadUserOrders());
            },
          );
        }

        if (state is OrdersLoaded) {
          final orders = _filterOrders(state.orders, filter);

          if (orders.isEmpty) {
            return _buildEmptyState(filter, l10n);
          }

          return RefreshIndicator(
            onRefresh: () async {
              context.read<OrderBloc>().add(LoadUserOrders());
            },
            child: ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: orders.length,
              itemBuilder: (context, index) {
                final order = orders[index];
                return _buildOrderCard(order, l10n);
              },
            ),
          );
        }

        return const SizedBox.shrink();
      },
    );
  }

  Widget _buildEmptyState(OrderStatusFilter filter, AppLocalizations l10n) {
    String title;
    String message;
    IconData icon;

    switch (filter) {
      case OrderStatusFilter.all:
        title = l10n.noOrdersFound;
        message = l10n.noOrdersMessage;
        icon = Icons.receipt_long_outlined;
        break;
      case OrderStatusFilter.active:
        title = l10n.noActiveOrders;
        message = l10n.noActiveOrdersMessage;
        icon = Icons.pending_outlined;
        break;
      case OrderStatusFilter.completed:
        title = l10n.noCompletedOrders;
        message = l10n.noCompletedOrdersMessage;
        icon = Icons.check_circle_outline;
        break;
    }

    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 80, color: AppColors.textSecondary),
          const SizedBox(height: 24),
          Text(
            title,
            style: AppTypography.headlineSmall.copyWith(
              color: AppColors.textSecondary,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            message,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 32),
          ElevatedButton(
            onPressed: () {
              AppNavigation.goToHome(context);
            },
            child: Text(l10n.startShopping),
          ),
        ],
      ),
    );
  }

  Widget _buildOrderCard(dynamic order, AppLocalizations l10n) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
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
      child: InkWell(
        onTap: () {
          AppNavigation.goToOrderDetail(context, order.id);
        },
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Order header
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '${l10n.orderNumber}: ${order.id}',
                    style: AppTypography.bodyLarge.copyWith(
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 8,
                      vertical: 4,
                    ),
                    decoration: BoxDecoration(
                      color: _getStatusColor(order.status).withOpacity(0.1),
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      _getStatusText(order.status, l10n),
                      style: AppTypography.bodySmall.copyWith(
                        color: _getStatusColor(order.status),
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 12),

              // Merchant info
              Row(
                children: [
                  ClipRRect(
                    borderRadius: BorderRadius.circular(6),
                    child: Container(
                      width: 40,
                      height: 40,
                      color: Colors.grey[200],
                      child:
                          order.merchantLogoUrl != null &&
                              order.merchantLogoUrl!.isNotEmpty
                          ? Image.network(
                              order.merchantLogoUrl!,
                              fit: BoxFit.cover,
                              errorBuilder: (context, error, stackTrace) {
                                return const Icon(
                                  Icons.store,
                                  size: 20,
                                  color: Colors.grey,
                                );
                              },
                            )
                          : const Icon(
                              Icons.store,
                              size: 20,
                              color: Colors.grey,
                            ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          order.merchantName,
                          style: AppTypography.bodyMedium.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                        const SizedBox(height: 2),
                        Text(
                          '${order.items.length} ${l10n.items}',
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 12),

              // Order details
              Row(
                children: [
                  Icon(
                    Icons.access_time,
                    size: 16,
                    color: AppColors.textSecondary,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    _formatDateTime(order.createdAt),
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                  const Spacer(),
                  Text(
                    '₺${order.totalAmount.toStringAsFixed(2)}',
                    style: AppTypography.bodyLarge.copyWith(
                      color: AppColors.primary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 12),

              // Action buttons
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: () {
                        AppNavigation.goToOrderDetail(context, order.id);
                      },
                      style: OutlinedButton.styleFrom(
                        foregroundColor: AppColors.primary,
                        side: const BorderSide(color: AppColors.primary),
                        padding: const EdgeInsets.symmetric(vertical: 8),
                      ),
                      child: Text(l10n.viewDetails),
                    ),
                  ),
                  const SizedBox(width: 12),
                  if (order.status == OrderStatus.confirmed ||
                      order.status == OrderStatus.preparing)
                    Expanded(
                      child: ElevatedButton(
                        onPressed: () {
                          AppNavigation.goToOrderTracking(context, order.id);
                        },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppColors.primary,
                          foregroundColor: AppColors.white,
                          padding: const EdgeInsets.symmetric(vertical: 8),
                        ),
                        child: Text(l10n.trackOrder),
                      ),
                    ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  List<dynamic> _filterOrders(List<dynamic> orders, OrderStatusFilter filter) {
    switch (filter) {
      case OrderStatusFilter.all:
        return orders;
      case OrderStatusFilter.active:
        return orders
            .where(
              (order) =>
                  order.status == OrderStatus.confirmed ||
                  order.status == OrderStatus.preparing ||
                  order.status == OrderStatus.onTheWay,
            )
            .toList();
      case OrderStatusFilter.completed:
        return orders
            .where(
              (order) =>
                  order.status == OrderStatus.delivered ||
                  order.status == OrderStatus.cancelled,
            )
            .toList();
    }
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

  String _formatDateTime(DateTime dateTime) {
    return '${dateTime.day}/${dateTime.month}/${dateTime.year} ${dateTime.hour.toString().padLeft(2, '0')}:${dateTime.minute.toString().padLeft(2, '0')}';
  }
}

enum OrderStatusFilter { all, active, completed }

extension _OrdersPageExtension on _OrdersPageState {
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

  String _getUserFriendlyErrorMessage(String message) {
    final lowerMessage = message.toLowerCase();

    if (lowerMessage.contains('network') ||
        lowerMessage.contains('connection') ||
        lowerMessage.contains('internet') ||
        lowerMessage.contains('bağlantı')) {
      return 'İnternet bağlantınızı kontrol edin ve tekrar deneyin.';
    } else if (lowerMessage.contains('500') ||
        lowerMessage.contains('502') ||
        lowerMessage.contains('503') ||
        lowerMessage.contains('server') ||
        lowerMessage.contains('sunucu')) {
      return 'Sunucu ile bağlantı kurulamadı. Lütfen daha sonra tekrar deneyin.';
    } else if (lowerMessage.contains('404') ||
        lowerMessage.contains('not found') ||
        lowerMessage.contains('bulunamadı')) {
      return 'Siparişleriniz bulunamadı. Lütfen tekrar deneyin.';
    } else if (lowerMessage.contains('401') ||
        lowerMessage.contains('403') ||
        lowerMessage.contains('unauthorized') ||
        lowerMessage.contains('yetkisiz')) {
      return 'Bu işlemi gerçekleştirmek için giriş yapmanız gerekiyor.';
    } else if (lowerMessage.contains('timeout')) {
      return 'İşlem zaman aşımına uğradı. Lütfen tekrar deneyin.';
    }

    return 'Siparişleriniz yüklenirken bir hata oluştu. Lütfen tekrar deneyin.';
  }
}
