import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../domain/entities/notification_preferences.dart';
import '../../bloc/notification_preferences/notification_preferences_bloc.dart';

class NotificationPreferencesPage extends StatefulWidget {
  const NotificationPreferencesPage({super.key});

  @override
  State<NotificationPreferencesPage> createState() =>
      _NotificationPreferencesPageState();
}

class _NotificationPreferencesPageState
    extends State<NotificationPreferencesPage> {
  Timer? _debounceTimer;
  NotificationPreferences? _pendingUpdate;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    context.read<NotificationPreferencesBloc>().add(
      LoadNotificationPreferences(),
    );
  }

  @override
  void dispose() {
    _debounceTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.white,
      appBar: AppBar(
        title: Text(
          'İletişim Tercihleri',
          style: AppTypography.headlineSmall.copyWith(
            color: AppColors.white,
            fontWeight: FontWeight.w600,
          ),
        ),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.pop(context),
        ),
        actions: [
          if (_isSaving)
            const Padding(
              padding: EdgeInsets.all(16.0),
              child: SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  valueColor: AlwaysStoppedAnimation<Color>(AppColors.white),
                ),
              ),
            )
          else
            BlocBuilder<
              NotificationPreferencesBloc,
              NotificationPreferencesState
            >(
              builder: (context, state) {
                if (state is NotificationPreferencesLoaded) {
                  return TextButton(
                    onPressed: () => _resetToDefaults(context),
                    child: Text(
                      'Varsayılan',
                      style: AppTypography.bodyMedium.copyWith(
                        color: AppColors.white,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  );
                }
                return const SizedBox.shrink();
              },
            ),
        ],
      ),
      body:
          BlocConsumer<
            NotificationPreferencesBloc,
            NotificationPreferencesState
          >(
            listener: (context, state) {
              if (state is NotificationPreferencesLoaded && _isSaving) {
                setState(() => _isSaving = false);
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Tercihleriniz başarıyla güncellendi!'),
                    backgroundColor: AppColors.success,
                    duration: Duration(seconds: 2),
                  ),
                );
              }

              if (state is NotificationPreferencesError) {
                setState(() => _isSaving = false);
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(state.message),
                    backgroundColor: AppColors.error,
                    duration: const Duration(seconds: 3),
                  ),
                );
              }
            },
            builder: (context, state) {
              if (state is NotificationPreferencesLoading) {
                return const Center(
                  child: CircularProgressIndicator(color: AppColors.primary),
                );
              }

              if (state is NotificationPreferencesLoaded) {
                return _buildPreferencesContent(state.preferences);
              }

              if (state is NotificationPreferencesError) {
                return _buildErrorState(state.message);
              }

              return const Center(
                child: CircularProgressIndicator(color: AppColors.primary),
              );
            },
          ),
    );
  }

  Widget _buildPreferencesContent(NotificationPreferences preferences) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Push Notifications Section
          _buildSectionHeader(
            'Bildirimler',
            'Uygulama içi bildirimler',
            Icons.notifications,
          ),
          _buildToggleItem(
            'Push Bildirimleri',
            'Uygulama içi bildirimler al',
            preferences.pushEnabled,
            (value) => _updatePreference('pushEnabled', value),
          ),
          _buildToggleItem(
            'Sipariş Güncellemeleri',
            'Sipariş durumu değişikliklerini bildir',
            preferences.pushOrderUpdates,
            (value) => _updatePreference('pushOrderUpdates', value),
          ),
          _buildToggleItem(
            'Promosyonlar',
            'Özel teklifler ve kampanyalar',
            preferences.pushPromotions,
            (value) => _updatePreference('pushPromotions', value),
          ),
          _buildToggleItem(
            'Güvenlik Uyarıları',
            'Hesap güvenliği bildirimleri',
            preferences.pushSecurityAlerts,
            (value) => _updatePreference('pushSecurityAlerts', value),
          ),

          const SizedBox(height: 24),

          // Communication Section
          _buildSectionHeader(
            'İletişim',
            'E-posta ve SMS tercihleri',
            Icons.message,
          ),
          _buildToggleItem(
            'E-posta Bildirimleri',
            'E-posta ile bildirimler al',
            preferences.emailEnabled,
            (value) => _updatePreference('emailEnabled', value),
          ),
          _buildToggleItem(
            'E-posta Siparişleri',
            'Sipariş güncellemelerini e-posta ile al',
            preferences.emailOrderUpdates,
            (value) => _updatePreference('emailOrderUpdates', value),
          ),
          _buildToggleItem(
            'SMS Bildirimleri',
            'SMS ile bildirimler al',
            preferences.smsEnabled,
            (value) => _updatePreference('smsEnabled', value),
          ),
          _buildToggleItem(
            'SMS Siparişleri',
            'Sipariş güncellemelerini SMS ile al',
            preferences.smsOrderUpdates,
            (value) => _updatePreference('smsOrderUpdates', value),
          ),

          const SizedBox(height: 24),

          // Marketing Section
          _buildSectionHeader(
            'Pazarlama',
            'Promosyon ve kampanya bilgileri',
            Icons.campaign,
          ),
          _buildToggleItem(
            'E-posta Promosyonları',
            'Özel indirimler ve kampanyalar',
            preferences.emailPromotions,
            (value) => _updatePreference('emailPromotions', value),
          ),
          _buildToggleItem(
            'E-posta Bülteni',
            'Ürün ve hizmet tanıtımları',
            preferences.emailNewsletter,
            (value) => _updatePreference('emailNewsletter', value),
          ),
          _buildToggleItem(
            'SMS Promosyonları',
            'SMS ile kampanya bildirimleri',
            preferences.smsPromotions,
            (value) => _updatePreference('smsPromotions', value),
          ),

          const SizedBox(height: 32),

          // Info Card
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(12),
              border: Border.all(
                color: AppColors.primary.withOpacity(0.3),
                width: 1,
              ),
            ),
            child: Row(
              children: [
                Icon(Icons.info_outline, color: AppColors.primary, size: 24),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    'Tercihlerinizi değiştirdiğinizde, yeni ayarlar hemen uygulanır. Önemli güvenlik ve hesap bildirimleri her zaman gönderilir.',
                    style: AppTypography.bodySmall.copyWith(
                      color: AppColors.textPrimary,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSectionHeader(String title, String subtitle, IconData icon) {
    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(8),
            decoration: BoxDecoration(
              color: AppColors.primary.withOpacity(0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(icon, color: AppColors.primary, size: 20),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: AppTypography.bodyLarge.copyWith(
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
                Text(
                  subtitle,
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildToggleItem(
    String title,
    String subtitle,
    bool value,
    ValueChanged<bool> onChanged,
  ) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.greyLight, width: 1),
        boxShadow: [
          BoxShadow(
            color: AppColors.shadowLight,
            blurRadius: 4,
            offset: const Offset(0, 1),
          ),
        ],
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: AppTypography.bodyMedium.copyWith(
                    fontWeight: FontWeight.w600,
                    color: AppColors.textPrimary,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  subtitle,
                  style: AppTypography.bodySmall.copyWith(
                    color: AppColors.textSecondary,
                  ),
                ),
              ],
            ),
          ),
          Switch(
            value: value,
            onChanged: _isSaving ? null : onChanged,
            activeColor: AppColors.primary,
            activeTrackColor: AppColors.primary.withOpacity(0.3),
            inactiveThumbColor: AppColors.grey,
            inactiveTrackColor: AppColors.greyLight,
          ),
        ],
      ),
    );
  }

  Widget _buildErrorState(String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 120,
              height: 120,
              decoration: BoxDecoration(
                color: AppColors.error.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(
                Icons.error_outline,
                size: 60,
                color: AppColors.error.withOpacity(0.6),
              ),
            ),
            const SizedBox(height: 24),
            Text(
              'Bir hata oluştu',
              style: AppTypography.headlineSmall.copyWith(
                color: AppColors.textPrimary,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              message,
              textAlign: TextAlign.center,
              style: AppTypography.bodyMedium.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 32),
            ElevatedButton(
              onPressed: () {
                context.read<NotificationPreferencesBloc>().add(
                  LoadNotificationPreferences(),
                );
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: AppColors.white,
                padding: const EdgeInsets.symmetric(
                  horizontal: 32,
                  vertical: 16,
                ),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              child: Text(
                'Tekrar Dene',
                style: AppTypography.bodyLarge.copyWith(
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _updatePreference(String key, bool value) {
    final currentState = context.read<NotificationPreferencesBloc>().state;
    if (currentState is NotificationPreferencesLoaded) {
      // Optimistic update - UI hemen güncellenir
      final updatedPreferences = _applyUpdate(
        currentState.preferences,
        key,
        value,
      );
      _pendingUpdate = updatedPreferences;

      // Debounce - 1 saniye bekle, sonra tek request gönder
      _debounceTimer?.cancel();
      setState(() => _isSaving = true);

      _debounceTimer = Timer(const Duration(milliseconds: 1000), () {
        if (_pendingUpdate != null) {
          context.read<NotificationPreferencesBloc>().add(
            UpdateNotificationPreferencesEvent(_pendingUpdate!),
          );
        }
      });
    }
  }

  NotificationPreferences _applyUpdate(
    NotificationPreferences prefs,
    String key,
    bool value,
  ) {
    switch (key) {
      case 'pushEnabled':
        return prefs.copyWith(pushEnabled: value);
      case 'pushOrderUpdates':
        return prefs.copyWith(pushOrderUpdates: value);
      case 'pushPromotions':
        return prefs.copyWith(pushPromotions: value);
      case 'pushSecurityAlerts':
        return prefs.copyWith(pushSecurityAlerts: value);
      case 'emailEnabled':
        return prefs.copyWith(emailEnabled: value);
      case 'emailOrderUpdates':
        return prefs.copyWith(emailOrderUpdates: value);
      case 'smsEnabled':
        return prefs.copyWith(smsEnabled: value);
      case 'smsOrderUpdates':
        return prefs.copyWith(smsOrderUpdates: value);
      case 'emailPromotions':
        return prefs.copyWith(emailPromotions: value);
      case 'emailNewsletter':
        return prefs.copyWith(emailNewsletter: value);
      case 'smsPromotions':
        return prefs.copyWith(smsPromotions: value);
      default:
        return prefs;
    }
  }

  void _resetToDefaults(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Varsayılan Ayarlar'),
        content: const Text(
          'Tüm tercihlerinizi varsayılan ayarlara sıfırlamak istediğinizden emin misiniz?',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('İptal'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.pop(context);
              context.read<NotificationPreferencesBloc>().add(
                LoadNotificationPreferences(),
              );
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: AppColors.white,
            ),
            child: const Text('Sıfırla'),
          ),
        ],
      ),
    );
  }
}
