import 'dart:async';
import 'dart:io';
import 'package:connectivity_plus/connectivity_plus.dart';
import 'package:flutter/foundation.dart';
import 'package:dio/dio.dart';
import 'package:dio/io.dart';
import '../constants/environment.dart';

class NetworkService {
  static final NetworkService _instance = NetworkService._internal();
  factory NetworkService() => _instance;
  NetworkService._internal() {
    _configureDio();
  }

  final Connectivity _connectivity = Connectivity();
  StreamSubscription<List<ConnectivityResult>>? _connectivitySubscription;

  bool _isOnline = true;
  final StreamController<bool> _networkStatusController =
      StreamController<bool>.broadcast();

  final Dio dio = Dio();

  // Getters
  bool get isOnline => _isOnline;
  Stream<bool> get networkStatusStream => _networkStatusController.stream;

  // Initialize network monitoring
  Future<void> initialize() async {
    // Check initial connectivity
    await _checkConnectivity();

    // Listen to connectivity changes
    _connectivitySubscription = _connectivity.onConnectivityChanged.listen(
      _onConnectivityChanged,
    );
  }

  // Check current connectivity
  Future<void> _checkConnectivity() async {
    try {
      final connectivityResults = await _connectivity.checkConnectivity();
      await _updateConnectionStatus(connectivityResults);
    } catch (e) {
      if (kDebugMode) {
        print('Error checking connectivity: $e');
      }
      _setOffline();
    }
  }

  // Handle connectivity changes
  Future<void> _onConnectivityChanged(List<ConnectivityResult> results) async {
    await _updateConnectionStatus(results);
  }

  // Update connection status
  Future<void> _updateConnectionStatus(List<ConnectivityResult> results) async {
    final wasOnline = _isOnline;

    if (results.contains(ConnectivityResult.none)) {
      _setOffline();
    } else {
      // Check if we can actually reach the internet
      final isReachable = await _isInternetReachable();
      if (isReachable) {
        _setOnline();
      } else {
        _setOffline();
      }
    }

    // Notify listeners if status changed
    if (wasOnline != _isOnline) {
      _networkStatusController.add(_isOnline);
      if (kDebugMode) {
        print('Network status changed: ${_isOnline ? "Online" : "Offline"}');
      }
    }
  }

  // Check if internet is actually reachable
  Future<bool> _isInternetReachable() async {
    try {
      final result = await InternetAddress.lookup(
        'google.com',
      ).timeout(const Duration(seconds: 5));
      return result.isNotEmpty && result[0].rawAddress.isNotEmpty;
    } catch (e) {
      return false;
    }
  }

  // Set online status
  void _setOnline() {
    _isOnline = true;
  }

  // Set offline status
  void _setOffline() {
    _isOnline = false;
  }

  // Check if specific host is reachable
  Future<bool> isHostReachable(String host) async {
    try {
      final result = await InternetAddress.lookup(
        host,
      ).timeout(const Duration(seconds: 5));
      return result.isNotEmpty && result[0].rawAddress.isNotEmpty;
    } catch (e) {
      return false;
    }
  }

  // Get connection type
  Future<String> getConnectionType() async {
    try {
      final connectivityResults = await _connectivity.checkConnectivity();

      if (connectivityResults.contains(ConnectivityResult.wifi)) {
        return 'WiFi';
      } else if (connectivityResults.contains(ConnectivityResult.mobile)) {
        return 'Mobile';
      } else if (connectivityResults.contains(ConnectivityResult.ethernet)) {
        return 'Ethernet';
      } else if (connectivityResults.contains(ConnectivityResult.bluetooth)) {
        return 'Bluetooth';
      } else {
        return 'Unknown';
      }
    } catch (e) {
      return 'Unknown';
    }
  }

  void _configureDio() {
    if (EnvironmentConfig.enableCertificatePinning) {
      dio.httpClientAdapter = IOHttpClientAdapter()
        ..onHttpClientCreate = (HttpClient client) {
          client.badCertificateCallback = (cert, host, port) {
            // Implement basic host allowlist (replace with real cert pinning)
            const allowedHosts = {'localhost', '127.0.0.1'};
            if (allowedHosts.contains(host)) return true;
            // TODO: Replace with certificate fingerprints comparison
            return false;
          };
          return client;
        };
    }
  }

  // Dispose resources
  void dispose() {
    _connectivitySubscription?.cancel();
    _networkStatusController.close();
  }
}
