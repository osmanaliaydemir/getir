import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../../core/cubits/language/language_cubit.dart';
import '../../widgets/common/language_selector.dart';
import '../../bloc/merchant/merchant_bloc.dart';
import '../../bloc/product/product_bloc.dart';
import '../../bloc/working_hours/working_hours_bloc.dart';
import '../../bloc/working_hours/working_hours_event.dart';
import '../../bloc/working_hours/working_hours_state.dart';
import '../../../domain/entities/working_hours.dart';
import 'product_list_page.dart';

class MerchantDetailPage extends StatefulWidget {
  final String merchantId;

  const MerchantDetailPage({super.key, required this.merchantId});

  @override
  State<MerchantDetailPage> createState() => _MerchantDetailPageState();
}

class _MerchantDetailPageState extends State<MerchantDetailPage>
    with TickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);

    // Load merchant details
    context.read<MerchantBloc>().add(LoadMerchantById(widget.merchantId));
    // Load merchant products
    context.read<ProductBloc>().add(LoadProductsByMerchant(widget.merchantId));
    // Load working hours
    context.read<WorkingHoursBloc>().add(LoadWorkingHours(widget.merchantId));
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
      body: BlocBuilder<MerchantBloc, MerchantState>(
        builder: (context, state) {
          if (state is MerchantLoading) {
            return const Center(
              child: CircularProgressIndicator(
                valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
              ),
            );
          }

          if (state is MerchantError) {
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
                      context.read<MerchantBloc>().add(
                        LoadMerchantById(widget.merchantId),
                      );
                    },
                    child: Text(l10n.retry),
                  ),
                ],
              ),
            );
          }

          if (state is MerchantLoaded) {
            final merchant = state.merchant;
            return _buildMerchantDetail(merchant, l10n);
          }

          return const SizedBox.shrink();
        },
      ),
    );
  }

  Widget _buildMerchantDetail(dynamic merchant, AppLocalizations l10n) {
    return NestedScrollView(
      headerSliverBuilder: (context, innerBoxIsScrolled) {
        return [
          SliverAppBar(
            expandedHeight: 200,
            floating: false,
            pinned: true,
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
            flexibleSpace: FlexibleSpaceBar(
              background: Stack(
                fit: StackFit.expand,
                children: [
                  // Cover Image
                  Container(
                    decoration: BoxDecoration(
                      image: DecorationImage(
                        image: NetworkImage(merchant.coverImageUrl ?? ''),
                        fit: BoxFit.cover,
                      ),
                    ),
                  ),
                  // Gradient overlay
                  Container(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topCenter,
                        end: Alignment.bottomCenter,
                        colors: [
                          Colors.transparent,
                          Colors.black.withOpacity(0.7),
                        ],
                      ),
                    ),
                  ),
                  // Merchant info
                  Positioned(
                    bottom: 16,
                    left: 16,
                    right: 16,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          merchant.name ?? '',
                          style: AppTypography.headlineMedium.copyWith(
                            color: AppColors.white,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            Icon(Icons.star, color: Colors.amber, size: 16),
                            const SizedBox(width: 4),
                            Text(
                              '${merchant.rating ?? 0.0}',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.white,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const SizedBox(width: 8),
                            Text(
                              '(${merchant.reviewCount ?? 0} ${l10n.reviews})',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.white.withOpacity(0.8),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 4),
                        Row(
                          children: [
                            Icon(
                              Icons.access_time,
                              color: AppColors.white.withOpacity(0.8),
                              size: 16,
                            ),
                            const SizedBox(width: 4),
                            Text(
                              '${merchant.estimatedDeliveryTime ?? 30} ${l10n.minutes}',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.white.withOpacity(0.8),
                              ),
                            ),
                            const SizedBox(width: 16),
                            Icon(
                              Icons.delivery_dining,
                              color: AppColors.white.withOpacity(0.8),
                              size: 16,
                            ),
                            const SizedBox(width: 4),
                            Text(
                              '₺${merchant.deliveryFee ?? 0.0}',
                              style: AppTypography.bodyMedium.copyWith(
                                color: AppColors.white.withOpacity(0.8),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ];
      },
      body: Column(
        children: [
          // Merchant status and info
          Container(
            padding: const EdgeInsets.all(16),
            color: AppColors.white,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Status indicator
                Row(
                  children: [
                    Container(
                      width: 8,
                      height: 8,
                      decoration: BoxDecoration(
                        color: merchant.isOpen == true
                            ? Colors.green
                            : Colors.red,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      merchant.isOpen == true ? l10n.open : l10n.closed,
                      style: AppTypography.bodyMedium.copyWith(
                        color: merchant.isOpen == true
                            ? Colors.green
                            : Colors.red,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                // Description
                if (merchant.description != null)
                  Text(
                    merchant.description,
                    style: AppTypography.bodyMedium.copyWith(
                      color: AppColors.textSecondary,
                    ),
                  ),
                const SizedBox(height: 12),
                // Categories
                if (merchant.categories != null &&
                    merchant.categories.isNotEmpty)
                  Wrap(
                    spacing: 8,
                    runSpacing: 4,
                    children: merchant.categories.map((category) {
                      return Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 8,
                          vertical: 4,
                        ),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Text(
                          category,
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.primary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      );
                    }).toList(),
                  ),
              ],
            ),
          ),
          // Tab bar
          Container(
            color: AppColors.white,
            child: TabBar(
              controller: _tabController,
              labelColor: AppColors.primary,
              unselectedLabelColor: AppColors.textSecondary,
              indicatorColor: AppColors.primary,
              tabs: [
                Tab(text: l10n.popularProducts),
                Tab(text: l10n.contactInfo),
              ],
            ),
          ),
          // Tab content
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                // Products tab
                ProductListPage(merchantId: widget.merchantId),
                // Info tab
                _buildInfoTab(merchant, l10n),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoTab(dynamic merchant, AppLocalizations l10n) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Working hours - BLoC Integration
          BlocBuilder<WorkingHoursBloc, WorkingHoursState>(
            builder: (context, whState) {
              return _buildInfoSection(
                title: l10n.workingHours,
                child: _buildWorkingHoursContent(whState),
              );
            },
          ),
          const SizedBox(height: 24),
          // Contact info
          _buildInfoSection(
            title: l10n.contactInfo,
            child: Column(
              children: [
                if (merchant.address != null)
                  _buildInfoRow(
                    icon: Icons.location_on,
                    label: l10n.address,
                    value: merchant.address,
                  ),
                if (merchant.phoneNumber != null)
                  _buildInfoRow(
                    icon: Icons.phone,
                    label: l10n.phone,
                    value: merchant.phoneNumber,
                  ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          // Delivery info
          _buildInfoSection(
            title: l10n.deliveryInfo,
            child: Column(
              children: [
                _buildInfoRow(
                  icon: Icons.delivery_dining,
                  label: l10n.deliveryFee,
                  value: '₺${merchant.deliveryFee ?? 0.0}',
                ),
                _buildInfoRow(
                  icon: Icons.access_time,
                  label: l10n.estimatedDeliveryTime,
                  value:
                      '${merchant.estimatedDeliveryTime ?? 30} ${l10n.minutes}',
                ),
                _buildInfoRow(
                  icon: Icons.shopping_cart,
                  label: l10n.minimumOrderAmount,
                  value: '₺${merchant.minimumOrderAmount ?? 0.0}',
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoSection({required String title, required Widget child}) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: AppTypography.headlineSmall.copyWith(
            fontWeight: FontWeight.bold,
          ),
        ),
        const SizedBox(height: 12),
        Container(
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
          child: child,
        ),
      ],
    );
  }

  Widget _buildInfoRow({
    required IconData icon,
    required String label,
    required String value,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          Icon(icon, color: AppColors.primary, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              label,
              style: AppTypography.bodyMedium.copyWith(
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Text(
            value,
            style: AppTypography.bodyMedium.copyWith(
              color: AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }

  /// Working hours content builder based on BLoC state
  Widget _buildWorkingHoursContent(WorkingHoursState state) {
    if (state is WorkingHoursLoading) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(16.0),
          child: CircularProgressIndicator(
            valueColor: AlwaysStoppedAnimation<Color>(AppColors.primary),
          ),
        ),
      );
    }

    if (state is WorkingHoursError) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: 8.0),
        child: Text(
          'Çalışma saatleri yüklenemedi',
          style: AppTypography.bodyMedium.copyWith(
            color: AppColors.error,
            fontStyle: FontStyle.italic,
          ),
        ),
      );
    }

    if (state is WorkingHoursNotFound) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: 8.0),
        child: Text(
          'Çalışma saatleri henüz belirlenmemiş',
          style: AppTypography.bodyMedium.copyWith(
            color: AppColors.textSecondary,
            fontStyle: FontStyle.italic,
          ),
        ),
      );
    }

    if (state is WorkingHoursLoaded) {
      final workingHours = state.workingHours;
      final today = WorkingHoursHelper.getTodayWorkingHours(workingHours);

      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Current status badge
          if (today != null)
            Container(
              margin: const EdgeInsets.only(bottom: 16),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: state.isOpen ? Colors.green.shade50 : Colors.red.shade50,
                borderRadius: BorderRadius.circular(8),
                border: Border.all(
                  color: state.isOpen
                      ? Colors.green.shade200
                      : Colors.red.shade200,
                  width: 1,
                ),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Container(
                    width: 8,
                    height: 8,
                    decoration: BoxDecoration(
                      color: state.isOpen ? Colors.green : Colors.red,
                      shape: BoxShape.circle,
                    ),
                  ),
                  const SizedBox(width: 8),
                  Text(
                    state.isOpen ? 'Şu an açık' : 'Şu an kapalı',
                    style: AppTypography.bodyMedium.copyWith(
                      color: state.isOpen
                          ? Colors.green.shade800
                          : Colors.red.shade800,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  if (!state.isOpen && state.nextOpenTime != null) ...[
                    const SizedBox(width: 8),
                    Text(
                      '• Açılış: ${state.nextOpenTime!.$1} ${WorkingHoursHelper.formatTimeOfDay(state.nextOpenTime!.$2)}',
                      style: AppTypography.bodySmall.copyWith(
                        color: Colors.red.shade700,
                      ),
                    ),
                  ],
                ],
              ),
            ),

          // 7 days working hours list
          ...workingHours.map((wh) {
            final isToday = today != null && wh.dayOfWeek == today.dayOfWeek;
            return Container(
              margin: const EdgeInsets.only(bottom: 8),
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              decoration: BoxDecoration(
                color: isToday
                    ? AppColors.primaryLight.withOpacity(0.1)
                    : Colors.transparent,
                borderRadius: BorderRadius.circular(6),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  // Day name
                  Row(
                    children: [
                      Text(
                        wh.getDayName(),
                        style: AppTypography.bodyMedium.copyWith(
                          fontWeight: isToday
                              ? FontWeight.w600
                              : FontWeight.w500,
                          color: isToday
                              ? AppColors.primary
                              : AppColors.textPrimary,
                        ),
                      ),
                      if (isToday) ...[
                        const SizedBox(width: 6),
                        Container(
                          padding: const EdgeInsets.symmetric(
                            horizontal: 6,
                            vertical: 2,
                          ),
                          decoration: BoxDecoration(
                            color: AppColors.primary,
                            borderRadius: BorderRadius.circular(4),
                          ),
                          child: Text(
                            'Bugün',
                            style: AppTypography.bodySmall.copyWith(
                              color: Colors.white,
                              fontSize: 10,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                      ],
                    ],
                  ),
                  // Hours
                  Text(
                    wh.getFormattedHours(),
                    style: AppTypography.bodyMedium.copyWith(
                      color: wh.isClosed
                          ? AppColors.error
                          : AppColors.textSecondary,
                      fontWeight: isToday ? FontWeight.w600 : FontWeight.normal,
                    ),
                  ),
                ],
              ),
            );
          }).toList(),
        ],
      );
    }

    return const SizedBox.shrink();
  }
}
