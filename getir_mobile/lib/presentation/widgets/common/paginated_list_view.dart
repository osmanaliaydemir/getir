import 'package:flutter/material.dart';

typedef ItemWidgetBuilder<T> =
    Widget Function(BuildContext context, T item, int index);

class PaginatedListView<T> extends StatefulWidget {
  final List<T> items;
  final ItemWidgetBuilder<T> itemBuilder;
  final Future<void> Function() onLoadMore;
  final bool hasMore;
  final EdgeInsetsGeometry padding;
  final ScrollController? controller;

  const PaginatedListView({
    super.key,
    required this.items,
    required this.itemBuilder,
    required this.onLoadMore,
    required this.hasMore,
    this.padding = const EdgeInsets.all(0),
    this.controller,
  });

  @override
  State<PaginatedListView<T>> createState() => _PaginatedListViewState<T>();
}

class _PaginatedListViewState<T> extends State<PaginatedListView<T>> {
  late final ScrollController _scrollController;
  bool _isLoadingMore = false;

  @override
  void initState() {
    super.initState();
    _scrollController = widget.controller ?? ScrollController();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    if (widget.controller == null) {
      _scrollController.dispose();
    } else {
      _scrollController.removeListener(_onScroll);
    }
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients || _isLoadingMore || !widget.hasMore) {
      return;
    }
    final threshold = 200.0; // px to bottom before loading more
    final maxScroll = _scrollController.position.maxScrollExtent;
    final current = _scrollController.position.pixels;
    if (maxScroll - current <= threshold) {
      _loadMore();
    }
  }

  Future<void> _loadMore() async {
    if (_isLoadingMore) return;
    setState(() => _isLoadingMore = true);
    try {
      await widget.onLoadMore();
    } finally {
      if (mounted) setState(() => _isLoadingMore = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return ListView.builder(
      controller: _scrollController,
      padding: widget.padding,
      itemCount: widget.items.length + (widget.hasMore ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= widget.items.length) {
          return const Padding(
            padding: EdgeInsets.symmetric(vertical: 16),
            child: Center(child: CircularProgressIndicator()),
          );
        }
        return widget.itemBuilder(context, widget.items[index], index);
      },
    );
  }
}

/*
import 'package:flutter/material.dart';
import '../../../core/models/pagination_model.dart';
import 'shimmer_loading.dart';

/// A paginated list view widget that handles lazy loading
class PaginatedListView<T> extends StatefulWidget {
  final PaginationModel<T> pagination;
  final Widget Function(BuildContext context, T item, int index) itemBuilder;
  final VoidCallback? onLoadMore;
  final VoidCallback? onRefresh;
  final Widget? emptyWidget;
  final Widget? loadingWidget;
  final Widget? errorWidget;
  final EdgeInsets? padding;
  final ScrollController? scrollController;
  final bool shrinkWrap;
  final ScrollPhysics? physics;
  final double? itemExtent;
  final int? itemCount;
  final Axis scrollDirection;
  final bool reverse;

  const PaginatedListView({
    super.key,
    required this.pagination,
    required this.itemBuilder,
    this.onLoadMore,
    this.onRefresh,
    this.emptyWidget,
    this.loadingWidget,
    this.errorWidget,
    this.padding,
    this.scrollController,
    this.shrinkWrap = false,
    this.physics,
    this.itemExtent,
    this.itemCount,
    this.scrollDirection = Axis.vertical,
    this.reverse = false,
  });

  @override
  State<PaginatedListView<T>> createState() => _PaginatedListViewState<T>();
}

class _PaginatedListViewState<T> extends State<PaginatedListView<T>> {
  late ScrollController _scrollController;
  bool _isLoadingMore = false;

  @override
  void initState() {
    super.initState();
    _scrollController = widget.scrollController ?? ScrollController();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    if (widget.scrollController == null) {
      _scrollController.dispose();
    }
    super.dispose();
  }

  void _onScroll() {
    if (_scrollController.position.pixels >=
        _scrollController.position.maxScrollExtent - 200) {
      _loadMore();
    }
  }

  void _loadMore() {
    if (!_isLoadingMore &&
        widget.pagination.hasNextPage &&
        !widget.pagination.isLoading &&
        widget.onLoadMore != null) {
      setState(() {
        _isLoadingMore = true;
      });
      widget.onLoadMore!();
      // Reset loading state after a delay
      Future.delayed(const Duration(milliseconds: 500), () {
        if (mounted) {
          setState(() {
            _isLoadingMore = false;
          });
        }
      });
    }
  }

  Future<void> _onRefresh() async {
    if (widget.onRefresh != null) {
      widget.onRefresh!();
    }
  }

  @override
  Widget build(BuildContext context) {
    if (widget.pagination.isEmpty && !widget.pagination.isLoading) {
      return widget.emptyWidget ?? _buildEmptyWidget();
    }

    return RefreshIndicator(
      onRefresh: _onRefresh,
      child: ListView.builder(
        controller: _scrollController,
        padding: widget.padding,
        shrinkWrap: widget.shrinkWrap,
        physics: widget.physics,
        itemExtent: widget.itemExtent,
        itemCount: _getItemCount(),
        scrollDirection: widget.scrollDirection,
        reverse: widget.reverse,
        itemBuilder: (context, index) {
          if (index < widget.pagination.items.length) {
            return widget.itemBuilder(
              context,
              widget.pagination.items[index],
              index,
            );
          } else if (index == widget.pagination.items.length) {
            return _buildLoadMoreWidget();
          } else {
            return const SizedBox.shrink();
          }
        },
      ),
    );
  }

  int _getItemCount() {
    int count = widget.pagination.items.length;
    if (widget.pagination.hasNextPage || _isLoadingMore) {
      count += 1; // Add space for loading indicator
    }
    return widget.itemCount ?? count;
  }

  Widget _buildEmptyWidget() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.inbox_outlined, size: 64, color: Colors.grey[400]),
            const SizedBox(height: 16),
            Text(
              'No items found',
              style: TextStyle(
                fontSize: 18,
                color: Colors.grey[600],
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Try refreshing or check back later',
              style: TextStyle(fontSize: 14, color: Colors.grey[500]),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildLoadMoreWidget() {
    if (widget.pagination.isLoading || _isLoadingMore) {
      return widget.loadingWidget ?? _buildDefaultLoadingWidget();
    }

    if (widget.pagination.hasNextPage) {
      return Padding(
        padding: const EdgeInsets.all(16),
        child: Center(
          child: TextButton(
            onPressed: _loadMore,
            child: const Text('Load More'),
          ),
        ),
      );
    }

    return const SizedBox.shrink();
  }

  Widget _buildDefaultLoadingWidget() {
    return const Padding(
      padding: EdgeInsets.all(16),
      child: Center(child: CircularProgressIndicator()),
    );
  }
}

/// A specialized paginated list for products
class PaginatedProductList extends StatelessWidget {
  final PaginationModel<dynamic> pagination;
  final VoidCallback? onLoadMore;
  final VoidCallback? onRefresh;
  final Function(dynamic product)? onProductTap;

  const PaginatedProductList({
    super.key,
    required this.pagination,
    this.onLoadMore,
    this.onRefresh,
    this.onProductTap,
  });

  @override
  Widget build(BuildContext context) {
    return PaginatedListView(
      pagination: pagination,
      onLoadMore: onLoadMore,
      onRefresh: onRefresh,
      itemBuilder: (context, product, index) {
        return _buildProductCard(context, product);
      },
      loadingWidget: const ProductCardShimmer(),
      emptyWidget: _buildEmptyProducts(),
    );
  }

  Widget _buildProductCard(BuildContext context, dynamic product) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
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
      child: InkWell(
        onTap: () => onProductTap?.call(product),
        borderRadius: BorderRadius.circular(12),
        child: Row(
          children: [
            // Product image
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: OptimizedImage(
                imageUrl: product.imageUrl ?? '',
                width: 80,
                height: 80,
              ),
            ),
            const SizedBox(width: 16),
            // Product info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    product.name ?? '',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w600,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    product.description ?? '',
                    style: TextStyle(fontSize: 14, color: Colors.grey[600]),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '₺${product.price?.toStringAsFixed(2) ?? '0.00'}',
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Colors.green,
                    ),
                  ),
                ],
              ),
            ),
            // Add to cart button
            IconButton(
              onPressed: () {
                // TODO: Add to cart functionality
              },
              icon: const Icon(Icons.add_shopping_cart, color: Colors.green),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyProducts() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.shopping_bag_outlined,
              size: 64,
              color: Colors.grey[400],
            ),
            const SizedBox(height: 16),
            Text(
              'No products found',
              style: TextStyle(
                fontSize: 18,
                color: Colors.grey[600],
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Try refreshing or check back later',
              style: TextStyle(fontSize: 14, color: Colors.grey[500]),
            ),
          ],
        ),
      ),
    );
  }
}

/// A specialized paginated list for merchants
class PaginatedMerchantList extends StatelessWidget {
  final PaginationModel<dynamic> pagination;
  final VoidCallback? onLoadMore;
  final VoidCallback? onRefresh;
  final Function(dynamic merchant)? onMerchantTap;

  const PaginatedMerchantList({
    super.key,
    required this.pagination,
    this.onLoadMore,
    this.onRefresh,
    this.onMerchantTap,
  });

  @override
  Widget build(BuildContext context) {
    return PaginatedListView(
      pagination: pagination,
      onLoadMore: onLoadMore,
      onRefresh: onRefresh,
      itemBuilder: (context, merchant, index) {
        return _buildMerchantCard(context, merchant);
      },
      loadingWidget: const MerchantCardShimmer(),
      emptyWidget: _buildEmptyMerchants(),
    );
  }

  Widget _buildMerchantCard(BuildContext context, dynamic merchant) {
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
      child: InkWell(
        onTap: () => onMerchantTap?.call(merchant),
        borderRadius: BorderRadius.circular(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Merchant image
            ClipRRect(
              borderRadius: const BorderRadius.only(
                topLeft: Radius.circular(12),
                topRight: Radius.circular(12),
              ),
              child: OptimizedImage(
                imageUrl: merchant.imageUrl ?? '',
                height: 120,
                width: double.infinity,
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    merchant.name ?? '',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    merchant.description ?? '',
                    style: TextStyle(fontSize: 14, color: Colors.grey[600]),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Icon(Icons.star, size: 16, color: Colors.amber[600]),
                      const SizedBox(width: 4),
                      Text(
                        merchant.rating?.toStringAsFixed(1) ?? '0.0',
                        style: const TextStyle(
                          fontSize: 14,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                      const SizedBox(width: 16),
                      Icon(
                        Icons.access_time,
                        size: 16,
                        color: Colors.grey[600],
                      ),
                      const SizedBox(width: 4),
                      Text(
                        merchant.deliveryTime ?? 'N/A',
                        style: TextStyle(fontSize: 14, color: Colors.grey[600]),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyMerchants() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.store_outlined, size: 64, color: Colors.grey[400]),
            const SizedBox(height: 16),
            Text(
              'No merchants found',
              style: TextStyle(
                fontSize: 18,
                color: Colors.grey[600],
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Try refreshing or check back later',
              style: TextStyle(fontSize: 14, color: Colors.grey[500]),
            ),
          ],
        ),
      ),
    );
  }
}
*/
