import 'package:flutter/material.dart';

/// Service to manage notification badge count
class NotificationBadgeService extends ChangeNotifier {
  int _unreadCount = 0;

  int get unreadCount => _unreadCount;
  bool get hasUnread => _unreadCount > 0;

  String get badgeText {
    if (_unreadCount > 99) return '99+';
    if (_unreadCount > 0) return _unreadCount.toString();
    return '';
  }

  void updateUnreadCount(int count) {
    if (_unreadCount != count) {
      _unreadCount = count;
      notifyListeners();
    }
  }

  void increment() {
    _unreadCount++;
    notifyListeners();
  }

  void decrement() {
    if (_unreadCount > 0) {
      _unreadCount--;
      notifyListeners();
    }
  }

  void reset() {
    _unreadCount = 0;
    notifyListeners();
  }
}
