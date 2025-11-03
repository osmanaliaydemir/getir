import 'core/config/environment_config.dart';
import 'main.dart';

/// Production environment entry point
/// 
/// Configured for production environment:
/// - Debug mode: disabled
/// - SSL pinning: enabled
/// - Logging: minimal
/// - Performance monitoring: enabled
/// - Crash reporting: enabled
/// 
/// Usage: flutter run -t lib/main_prod.dart --release
void main() {
  mainWithEnvironment(EnvironmentConfig.prod);
}

