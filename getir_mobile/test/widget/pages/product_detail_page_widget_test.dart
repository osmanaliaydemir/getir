import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/pages/product/product_detail_page.dart';
import 'package:getir_mobile/presentation/bloc/product/product_bloc.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([ProductBloc, CartBloc])
import 'product_detail_page_widget_test.mocks.dart';

void main() {
  late MockProductBloc mockProductBloc;
  late MockCartBloc mockCartBloc;

  setUp(() {
    mockProductBloc = MockProductBloc();
    mockCartBloc = MockCartBloc();
  });

  Widget createWidgetUnderTest({String productId = 'product-123'}) {
    return MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: MultiBlocProvider(
        providers: [
          BlocProvider<ProductBloc>.value(value: mockProductBloc),
          BlocProvider<CartBloc>.value(value: mockCartBloc),
        ],
        child: ProductDetailPage(productId: productId),
      ),
    );
  }

  group('ProductDetailPage Widget Tests -', () {
    testWidgets('renders product detail page', (tester) async {
      // Arrange
      final productState = ProductLoaded(MockData.testProduct);
      when(mockProductBloc.state).thenReturn(productState);
      when(
        mockProductBloc.stream,
      ).thenAnswer((_) => Stream.value(productState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byType(ProductDetailPage), findsOneWidget);
    });

    testWidgets('displays product details', (tester) async {
      // Arrange
      final productState = ProductLoaded(MockData.testProduct);
      when(mockProductBloc.state).thenReturn(productState);
      when(
        mockProductBloc.stream,
      ).thenAnswer((_) => Stream.value(productState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.text('Test Ürün'), findsOneWidget);
    });

    testWidgets('shows error state', (tester) async {
      // Arrange
      const errorState = ProductError('Product not found');
      when(mockProductBloc.state).thenReturn(errorState);
      when(mockProductBloc.stream).thenAnswer((_) => Stream.value(errorState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.error_outline), findsOneWidget);
      expect(find.text('Product not found'), findsOneWidget);
    });

    testWidgets('displays quantity controls', (tester) async {
      // Arrange
      final productState = ProductLoaded(MockData.testProduct);
      when(mockProductBloc.state).thenReturn(productState);
      when(
        mockProductBloc.stream,
      ).thenAnswer((_) => Stream.value(productState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.add), findsOneWidget);
      expect(find.byIcon(Icons.remove), findsOneWidget);
    });

    testWidgets('back button is present', (tester) async {
      // Arrange
      final productState = ProductLoaded(MockData.testProduct);
      when(mockProductBloc.state).thenReturn(productState);
      when(
        mockProductBloc.stream,
      ).thenAnswer((_) => Stream.value(productState));
      when(mockCartBloc.state).thenReturn(CartInitial());
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(CartInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byIcon(Icons.arrow_back), findsOneWidget);
    });
  });
}
