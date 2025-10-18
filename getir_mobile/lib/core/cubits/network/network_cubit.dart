import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../services/network_service.dart';
import '../../services/logger_service.dart';

part 'network_state.dart';

/// NetworkCubit
///
/// Manages network connectivity state using BLoC pattern
/// Replaces NetworkProvider for consistent state management architecture
class NetworkCubit extends Cubit<NetworkState> {
  final NetworkService _networkService;

  NetworkCubit(this._networkService) : super(const NetworkState.online());

  /// Initialize network monitoring
  Future<void> initialize() async {
    try {
      await _networkService.initialize();

      // Listen to network status changes
      _networkService.networkStatusStream.listen((isOnline) {
        if (isOnline) {
          emit(const NetworkState.online());
          logger.info('Network status: Online', tag: 'NetworkCubit');
        } else {
          emit(const NetworkState.offline());
          logger.warning('Network status: Offline', tag: 'NetworkCubit');
        }
      });
    } catch (e, stackTrace) {
      logger.error(
        'Failed to initialize network monitoring',
        tag: 'NetworkCubit',
        error: e,
        stackTrace: stackTrace,
      );
      emit(const NetworkState.offline());
    }
  }

  /// Retry connection
  Future<void> retryConnection() async {
    if (state.isRetrying) return;

    emit(state.copyWith(isRetrying: true));
    logger.debug('Retrying network connection', tag: 'NetworkCubit');

    try {
      // Wait a bit before retrying
      await Future.delayed(const Duration(seconds: 1));

      // Force check connectivity
      await _networkService.initialize();

      // Wait for status update
      await Future.delayed(const Duration(seconds: 2));
    } catch (e, stackTrace) {
      logger.error(
        'Retry connection failed',
        tag: 'NetworkCubit',
        error: e,
        stackTrace: stackTrace,
      );
    } finally {
      emit(state.copyWith(isRetrying: false));
    }
  }

  /// Check if specific host is reachable
  Future<bool> isHostReachable(String host) async {
    return await _networkService.isHostReachable(host);
  }

  /// Get connection type
  Future<String> getConnectionType() async {
    return await _networkService.getConnectionType();
  }

  @override
  Future<void> close() {
    _networkService.dispose();
    return super.close();
  }
}
