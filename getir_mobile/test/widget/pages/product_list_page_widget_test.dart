import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/pages/merchant/product_list_page.dart';
import 'package:getir_mobile/presentation/bloc/product/product_bloc.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([ProductBloc, CartBloc])
import 'product_list_page_widget_test.mocks.dart';

void main() {
  late MockProductBloc mockProductBloc;
  late MockCartBloc mockCartBloc;

  setUp(() {
    mockProductBloc = MockProductBloc();
    mockCartBloc = MockCartBloc();
  });

  Widget createWidgetUnderTest() {
    return MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: MultiBlocProvider(
        providers: [
          BlocProvider<ProductBloc>.value(value: mockProductBloc),
          BlocProvider<CartBloc>.value(value: mockCartBloc),
        ],
        child: const Scaffold(
          body: ProductListPage(merchantId: 'merchant-123'),
        ),
      ),
    );
  }

  group('ProductListPage Widget Tests -', () {
    testWidgets('renders product list page', (tester) async {
      // Arrange
      final productsState = ProductsLoaded([MockData.testProduct]);
      when(mockProductBloc.state).thenReturn(productsState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productsState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byType(ProductListPage), findsOneWidget);
    });

    testWidgets('displays products when loaded', (tester) async {
      // Arrange
      final productsState = ProductsLoaded([MockData.testProduct, MockData.testProduct2]);
      when(mockProductBloc.state).thenReturn(productsState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productsState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.text('Test Ürün'), findsOneWidget);
      expect(find.text('Test Ürün 2'), findsOneWidget);
    });

    testWidgets('shows empty state when no products', (tester) async {
      // Arrange
      const productsState = ProductsLoaded([]);
      when(mockProductBloc.state).thenReturn(productsState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productsState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.inventory_2_outlined), findsOneWidget);
    });

    testWidgets('shows search bar', (tester) async {
      // Arrange
      const productsState = ProductsLoaded([]);
      when(mockProductBloc.state).thenReturn(productsState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productsState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.search), findsOneWidget);
      expect(find.byType(TextField), findsOneWidget);
    });

    testWidgets('displays error state', (tester) async {
      // Arrange
      const errorState = ProductError('Failed to load');
      when(mockProductBloc.state).thenReturn(errorState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(errorState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.error_outline), findsOneWidget);
      expect(find.text('Failed to load'), findsOneWidget);
    });

    testWidgets('displays product ratings', (tester) async {
      // Arrange
      final productsState = ProductsLoaded([MockData.testProduct]);
      when(mockProductBloc.state).thenReturn(productsState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productsState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.star), findsOneWidget);
      expect(find.text('4.2'), findsOneWidget);
    });
  });
}
