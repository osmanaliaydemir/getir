import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/pages/cart/cart_page.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/domain/entities/cart.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([CartBloc])
import 'cart_page_widget_test.mocks.dart';

void main() {
  late MockCartBloc mockCartBloc;

  setUp(() {
    mockCartBloc = MockCartBloc();
  });

  Widget createWidgetUnderTest() {
    return MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: BlocProvider<CartBloc>.value(
        value: mockCartBloc,
        child: const CartPage(),
      ),
    );
  }

  group('CartPage Widget Tests -', () {
    testWidgets('renders cart page', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert - Just verify the page renders
      expect(find.byType(CartPage), findsOneWidget);
    });

    testWidgets('shows cart items when loaded', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.text('Test Ürün'), findsOneWidget);
    });

    testWidgets('shows empty state when cart is empty', (tester) async {
      // Arrange
      final emptyState = CartLoaded(MockData.emptyCart);
      when(mockCartBloc.state).thenReturn(emptyState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(emptyState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.shopping_cart_outlined), findsOneWidget);
    });

    testWidgets('shows error state when error occurs', (tester) async {
      // Arrange
      const errorState = CartError('Test error');
      when(mockCartBloc.state).thenReturn(errorState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(errorState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.error_outline), findsOneWidget);
      expect(find.text('Test error'), findsOneWidget);
    });

    testWidgets('shows quantity controls', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.add), findsWidgets);
      expect(find.byIcon(Icons.remove), findsWidgets);
    });

    testWidgets('triggers event when add button tapped', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Tap add button
      final addButtons = find.byIcon(Icons.add);
      if (addButtons.evaluate().isNotEmpty) {
        await tester.tap(addButtons.first);
        await tester.pump();
      }

      // Assert
      verify(mockCartBloc.add(any)).called(greaterThan(0));
    });
  });
}
