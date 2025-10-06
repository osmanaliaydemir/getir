import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../core/localization/app_localizations.dart';
import '../../bloc/notification_preferences/notification_preferences_bloc.dart';
import '../../../domain/entities/notification_preferences.dart';

class NotificationSettingsPage extends StatefulWidget {
  const NotificationSettingsPage({super.key});

  @override
  State<NotificationSettingsPage> createState() =>
      _NotificationSettingsPageState();
}

class _NotificationSettingsPageState extends State<NotificationSettingsPage> {
  @override
  void initState() {
    super.initState();
    context.read<NotificationPreferencesBloc>().add(
      LoadNotificationPreferences(),
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(l10n.notifications),
        backgroundColor: AppColors.primary,
        foregroundColor: AppColors.white,
      ),
      body:
          BlocBuilder<
            NotificationPreferencesBloc,
            NotificationPreferencesState
          >(
            builder: (context, state) {
              if (state is NotificationPreferencesLoading ||
                  state is NotificationPreferencesInitial) {
                return const Center(
                  child: CircularProgressIndicator(
                    valueColor: AlwaysStoppedAnimation<Color>(
                      AppColors.primary,
                    ),
                  ),
                );
              }

              if (state is NotificationPreferencesError) {
                return Center(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(
                          Icons.error_outline,
                          color: AppColors.error,
                          size: 56,
                        ),
                        const SizedBox(height: 12),
                        Text(
                          state.message,
                          style: AppTypography.bodyLarge.copyWith(
                            color: AppColors.error,
                          ),
                          textAlign: TextAlign.center,
                        ),
                        const SizedBox(height: 16),
                        ElevatedButton(
                          onPressed: () => context
                              .read<NotificationPreferencesBloc>()
                              .add(LoadNotificationPreferences()),
                          child: Text(l10n.retry),
                        ),
                      ],
                    ),
                  ),
                );
              }

              if (state is NotificationPreferencesLoaded) {
                final prefs = state.preferences;
                return ListView(
                  padding: const EdgeInsets.all(16),
                  children: [
                    _buildSwitchTile(
                      title: l10n.orderUpdates,
                      value: prefs.orderUpdates,
                      onChanged: (v) =>
                          _update(context, prefs.copyWith(orderUpdates: v)),
                    ),
                    _buildSwitchTile(
                      title: l10n.promotions,
                      value: prefs.promotions,
                      onChanged: (v) =>
                          _update(context, prefs.copyWith(promotions: v)),
                    ),
                    _buildSwitchTile(
                      title: l10n.systemNotifications,
                      value: prefs.system,
                      onChanged: (v) =>
                          _update(context, prefs.copyWith(system: v)),
                    ),
                    _buildSwitchTile(
                      title: l10n.marketing,
                      value: prefs.marketing,
                      onChanged: (v) =>
                          _update(context, prefs.copyWith(marketing: v)),
                    ),
                  ],
                );
              }

              return const SizedBox.shrink();
            },
          ),
    );
  }

  Widget _buildSwitchTile({
    required String title,
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: AppColors.white,
        borderRadius: BorderRadius.circular(12),
      ),
      child: SwitchListTile(
        title: Text(title, style: AppTypography.bodyLarge),
        value: value,
        onChanged: onChanged,
        activeThumbColor: AppColors.primary,
      ),
    );
  }

  void _update(BuildContext context, NotificationPreferences prefs) {
    context.read<NotificationPreferencesBloc>().add(
      UpdateNotificationPreferencesEvent(prefs),
    );
  }
}
