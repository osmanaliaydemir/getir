import 'package:flutter/material.dart';

class LanguageSelector extends StatelessWidget {
  final String currentLanguage;
  final Function(String) onLanguageChanged;

  const LanguageSelector({
    super.key,
    required this.currentLanguage,
    required this.onLanguageChanged,
  });

  @override
  Widget build(BuildContext context) {
    return PopupMenuButton<String>(
      icon: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
        decoration: BoxDecoration(
          border: Border.all(color: Colors.grey[300]!),
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              _getLanguageFlag(currentLanguage),
              style: const TextStyle(fontSize: 16),
            ),
            const SizedBox(width: 4),
            Text(
              _getLanguageName(currentLanguage),
              style: const TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w500,
              ),
            ),
            const SizedBox(width: 4),
            const Icon(
              Icons.keyboard_arrow_down,
              size: 16,
            ),
          ],
        ),
      ),
      onSelected: onLanguageChanged,
      itemBuilder: (BuildContext context) => [
        PopupMenuItem<String>(
          value: 'tr',
          child: Row(
            children: [
              Text('🇹🇷', style: const TextStyle(fontSize: 16)),
              const SizedBox(width: 8),
              const Text('Türkçe'),
              if (currentLanguage == 'tr') ...[
                const Spacer(),
                const Icon(Icons.check, color: Colors.green, size: 16),
              ],
            ],
          ),
        ),
        PopupMenuItem<String>(
          value: 'en',
          child: Row(
            children: [
              Text('🇺🇸', style: const TextStyle(fontSize: 16)),
              const SizedBox(width: 8),
              const Text('English'),
              if (currentLanguage == 'en') ...[
                const Spacer(),
                const Icon(Icons.check, color: Colors.green, size: 16),
              ],
            ],
          ),
        ),
        PopupMenuItem<String>(
          value: 'ar',
          child: Row(
            children: [
              Text('🇸🇦', style: const TextStyle(fontSize: 16)),
              const SizedBox(width: 8),
              const Text('العربية'),
              if (currentLanguage == 'ar') ...[
                const Spacer(),
                const Icon(Icons.check, color: Colors.green, size: 16),
              ],
            ],
          ),
        ),
      ],
    );
  }

  String _getLanguageFlag(String languageCode) {
    switch (languageCode) {
      case 'tr':
        return '🇹🇷';
      case 'en':
        return '🇺🇸';
      case 'ar':
        return '🇸🇦';
      default:
        return '🇹🇷';
    }
  }

  String _getLanguageName(String languageCode) {
    switch (languageCode) {
      case 'tr':
        return 'TR';
      case 'en':
        return 'EN';
      case 'ar':
        return 'AR';
      default:
        return 'TR';
    }
  }
}
