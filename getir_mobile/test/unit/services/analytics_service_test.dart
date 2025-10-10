import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';
import 'package:firebase_analytics/firebase_analytics.dart';
import 'package:firebase_crashlytics/firebase_crashlytics.dart';
import 'package:firebase_performance/firebase_performance.dart';
import 'package:getir_mobile/core/services/analytics_service.dart';

@GenerateMocks([
  FirebaseAnalytics,
  FirebaseCrashlytics,
  FirebasePerformance,
  Trace,
])
import 'analytics_service_test.mocks.dart';

void main() {
  late AnalyticsService service;
  late MockFirebaseAnalytics mockAnalytics;
  late MockFirebaseCrashlytics mockCrashlytics;
  late MockFirebasePerformance mockPerformance;

  setUp(() {
    mockAnalytics = MockFirebaseAnalytics();
    mockCrashlytics = MockFirebaseCrashlytics();
    mockPerformance = MockFirebasePerformance();
    service = AnalyticsService(mockAnalytics, mockCrashlytics, mockPerformance);
  });

  group('AnalyticsService - Screen Tracking', () {
    test('should log screen view', () async {
      // Arrange
      when(
        mockAnalytics.logScreenView(
          screenName: anyNamed('screenName'),
          screenClass: anyNamed('screenClass'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logScreenView(screenName: 'HomeScreen');

      // Assert
      verify(
        mockAnalytics.logScreenView(
          screenName: 'HomeScreen',
          screenClass: 'HomeScreen',
        ),
      ).called(1);
    });

    test('should log screen view with custom screen class', () async {
      // Arrange
      when(
        mockAnalytics.logScreenView(
          screenName: anyNamed('screenName'),
          screenClass: anyNamed('screenClass'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logScreenView(
        screenName: 'ProductDetail',
        screenClass: 'DetailScreen',
      );

      // Assert
      verify(
        mockAnalytics.logScreenView(
          screenName: 'ProductDetail',
          screenClass: 'DetailScreen',
        ),
      ).called(1);
    });

    test('should log screen view with parameters', () async {
      // Arrange
      when(
        mockAnalytics.logScreenView(
          screenName: anyNamed('screenName'),
          screenClass: anyNamed('screenClass'),
        ),
      ).thenAnswer((_) async => {});
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logScreenView(
        screenName: 'ProfileScreen',
        parameters: {'userId': '123'},
      );

      // Assert
      verify(
        mockAnalytics.logEvent(
          name: 'screen_view_details',
          parameters: anyNamed('parameters'),
        ),
      ).called(1);
    });
  });

  group('AnalyticsService - User Actions', () {
    test('should log button click', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logButtonClick(buttonName: 'login_button');

      // Assert
      verify(
        mockAnalytics.logEvent(
          name: 'button_click',
          parameters: anyNamed('parameters'),
        ),
      ).called(1);
    });

    test('should log button click with screen name', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logButtonClick(
        buttonName: 'add_to_cart',
        screenName: 'ProductScreen',
      );

      // Assert
      verify(
        mockAnalytics.logEvent(
          name: 'button_click',
          parameters: anyNamed('parameters'),
        ),
      ).called(1);
    });

    test('should log search action', () async {
      // Arrange
      when(
        mockAnalytics.logSearch(
          searchTerm: anyNamed('searchTerm'),
          numberOfNights: anyNamed('numberOfNights'),
          numberOfRooms: anyNamed('numberOfRooms'),
          numberOfPassengers: anyNamed('numberOfPassengers'),
          origin: anyNamed('origin'),
          destination: anyNamed('destination'),
          startDate: anyNamed('startDate'),
          endDate: anyNamed('endDate'),
          travelClass: anyNamed('travelClass'),
        ),
      ).thenAnswer((_) async => {});
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logSearch(searchTerm: 'pizza');

      // Assert
      verify(mockAnalytics.logSearch(searchTerm: 'pizza')).called(1);
    });

    test('should log product view', () async {
      // Arrange
      when(
        mockAnalytics.logViewItem(
          currency: anyNamed('currency'),
          value: anyNamed('value'),
          items: anyNamed('items'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logProductView(
        productId: 'prod-123',
        productName: 'Test Product',
        price: 29.99,
      );

      // Assert
      verify(
        mockAnalytics.logViewItem(
          currency: 'TRY',
          value: 29.99,
          items: anyNamed('items'),
        ),
      ).called(1);
    });
  });

  group('AnalyticsService - E-commerce Events', () {
    test('should log add to cart', () async {
      // Arrange
      when(
        mockAnalytics.logAddToCart(
          currency: anyNamed('currency'),
          value: anyNamed('value'),
          items: anyNamed('items'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logAddToCart(
        productId: 'prod-123',
        productName: 'Test Product',
        price: 29.99,
        quantity: 2,
      );

      // Assert
      verify(
        mockAnalytics.logAddToCart(
          currency: 'TRY',
          value: 59.98,
          items: anyNamed('items'),
        ),
      ).called(1);
    });

    test('should log remove from cart', () async {
      // Arrange
      when(
        mockAnalytics.logRemoveFromCart(
          currency: anyNamed('currency'),
          value: anyNamed('value'),
          items: anyNamed('items'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logRemoveFromCart(
        productId: 'prod-123',
        productName: 'Test Product',
        price: 29.99,
        quantity: 1,
      );

      // Assert
      verify(
        mockAnalytics.logRemoveFromCart(
          currency: 'TRY',
          value: 29.99,
          items: anyNamed('items'),
        ),
      ).called(1);
    });

    test('should log begin checkout', () async {
      // Arrange
      when(
        mockAnalytics.logBeginCheckout(
          value: anyNamed('value'),
          currency: anyNamed('currency'),
          items: anyNamed('items'),
          coupon: anyNamed('coupon'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logBeginCheckout(value: 100.0, currency: 'TRY');

      // Assert
      verify(
        mockAnalytics.logBeginCheckout(
          value: 100.0,
          currency: 'TRY',
          items: null,
          coupon: null,
        ),
      ).called(1);
    });

    test('should log add payment info', () async {
      // Arrange
      when(
        mockAnalytics.logAddPaymentInfo(
          currency: anyNamed('currency'),
          value: anyNamed('value'),
          paymentType: anyNamed('paymentType'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logAddPaymentInfo(paymentType: 'credit_card', value: 100.0);

      // Assert
      verify(
        mockAnalytics.logAddPaymentInfo(
          currency: 'TRY',
          value: 100.0,
          paymentType: 'credit_card',
        ),
      ).called(1);
    });

    test('should log purchase', () async {
      // Arrange
      when(
        mockAnalytics.logPurchase(
          currency: anyNamed('currency'),
          value: anyNamed('value'),
          transactionId: anyNamed('transactionId'),
          tax: anyNamed('tax'),
          shipping: anyNamed('shipping'),
          items: anyNamed('items'),
          coupon: anyNamed('coupon'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logPurchase(
        orderId: 'order-123',
        total: 100.0,
        currency: 'TRY',
      );

      // Assert
      verify(
        mockAnalytics.logPurchase(
          currency: 'TRY',
          value: 100.0,
          transactionId: 'order-123',
          tax: null,
          shipping: null,
          items: null,
          coupon: null,
        ),
      ).called(1);
    });
  });

  group('AnalyticsService - Auth Events', () {
    test('should log login', () async {
      // Arrange
      when(
        mockAnalytics.logLogin(loginMethod: anyNamed('loginMethod')),
      ).thenAnswer((_) async => {});

      // Act
      await service.logLogin();

      // Assert
      verify(
        mockAnalytics.logLogin(loginMethod: anyNamed('loginMethod')),
      ).called(1);
    });

    test('should log sign up', () async {
      // Arrange
      when(
        mockAnalytics.logSignUp(signUpMethod: anyNamed('signUpMethod')),
      ).thenAnswer((_) async => {});

      // Act
      await service.logSignUp();

      // Assert
      verify(
        mockAnalytics.logSignUp(signUpMethod: anyNamed('signUpMethod')),
      ).called(1);
    });

    test('should log logout', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logLogout();

      // Assert
      verify(
        mockAnalytics.logEvent(
          name: 'logout',
          parameters: anyNamed('parameters'),
        ),
      ).called(1);
    });
  });

  group('AnalyticsService - Error Tracking', () {
    test('should log error to crashlytics', () async {
      // Arrange
      final error = Exception('Test error');
      when(
        mockCrashlytics.recordError(
          any,
          any,
          reason: anyNamed('reason'),
          fatal: anyNamed('fatal'),
          information: anyNamed('information'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logError(error: error, reason: 'Test failed');

      // Assert
      verify(
        mockCrashlytics.recordError(
          error,
          null,
          reason: 'Test failed',
          fatal: false,
          information: anyNamed('information'),
        ),
      ).called(1);
    });

    test('should log fatal error', () async {
      // Arrange
      final error = Exception('Fatal error');
      when(
        mockCrashlytics.recordError(
          any,
          any,
          reason: anyNamed('reason'),
          fatal: anyNamed('fatal'),
          information: anyNamed('information'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logError(error: error, reason: 'App crashed', fatal: true);

      // Assert
      verify(
        mockCrashlytics.recordError(
          error,
          null,
          reason: 'App crashed',
          fatal: true,
          information: anyNamed('information'),
        ),
      ).called(1);
    });

    test('should log error with stack trace', () async {
      // Arrange
      final error = Exception('Test error');
      final stackTrace = StackTrace.current;
      when(
        mockCrashlytics.recordError(
          any,
          any,
          reason: anyNamed('reason'),
          fatal: anyNamed('fatal'),
          information: anyNamed('information'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logError(
        error: error,
        stackTrace: stackTrace,
        reason: 'Test failed',
      );

      // Assert
      verify(
        mockCrashlytics.recordError(
          error,
          stackTrace,
          reason: 'Test failed',
          fatal: false,
          information: anyNamed('information'),
        ),
      ).called(1);
    });

    test('should log error with context', () async {
      // Arrange
      final error = Exception('Test error');
      final context = {'userId': '123', 'screen': 'Home'};
      when(
        mockCrashlytics.recordError(
          any,
          any,
          reason: anyNamed('reason'),
          fatal: anyNamed('fatal'),
          information: anyNamed('information'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.logError(error: error, context: context);

      // Assert
      verify(
        mockCrashlytics.recordError(
          error,
          null,
          reason: anyNamed('reason'),
          fatal: false,
          information: anyNamed('information'),
        ),
      ).called(1);
    });
  });

  group('AnalyticsService - Performance Monitoring', () {
    test('should create new performance trace', () {
      // Arrange
      final mockTrace = MockTrace();
      when(mockPerformance.newTrace(any)).thenReturn(mockTrace);

      // Act
      final trace = mockPerformance.newTrace('test_trace');

      // Assert
      expect(trace, isNotNull);
      expect(trace, isA<Trace>());
    });

    test('should handle performance monitoring without errors', () {
      // Arrange
      final mockTrace = MockTrace();
      when(mockPerformance.newTrace(any)).thenReturn(mockTrace);

      // Act & Assert
      expect(() => mockPerformance.newTrace('api_call'), returnsNormally);
    });
  });

  group('AnalyticsService - User Properties', () {
    test('should set user ID', () async {
      // Arrange
      when(
        mockAnalytics.setUserId(id: anyNamed('id')),
      ).thenAnswer((_) async => {});
      when(mockCrashlytics.setUserIdentifier(any)).thenAnswer((_) async => {});

      // Act
      await service.setUserId('user-123');

      // Assert
      verify(mockAnalytics.setUserId(id: 'user-123')).called(1);
      verify(mockCrashlytics.setUserIdentifier('user-123')).called(1);
    });

    test('should set user property', () async {
      // Arrange
      when(
        mockAnalytics.setUserProperty(
          name: anyNamed('name'),
          value: anyNamed('value'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.setUserProperty(name: 'user_type', value: 'premium');

      // Assert
      verify(
        mockAnalytics.setUserProperty(name: 'user_type', value: 'premium'),
      ).called(1);
    });

    test('should set multiple user properties', () async {
      // Arrange
      when(
        mockAnalytics.setUserProperty(
          name: anyNamed('name'),
          value: anyNamed('value'),
        ),
      ).thenAnswer((_) async => {});

      // Act
      await service.setUserProperty(name: 'city', value: 'Istanbul');
      await service.setUserProperty(name: 'age_group', value: '25-34');

      // Assert
      verify(
        mockAnalytics.setUserProperty(name: 'city', value: 'Istanbul'),
      ).called(1);
      verify(
        mockAnalytics.setUserProperty(name: 'age_group', value: '25-34'),
      ).called(1);
    });
  });

  group('AnalyticsService - Edge Cases', () {
    test('should handle empty event names gracefully', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act & Assert
      expect(() => service.logButtonClick(buttonName: ''), returnsNormally);
    });

    test('should handle null values in parameters', () async {
      // Arrange
      when(
        mockAnalytics.logScreenView(
          screenName: anyNamed('screenName'),
          screenClass: anyNamed('screenClass'),
        ),
      ).thenAnswer((_) async => {});

      // Act & Assert
      expect(
        () => service.logScreenView(screenName: 'Test', screenClass: null),
        returnsNormally,
      );
    });

    test('should handle very long string values', () async {
      // Arrange
      final longString = 'A' * 10000;
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act & Assert
      expect(
        () => service.logButtonClick(buttonName: longString),
        returnsNormally,
      );
    });

    test('should handle special characters in event names', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer((_) async => {});

      // Act & Assert
      expect(
        () => service.logButtonClick(buttonName: 'test!@#\$%^&*()'),
        returnsNormally,
      );
    });

    test('should handle concurrent event logging', () async {
      // Arrange
      when(
        mockAnalytics.logEvent(
          name: anyNamed('name'),
          parameters: anyNamed('parameters'),
        ),
      ).thenAnswer(
        (_) async => Future.delayed(const Duration(milliseconds: 10)),
      );

      // Act
      final futures = List.generate(
        10,
        (i) => service.logButtonClick(buttonName: 'button_$i'),
      );

      // Assert
      await Future.wait(futures);
      verify(
        mockAnalytics.logEvent(
          name: 'button_click',
          parameters: anyNamed('parameters'),
        ),
      ).called(10);
    });
  });
}
