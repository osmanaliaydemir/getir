import 'core/config/environment_config.dart';
import 'main.dart';

/// Staging environment entry point
/// 
/// Configured for staging environment:
/// - Debug mode: enabled
/// - SSL pinning: enabled
/// - Logging: moderate
/// - Performance monitoring: enabled
/// 
/// Usage: flutter run -t lib/main_staging.dart
void main() {
  mainWithEnvironment(EnvironmentConfig.staging);
}

