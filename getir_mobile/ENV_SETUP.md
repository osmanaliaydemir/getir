# Environment Configuration Setup Guide

## 📋 Overview

This app uses environment-specific configurations for different deployment stages (dev, staging, production).

## 🔧 Setup Instructions

### 1. Create Environment Files

Create the following files in the root directory:

#### `.env.dev` (Development)
```env
API_BASE_URL=http://localhost:5000
API_TIMEOUT=30000
API_KEY=dev_api_key_12345
ENCRYPTION_KEY=dev_32_char_encryption_key_12345678
ENABLE_SSL_PINNING=false
DEBUG_MODE=true
GOOGLE_MAPS_API_KEY=your_google_maps_dev_key
```

#### `.env.staging` (Staging)
```env
API_BASE_URL=https://staging-api.getir.com
API_TIMEOUT=30000
API_KEY=staging_api_key_here
ENCRYPTION_KEY=staging_32_char_encryption_key
ENABLE_SSL_PINNING=true
DEBUG_MODE=true
GOOGLE_MAPS_API_KEY=your_google_maps_staging_key
```

#### `.env.prod` (Production)
```env
API_BASE_URL=https://api.getir.com
API_TIMEOUT=30000
API_KEY=production_api_key_here
ENCRYPTION_KEY=production_32_char_encryption_key
ENABLE_SSL_PINNING=true
DEBUG_MODE=false
GOOGLE_MAPS_API_KEY=your_google_maps_prod_key
```

### 2. Update main.dart

Change the environment in `main.dart`:

```dart
// For Development
await EnvironmentConfig.initialize(environment: EnvironmentConfig.dev);

// For Staging
await EnvironmentConfig.initialize(environment: EnvironmentConfig.staging);

// For Production
await EnvironmentConfig.initialize(environment: EnvironmentConfig.prod);
```

### 3. Build Commands

```bash
# Development Build
flutter run --debug

# Staging Build (with dart-define)
flutter run --release --dart-define=ENV=staging

# Production Build
flutter build apk --release --dart-define=ENV=production
flutter build ios --release --dart-define=ENV=production
```

## 🔐 Security Notes

- ⚠️ **NEVER commit .env files to git!**
- ✅ Only commit `.env.example` as a template
- 🔒 Keep production keys secure
- 🔑 Rotate API keys regularly
- 📝 Use different keys for each environment

## 📱 Platform-Specific Setup

### Android

Edit `android/app/build.gradle`:

```gradle
android {
    flavorDimensions "environment"
    productFlavors {
        dev {
            dimension "environment"
            applicationIdSuffix ".dev"
            versionNameSuffix "-dev"
        }
        staging {
            dimension "environment"
            applicationIdSuffix ".staging"
            versionNameSuffix "-staging"
        }
        prod {
            dimension "environment"
        }
    }
}
```

Build commands:
```bash
flutter build apk --flavor dev
flutter build apk --flavor staging
flutter build apk --flavor prod
```

### iOS

1. Open `ios/Runner.xcworkspace` in Xcode
2. Create 3 schemes: Dev, Staging, Production
3. Add configuration for each scheme

## 🧪 Testing Environments

```bash
# Test with dev environment
flutter test --dart-define=ENV=dev

# Test with staging
flutter test --dart-define=ENV=staging
```

## ✅ Verification

After setup, run the app and check console output:
```
🔧 Environment Configuration:
   Environment: dev
   API Base URL: http://localhost:5000
   API Timeout: 30000ms
   SSL Pinning: false
   Debug Mode: true
```

## 📚 References

- [flutter_dotenv Package](https://pub.dev/packages/flutter_dotenv)
- [Flutter Environment Variables](https://dart.dev/guides/environment-declarations)

