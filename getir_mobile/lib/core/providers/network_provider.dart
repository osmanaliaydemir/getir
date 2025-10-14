import 'package:flutter/foundation.dart';
import '../services/network_service.dart';

class NetworkProvider extends ChangeNotifier {
  final NetworkService _networkService = NetworkService();
  
  bool _isOnline = true;
  bool _isRetrying = false;

  // Getters
  bool get isOnline => _isOnline;
  bool get isRetrying => _isRetrying;
  bool get isOffline => !_isOnline;

  // Initialize network monitoring
  Future<void> initialize() async {
    await _networkService.initialize();
    
    // Listen to network status changes
    _networkService.networkStatusStream.listen((isOnline) {
      _isOnline = isOnline;
      _isRetrying = false;
      notifyListeners();
    });
  }

  // Retry connection
  Future<void> retryConnection() async {
    if (_isRetrying) return;
    
    _isRetrying = true;
    notifyListeners();

    try {
      // Wait a bit before retrying
      await Future.delayed(const Duration(seconds: 1));
      
      // Force check connectivity
      await _networkService.initialize();
      
      // Wait for status update
      await Future.delayed(const Duration(seconds: 2));
      
    } catch (e) {
      if (kDebugMode) {
        print('Retry connection failed: $e');
      }
    } finally {
      _isRetrying = false;
      notifyListeners();
    }
  }

  // Check if specific host is reachable
  Future<bool> isHostReachable(String host) async {
    return await _networkService.isHostReachable(host);
  }

  // Get connection type
  Future<String> getConnectionType() async {
    return await _networkService.getConnectionType();
  }

  @override
  void dispose() {
    _networkService.dispose();
    super.dispose();
  }
}
