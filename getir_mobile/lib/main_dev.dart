import 'core/config/environment_config.dart';
import 'main.dart';

/// Development environment entry point
/// 
/// Configured for development environment:
/// - Debug mode: enabled
/// - SSL pinning: disabled  
/// - Logging: verbose
/// - Hot reload: enabled
/// 
/// Usage: flutter run -t lib/main_dev.dart
void main() {
  mainWithEnvironment(EnvironmentConfig.dev);
}

