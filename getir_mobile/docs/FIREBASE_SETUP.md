# 🔥 Firebase Setup Guide

This guide will help you set up Firebase services for the Getir Mobile app.

---

## 📋 Prerequisites

- Firebase account (https://console.firebase.google.com)
- Firebase CLI installed (`npm install -g firebase-tools`)
- Access to Firebase project (or create new one)
- Flutter SDK installed

---

## 🚀 Step 1: Create Firebase Project

### 1.1 Go to Firebase Console
```
https://console.firebase.google.com
```

### 1.2 Create New Project
1. Click "Add project"
2. Enter project name: **"Getir Mobile"** (or your preferred name)
3. Enable Google Analytics (Recommended)
4. Select Analytics account or create new
5. Click "Create project"

---

## 📱 Step 2: Add Apps to Firebase Project

### 2.1 Add Android App

1. Click "Add app" → Select **Android**
2. Enter details:
   ```
   Android package name: com.getir.mobile
   App nickname: Getir Mobile Android
   SHA-1 certificate: (optional for debug, required for release)
   ```

3. Download `google-services.json`
4. Place file in: `android/app/google-services.json`

5. Update `android/build.gradle`:
   ```gradle
   dependencies {
       classpath 'com.google.gms:google-services:4.4.0'
       classpath 'com.google.firebase:firebase-crashlytics-gradle:2.9.9'
       classpath 'com.google.firebase:perf-plugin:1.4.2'
   }
   ```

6. Update `android/app/build.gradle`:
   ```gradle
   apply plugin: 'com.google.gms.google-services'
   apply plugin: 'com.google.firebase.crashlytics'
   apply plugin: 'com.google.firebase.firebase-perf'

   dependencies {
       implementation platform('com.google.firebase:firebase-bom:32.7.0')
       implementation 'com.google.firebase:firebase-analytics'
       implementation 'com.google.firebase:firebase-crashlytics'
       implementation 'com.google.firebase:firebase-messaging'
       implementation 'com.google.firebase:firebase-perf'
   }
   ```

### 2.2 Add iOS App

1. Click "Add app" → Select **iOS**
2. Enter details:
   ```
   iOS bundle ID: com.getir.mobile
   App nickname: Getir Mobile iOS
   App Store ID: (leave empty for now)
   ```

3. Download `GoogleService-Info.plist`
4. Add to Xcode project:
   - Open `ios/Runner.xcworkspace` in Xcode
   - Drag `GoogleService-Info.plist` into `Runner` folder
   - Ensure "Copy items if needed" is checked
   - Select `Runner` target

5. Update `ios/Podfile`:
   ```ruby
   # Add at top
   platform :ios, '13.0'

   # Add in target
   pod 'Firebase/Analytics'
   pod 'Firebase/Crashlytics'
   pod 'Firebase/Messaging'
   pod 'Firebase/Performance'
   ```

6. Run `pod install`:
   ```bash
   cd ios
   pod install
   ```

---

## 🔧 Step 3: Enable Firebase Services

### 3.1 Analytics
- **Automatically enabled** when you add Analytics to your project
- No additional configuration needed

### 3.2 Crashlytics
1. Go to Firebase Console → Crashlytics
2. Click "Enable Crashlytics"
3. Follow on-screen instructions
4. Wait for first crash report (can take 24 hours)

### 3.3 Performance Monitoring
1. Go to Firebase Console → Performance
2. Click "Get started"
3. Performance monitoring will start automatically

### 3.4 Cloud Messaging (Already configured)
- FCM token handling: ✅ Already implemented
- Background messages: ✅ Already implemented
- Notification handling: ✅ Already implemented

---

## 🧪 Step 4: Test Firebase Integration

### 4.1 Test Analytics

#### Enable DebugView
```bash
# Android
adb shell setprop debug.firebase.analytics.app com.getir.mobile

# iOS (Xcode)
Edit scheme → Arguments → Add: -FIRDebugEnabled
```

#### Verify Events
1. Open Firebase Console → Analytics → DebugView
2. Select your device
3. Open app and navigate
4. You should see:
   - `screen_view` events
   - `first_open` event
   - Custom events (login, add_to_cart, etc.)

### 4.2 Test Crashlytics

#### Force a Test Crash
```dart
// Add to a debug button
import 'package:firebase_crashlytics/firebase_crashlytics.dart';

FirebaseCrashlytics.instance.crash();
```

#### Or Test Error
```dart
try {
  throw Exception('Test crash from button');
} catch (e, stack) {
  FirebaseCrashlytics.instance.recordError(e, stack, fatal: false);
}
```

#### Verify
1. Trigger crash/error
2. Wait 5-10 minutes
3. Check Firebase Console → Crashlytics
4. You should see the crash report

### 4.3 Test Performance

1. Performance traces automatically tracked
2. Go to Firebase Console → Performance
3. Wait a few hours for data
4. You should see:
   - App start time
   - Screen rendering time
   - Network request duration

---

## 📊 Step 5: Configure Analytics Events

### Verify Events in Console

After running the app for a few minutes, check:

1. **User Engagement:**
   - screen_view
   - session_start
   - user_engagement

2. **E-commerce:**
   - view_item (product views)
   - add_to_cart
   - begin_checkout
   - add_payment_info
   - purchase

3. **Authentication:**
   - login
   - sign_up

4. **Custom Events:**
   - button_click
   - search
   - add_to_favorites
   - network_reconnected
   - app_error

---

## 🎯 Step 6: Set Up Audiences & Conversion Tracking

### 6.1 Create Audiences
1. Go to Analytics → Audiences
2. Create custom audiences:
   - Active users (used app in last 7 days)
   - Buyers (completed purchase)
   - Cart abandoners (added to cart but didn't purchase)
   - Search users (searched in last 30 days)

### 6.2 Mark Conversions
1. Go to Analytics → Events
2. Mark these as conversions:
   - `purchase`
   - `sign_up`
   - `add_to_cart`

---

## 🔐 Step 7: Security & Privacy

### 7.1 Update Privacy Policy
Add section about Firebase data collection:
```
Our app uses Firebase services which may collect:
- Analytics data (user behavior, demographics)
- Crash reports (device info, stack traces)
- Performance data (app performance metrics)

For more info: https://firebase.google.com/support/privacy
```

### 7.2 Data Collection Settings

Go to Project Settings → Integrations:
- ✅ Enable Google Analytics
- ✅ Enable data sharing for improved suggestions
- ⚠️ Disable if GDPR requires (EU users)

### 7.3 User Consent (GDPR/KVKK)

Add to your consent flow:
```dart
// Disable analytics if user opts out
await analytics.setAnalyticsEnabled(userConsented);
await analytics.setCrashlyticsEnabled(userConsented);
```

---

## 🌍 Step 8: Multi-Environment Setup

### Development
```dart
// Use separate Firebase project for dev
// File: .env.dev
FIREBASE_PROJECT_ID=getir-mobile-dev
```

### Staging
```dart
// File: .env.staging
FIREBASE_PROJECT_ID=getir-mobile-staging
```

### Production
```dart
// File: .env.prod
FIREBASE_PROJECT_ID=getir-mobile-prod
```

### Configuration per Environment

**Recommended:** Use FlavorDimensions
```gradle
// android/app/build.gradle
flavorDimensions "environment"
productFlavors {
    dev {
        dimension "environment"
        applicationIdSuffix ".dev"
        // Use google-services-dev.json
    }
    staging {
        dimension "environment"
        applicationIdSuffix ".staging"
        // Use google-services-staging.json
    }
    prod {
        dimension "environment"
        // Use google-services.json
    }
}
```

---

## 📈 Step 9: Set Up Dashboards

### 9.1 Analytics Dashboard

**Key Metrics to Monitor:**
```
✅ Daily Active Users (DAU)
✅ Monthly Active Users (MAU)
✅ Session Duration
✅ Screen Views per Session
✅ Conversion Rate (purchase / view_item)
✅ Cart Abandonment Rate
✅ Search Success Rate
```

**Custom Events:**
```
✅ Top 10 Searches
✅ Most Viewed Products
✅ Most Added to Cart
✅ Most Purchased Items
✅ Error Frequency
```

### 9.2 Crashlytics Dashboard

**Metrics to Monitor:**
```
✅ Crash-free users (target: >99.5%)
✅ Crash count per version
✅ Most common crashes
✅ Top affected devices
✅ OS version distribution
```

### 9.3 Performance Dashboard

**Metrics to Monitor:**
```
✅ App start time (target: <2s)
✅ Screen rendering time (target: <16ms)
✅ Network request duration
✅ API response times
✅ Custom traces (if added)
```

---

## 🚨 Step 10: Alerts & Notifications

### 10.1 Set Up Alerts

Go to each service and configure:

**Crashlytics Alerts:**
- New crash type detected
- Crash rate spike (>1%)
- Regressed issue (previously fixed)

**Performance Alerts:**
- App start time > 3 seconds
- HTTP request duration > 5 seconds

**Analytics Alerts:**
- DAU drops >20%
- Conversion rate drops >15%

### 10.2 Notification Channels
1. Go to Project Settings → Integrations
2. Add team email for alerts
3. Configure Slack/Discord webhooks (optional)

---

## 🔍 Step 11: Debug & Troubleshooting

### Common Issues

#### Analytics not showing in Console
```bash
# Check if google-services.json is present
ls -la android/app/google-services.json

# Enable debug logging
adb logcat | grep -i firebase

# Verify events in DebugView
# Wait 24 hours for production dashboard
```

#### Crashlytics not receiving crashes
```bash
# Check build.gradle has crashlytics plugin
# Verify ProGuard rules (for release builds)
# Wait 5-10 minutes after crash
# Check logs: adb logcat | grep -i crashlytics
```

#### Performance not showing data
```bash
# Wait 24 hours for first data
# Verify perf plugin in build.gradle
# Check automatic traces are enabled
```

---

## ✅ Verification Checklist

### Android
```
✅ google-services.json in android/app/
✅ google-services plugin in build.gradle
✅ Firebase BoM in dependencies
✅ Crashlytics plugin applied
✅ Performance plugin applied
✅ SHA-1 certificate added (for release)
✅ ProGuard rules added (for release)
```

### iOS
```
✅ GoogleService-Info.plist in Xcode project
✅ Firebase pods installed
✅ Runner scheme configured for debug
✅ Crashlytics upload script in build phases
✅ dSYM upload enabled
```

### Firebase Console
```
✅ Analytics enabled
✅ Crashlytics enabled
✅ Performance enabled
✅ FCM configured
✅ DebugView working
✅ Alerts configured
```

---

## 📚 Resources

### Official Documentation
- [Firebase Setup](https://firebase.google.com/docs/flutter/setup)
- [Analytics](https://firebase.google.com/docs/analytics/get-started?platform=flutter)
- [Crashlytics](https://firebase.google.com/docs/crashlytics/get-started?platform=flutter)
- [Performance](https://firebase.google.com/docs/perf-mon/get-started?platform=flutter)

### Troubleshooting
- [Analytics Debug](https://firebase.google.com/docs/analytics/debugview)
- [Crashlytics Testing](https://firebase.google.com/docs/crashlytics/test-implementation)

---

## 🎯 Success Criteria

After completing this setup, you should have:

```
✅ Firebase project created
✅ Android & iOS apps registered
✅ Configuration files in place
✅ Analytics tracking events
✅ Crashlytics receiving reports
✅ Performance monitoring active
✅ Alerts configured
✅ Team notifications set up
✅ Production ready monitoring
```

---

## ⚠️ Important Notes

### For Production Release

1. **Create separate Firebase projects:**
   - getir-mobile-dev
   - getir-mobile-staging
   - getir-mobile-prod

2. **Use build flavors** to switch between environments

3. **Enable ProGuard** for release builds (already configured)

4. **Upload dSYMs** for iOS symbolication

5. **Test in staging** before production

---

## 🔥 Quick Start Commands

```bash
# Install Firebase CLI
npm install -g firebase-tools

# Login to Firebase
firebase login

# Initialize project (run in project root)
cd getir_mobile
firebase init

# Select:
✅ Analytics
✅ Crashlytics  
✅ Performance Monitoring
✅ Cloud Messaging (already configured)

# Download config files
# Place in correct directories (see above)

# Run app
flutter run

# Check Firebase Console
# Wait 5-10 minutes for data
```

---

**Last Updated:** 8 Ekim 2025  
**Status:** Ready for Setup  
**Time Required:** 2-3 hours for complete setup
