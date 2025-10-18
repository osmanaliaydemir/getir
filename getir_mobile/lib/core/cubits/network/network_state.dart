part of 'network_cubit.dart';

/// NetworkState
///
/// Represents network connectivity state
class NetworkState extends Equatable {
  final bool isOnline;
  final bool isRetrying;

  const NetworkState({required this.isOnline, this.isRetrying = false});

  const NetworkState.online() : this(isOnline: true, isRetrying: false);
  const NetworkState.offline() : this(isOnline: false, isRetrying: false);

  bool get isOffline => !isOnline;

  NetworkState copyWith({bool? isOnline, bool? isRetrying}) {
    return NetworkState(
      isOnline: isOnline ?? this.isOnline,
      isRetrying: isRetrying ?? this.isRetrying,
    );
  }

  @override
  List<Object?> get props => [isOnline, isRetrying];
}
