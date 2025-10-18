# 📊 Monitoring & Observability Dashboard

**Last Updated:** 8 Ekim 2025  
**Status:** Ready for Firebase Setup

---

## 🎯 Overview

This document describes the monitoring and observability setup for Getir Mobile app using Firebase services.

---

## 📊 Key Metrics to Monitor

### 1. User Engagement Metrics

#### Daily Active Users (DAU)
```
Target: Growing trend
Alert: Drop >20% day-over-day
```

#### Session Metrics
```
- Sessions per user: Target >3/day
- Session duration: Target >5 minutes
- Screen views per session: Target >10
```

#### Retention
```
- Day 1 retention: Target >40%
- Day 7 retention: Target >20%
- Day 30 retention: Target >10%
```

### 2. E-Commerce Metrics

#### Conversion Funnel
```
100% → Product View
 80% → Add to Cart
 50% → Begin Checkout
 45% → Add Payment Info
 40% → Purchase (Target conversion: >35%)
```

#### Cart Metrics
```
- Cart abandonment rate: Target <60%
- Average cart value: Monitor trend
- Items per cart: Target >2
```

#### Order Metrics
```
- Orders per day: Monitor growth
- Average order value: Monitor trend
- Order completion rate: Target >95%
- Order cancellation rate: Target <5%
```

### 3. Technical Performance Metrics

#### App Performance
```
- App start time: Target <2s (cold start)
- Screen rendering: Target <16ms (60fps)
- Memory usage: Target <150MB
- CPU usage: Target <30%
```

#### Network Performance
```
- API response time: Target <500ms
- Network error rate: Target <1%
- Connection quality score: Target >80
- Cache hit rate: Target >70%
```

#### Crash Metrics
```
- Crash-free users: Target >99.5%
- Crash-free sessions: Target >99.9%
- ANR rate: Target <0.1%
```

### 4. Feature Usage Metrics

#### Search
```
- Search queries per user: Monitor trend
- Search success rate: Target >80%
- Search-to-purchase rate: Target >15%
```

#### Favorites
```
- Users with favorites: Target >30%
- Favorites per user: Target >5
- Favorite-to-purchase rate: Target >25%
```

#### Notifications
```
- Notification delivery rate: Target >95%
- Notification open rate: Target >15%
- Notification-to-action rate: Target >10%
```

---

## 🔥 Firebase Analytics Dashboard

### Custom Events to Track

#### Authentication
```
- login (method: email)
- sign_up (method: email)
- logout
- forgot_password
```

#### Product Discovery
```
- screen_view (automatic)
- search (searchTerm, resultCount)
- view_item (productId, category, price)
- button_click (buttonName, screenName)
```

#### Shopping Flow
```
- add_to_cart (productId, price, quantity)
- remove_from_cart (itemId)
- begin_checkout (value, currency, items)
- add_payment_info (paymentType, value)
```

#### Purchase
```
- purchase (transactionId, total, items, shipping)
- order_cancelled (orderId, reason)
```

#### Offline Mode
```
- network_disconnected (pendingActions)
- network_reconnected (offlineDuration, pendingActions)
- offline_sync_completed (actionsSynced, duration)
```

### Custom Parameters
```
- product_id
- product_name
- product_category
- merchant_id
- merchant_name
- order_id
- cart_value
- user_segment
```

---

## 🐛 Crashlytics Dashboard

### Crash Prioritization

#### Priority 1 (Critical - Fix Immediately)
```
- Crash affects >1% of users
- Crash in payment flow
- Crash in order creation
- ANR (App Not Responding)
```

#### Priority 2 (High - Fix in 24h)
```
- Crash affects >0.1% of users
- Crash in checkout flow
- Crash in authentication
- Repeated crash patterns
```

#### Priority 3 (Medium - Fix in 1 week)
```
- Crash affects <0.1% of users
- Crash in non-critical features
- Edge case crashes
```

### Custom Keys to Track
```
- user_id
- screen_name
- network_status (online/offline)
- app_version
- device_model
- os_version
- api_endpoint (last called)
```

### Crash Grouping
```
Group by:
- Exception type
- Crash location (file + line)
- Affected OS versions
- Affected app versions
```

---

## ⚡ Performance Monitoring Dashboard

### Automatic Traces
```
✅ App start time
   - Cold start: Target <2s
   - Warm start: Target <1s
   - Hot start: Target <500ms

✅ Screen rendering
   - Slow rendering: Alert if >16ms
   - Frozen frames: Alert if >0.1%

✅ Network requests
   - Duration per endpoint
   - Success/failure rate
   - Timeout rate
```

### Custom Traces (if needed)
```
- checkout_flow (total time)
- image_load_time
- database_query_time
- map_load_time
- signalr_connection_time
```

### Network Request Monitoring
```
Track all API endpoints:
- /api/v1/auth/login
- /api/v1/products
- /api/v1/cart
- /api/v1/orders
- etc.

Metrics per endpoint:
- Average response time
- Success rate
- P50, P90, P95, P99 latency
```

---

## 📈 Custom Dashboards

### Dashboard 1: Business Overview
```
Metrics:
- DAU / MAU
- New users (today, this week, this month)
- Total orders (today, this week, this month)
- Revenue (today, this week, this month)
- Conversion rate
- Average order value
```

### Dashboard 2: User Journey
```
Funnel:
1. App opens
2. Search / Browse
3. Product views
4. Add to cart
5. Checkout
6. Payment
7. Purchase

Metrics per step:
- Conversion rate
- Drop-off rate
- Average time spent
```

### Dashboard 3: Technical Health
```
Metrics:
- Crash-free rate
- App start time (P50, P90, P95)
- API response time (per endpoint)
- Error rate (per feature)
- Network error rate
- Offline mode usage
```

### Dashboard 4: Feature Adoption
```
Metrics:
- Search usage (% of users)
- Favorites usage (% of users)
- Notification opt-in rate
- Profile completion rate
- Address saved (% of users)
```

---

## 🚨 Alerts Configuration

### Critical Alerts (Immediate Action)
```
🔴 Crash-free rate <99%
🔴 Order completion rate <90%
🔴 Payment failure rate >5%
🔴 API error rate >5%
🔴 App start time >5s
```

### Warning Alerts (Check within 1 hour)
```
🟡 DAU drops >15%
🟡 Conversion rate drops >10%
🟡 Search success rate <70%
🟡 Network error rate >2%
🟡 Crash-free rate <99.5%
```

### Info Alerts (Check within 24 hours)
```
🟢 New feature usage
🟢 Unusual spike in events
🟢 Platform-specific issues
```

---

## 📱 Platform-Specific Monitoring

### Android
```
Monitor:
- ANR rate (target <0.1%)
- Battery drain
- Background CPU usage
- APK size (target <50MB)
- Install success rate
```

### iOS
```
Monitor:
- Crash rate per iOS version
- Memory warnings
- Background task completion
- IPA size (target <50MB)
- TestFlight crashes
```

---

## 🔍 Debug Logging Strategy

### Log Levels per Environment

#### Development
```
- Level: DEBUG
- All logs visible
- Console logging enabled
- Verbose network logs
- BLoC state transitions logged
```

#### Staging
```
- Level: INFO
- Important logs visible
- Error logs sent to Firebase
- Network errors logged
- Critical state changes logged
```

#### Production
```
- Level: WARNING
- Only warnings and errors
- All errors sent to Firebase
- Sensitive data sanitized
- Minimal console logging
```

---

## 📊 BigQuery Export (Optional)

### Setup
1. Go to Firebase → Analytics → BigQuery linking
2. Enable daily export
3. Select events to export
4. Configure streaming (optional)

### Use Cases
```
✅ Custom SQL queries
✅ Advanced funnel analysis
✅ Cohort analysis
✅ Revenue forecasting
✅ Churn prediction
✅ Integration with Data Studio
```

---

## 🎯 Monitoring Best Practices

### Do's
```
✅ Monitor key business metrics daily
✅ Set up alerts for critical issues
✅ Review crashlytics weekly
✅ Track performance regressions
✅ Monitor new feature adoption
✅ Analyze user feedback
✅ Regular dashboard reviews
```

### Don'ts
```
❌ Log sensitive user data
❌ Over-alert (alert fatigue)
❌ Ignore warning-level issues
❌ Skip weekly reviews
❌ Forget to update dashboards
```

---

## 🛠️ Tools & Services

### Primary (Firebase)
```
✅ Firebase Analytics - User behavior
✅ Firebase Crashlytics - Crash reporting
✅ Firebase Performance - App performance
✅ Firebase Remote Config - Feature flags (P3-31)
```

### Optional (Third-party)
```
⚪ Sentry - Advanced error tracking
⚪ Datadog - APM & Monitoring
⚪ New Relic - Full-stack monitoring
⚪ Mixpanel - Product analytics
⚪ Amplitude - Behavioral analytics
```

---

## 📈 Success Metrics

### After Firebase Setup
```
Within 24 hours:
✅ Analytics events flowing
✅ DebugView working
✅ User properties set

Within 1 week:
✅ Dashboard populated
✅ Conversion funnel visible
✅ No critical crashes
✅ Performance baseline established
```

---

## 🎯 Next Steps

1. **Complete Firebase Setup** (see FIREBASE_SETUP.md)
2. **Verify all services** are receiving data
3. **Create custom dashboards** in Firebase Console
4. **Set up alerts** for critical metrics
5. **Train team** on dashboard usage
6. **Document** baseline metrics
7. **Schedule** weekly review meetings

---

**Status:** Documentation Complete, Ready for Implementation  
**Time to Setup:** 2-3 hours  
**Production Impact:** HIGH (Essential for production monitoring)
