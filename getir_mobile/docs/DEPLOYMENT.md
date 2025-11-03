# 🚀 Deployment Guide

**Tarih:** 2 Kasım 2025  
**Konu:** Getir Mobile - Production Deployment

---

## 📋 İçindekiler

- [Pre-Deployment Checklist](#pre-deployment-checklist)
- [Android Deployment](#android-deployment)
- [iOS Deployment](#ios-deployment)
- [CI/CD Pipeline](#cicd-pipeline)
- [Post-Deployment](#post-deployment)

---

## ✅ Pre-Deployment Checklist

### Security
- [ ] SSL Pinning enabled (`.env.prod`)
- [ ] Debug mode disabled
- [ ] Logging minimal
- [ ] Encryption keys secure
- [ ] API keys production-ready

### Testing
- [ ] All tests passing (`flutter test`)
- [ ] Integration tests passing
- [ ] Coverage above 60%
- [ ] Manual QA completed
- [ ] Performance tested

### Documentation
- [ ] Version bumped
- [ ] Changelog updated
- [ ] Release notes prepared

---

## 🤖 Android Deployment

### 1. Build Release APK

```bash
cd getir_mobile

# Build APK
flutter build apk --release --target lib/main_prod.dart

# Output: build/app/outputs/flutter-apk/app-release.apk
```

### 2. Build App Bundle (Play Store)

```bash
# Build AAB
flutter build appbundle --release --target lib/main_prod.dart

# Output: build/app/outputs/bundle/prodRelease/app-prod-release.aab
```

### 3. Sign the APK/AAB

```bash
# Sign with jarsigner
jarsigner -verbose -sigalg SHA256withRSA -digestalg SHA-256 \
  -keystore /path/to/keystore.jks \
  app-release.apk \
  alias_name

# Verify signature
jarsigner -verify -verbose -certs app-release.apk
```

### 4. Play Store Upload

1. Go to [Google Play Console](https://play.google.com/console)
2. Create new release
3. Upload `.aab` file
4. Fill release notes
5. Review & publish

---

## 🍎 iOS Deployment

### 1. Build iOS Release

```bash
cd getir_mobile

# Build iOS
flutter build ios --release --target lib/main_prod.dart

# Output: build/ios/iphoneos/Runner.app
```

### 2. Xcode Archive

```bash
# Open workspace in Xcode
open ios/Runner.xcworkspace

# Product → Archive
# Wait for archive to complete
```

### 3. Upload to App Store

1. Xcode → Window → Organizer
2. Select archive
3. Distribute App
4. App Store Connect
5. Upload
6. Wait for processing

### 4. App Store Connect

1. Go to [App Store Connect](https://appstoreconnect.apple.com)
2. Create new version
3. Add build
4. Fill metadata
5. Submit for review

---

## 🔄 CI/CD Pipeline

### GitHub Actions

`.github/workflows/flutter_ci.yml`:

```yaml
name: Deploy to Production

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  deploy-android:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: subosito/flutter-action@v4
        with:
          flutter-version: '3.19.0'
      
      - name: Build APK
        run: flutter build apk --release --target lib/main_prod.dart
      
      - name: Upload to Play Store
        uses: r0adkll/upload-google-play@v1
        with:
          serviceAccountJsonPlainText: ${{ secrets.GOOGLE_PLAY_SERVICE_ACCOUNT }}
          packageName: com.getir.mobile
          releaseFiles: build/app/outputs/flutter-apk/app-release.apk
  
  deploy-ios:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v4
      - uses: subosito/flutter-action@v4
        with:
          flutter-version: '3.19.0'
      
      - name: Build iOS
        run: flutter build ios --release --target lib/main_prod.dart
      
      - name: Upload to App Store
        uses: apple-actions/upload-testflight-build
        with:
          appPath: build/ios/Runner.ipa
```

---

## 📊 Post-Deployment

### Monitoring

1. **Crashlytics**
   - Check for crashes
   - Monitor crash-free rate
   - Fix critical crashes

2. **Analytics**
   - User engagement
   - Feature usage
   - Conversion rates

3. **Performance**
   - App startup time
   - API response times
   - Memory usage

### Rollback Plan

If critical issues occur:

```bash
# Android
# 1. Go to Play Console
# 2. Create new release with previous version
# 3. Publish immediately

# iOS
# 1. Go to App Store Connect
# 2. Remove current version
# 3. Re-publish previous version
```

---

**Hazırlayan:** DevOps Team  
**Tarih:** 2 Kasım 2025

