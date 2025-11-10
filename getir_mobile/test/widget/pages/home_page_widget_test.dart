import 'package:bloc_test/bloc_test.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:getir_mobile/core/cubits/language/language_cubit.dart';
import 'package:getir_mobile/core/di/injection.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import 'package:getir_mobile/domain/entities/service_category.dart';
import 'package:getir_mobile/domain/entities/service_category_type.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/presentation/bloc/merchant/merchant_bloc.dart';
import 'package:getir_mobile/presentation/bloc/product/product_bloc.dart';
import 'package:getir_mobile/presentation/cubit/category/category_cubit.dart';
import 'package:getir_mobile/presentation/pages/home/home_page.dart';
import 'package:mockito/mockito.dart';

import '../../helpers/mock_data.dart';

class MockCartBloc extends MockBloc<CartEvent, CartState> implements CartBloc {
  final List<CartEvent> recordedEvents = [];

  @override
  void add(CartEvent event) {
    recordedEvents.add(event);
    super.add(event);
  }
}

class MockProductBloc extends MockBloc<ProductEvent, ProductState>
    implements ProductBloc {}

class MockMerchantBloc extends MockBloc<MerchantEvent, MerchantState>
    implements MerchantBloc {}

class MockCategoryCubit extends MockCubit<CategoryState>
    implements CategoryCubit {
  @override
  Future<void> loadAllActiveCategories() => Future.value();

  @override
  Future<void> refreshCategories() => Future.value();
}

class MockLanguageCubit extends MockCubit<LanguageState>
    implements LanguageCubit {}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  const geolocatorChannel = MethodChannel('flutter.baseflow.com/geolocator');

  late MockCartBloc mockCartBloc;
  late MockProductBloc mockProductBloc;
  late MockMerchantBloc mockMerchantBloc;
  late MockCategoryCubit mockCategoryCubit;
  late MockLanguageCubit mockLanguageCubit;
  late ServiceCategory testCategory;
  late CategoryState defaultCategoryState;

  setUp(() async {
    await getIt.reset();

    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(geolocatorChannel, (methodCall) async {
          switch (methodCall.method) {
            case 'checkPermission':
            case 'requestPermission':
              // LocationPermission.denied
              return 1;
            default:
              return null;
          }
        });

    mockCartBloc = MockCartBloc();
    mockProductBloc = MockProductBloc();
    mockMerchantBloc = MockMerchantBloc();
    mockCategoryCubit = MockCategoryCubit();
    mockLanguageCubit = MockLanguageCubit();

    testCategory = ServiceCategory(
      id: 'cat-market',
      name: 'Market',
      description: 'Günlük market ürünleri',
      type: ServiceCategoryType.market,
      imageUrl: null,
      iconUrl: null,
      displayOrder: 1,
      isActive: true,
      merchantCount: 12,
    );

    defaultCategoryState = CategoryLoaded([testCategory]);

    whenListen(
      mockCategoryCubit,
      Stream<CategoryState>.value(defaultCategoryState),
      initialState: defaultCategoryState,
    );

    getIt.registerFactory<CategoryCubit>(() => mockCategoryCubit);
  });

  tearDown(() async {
    await getIt.reset();
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(geolocatorChannel, null);
  });

  Widget createWidgetUnderTest() {
    return MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: MultiBlocProvider(
        providers: [
          BlocProvider<CartBloc>.value(value: mockCartBloc),
          BlocProvider<ProductBloc>.value(value: mockProductBloc),
          BlocProvider<MerchantBloc>.value(value: mockMerchantBloc),
          BlocProvider<LanguageCubit>.value(value: mockLanguageCubit),
        ],
        child: const HomePage(),
      ),
    );
  }

  void arrangeBlocStates({required CartState cartState}) {
    final productState = ProductsLoaded([MockData.testProduct]);
    when(mockProductBloc.state).thenReturn(productState);
    when(mockProductBloc.stream).thenAnswer((_) => Stream.value(productState));

    when(mockCartBloc.state).thenReturn(cartState);
    when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));

    final merchantState = MerchantInitial();
    when(mockMerchantBloc.state).thenReturn(merchantState);
    when(
      mockMerchantBloc.stream,
    ).thenAnswer((_) => Stream.value(merchantState));

    const languageState = LanguageState(locale: Locale('tr', 'TR'));
    when(mockLanguageCubit.state).thenReturn(languageState);
    when(
      mockLanguageCubit.stream,
    ).thenAnswer((_) => Stream.value(languageState));
  }

  group('HomePage cart interactions', () {
    testWidgets('Sepete Ekle butonu AddToCart eventi tetikler', (tester) async {
      arrangeBlocStates(cartState: CartLoaded(MockData.emptyCart));

      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 200));

      mockCartBloc.recordedEvents.clear();

      await tester.tap(find.text('Sepete Ekle').first);
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 300));

      expect(mockCartBloc.recordedEvents, hasLength(1));
      final addEvent = mockCartBloc.recordedEvents.single as AddToCart;
      expect(addEvent.productId, 'product-123');
      expect(addEvent.merchantId, 'merchant-123');
      expect(addEvent.quantity, 1);
    });

    testWidgets(
      'Artı butonu sepet adedini artırmak için UpdateCartItem tetikler',
      (tester) async {
        arrangeBlocStates(cartState: CartLoaded(MockData.testCart));

        await tester.pumpWidget(createWidgetUnderTest());
        await tester.pump();
        await tester.pump(const Duration(milliseconds: 200));

        mockCartBloc.recordedEvents.clear();

        await tester.tap(find.byIcon(Icons.add).first);
        await tester.pump();

        expect(mockCartBloc.recordedEvents, hasLength(1));
        final updateEvent =
            mockCartBloc.recordedEvents.single as UpdateCartItem;
        expect(updateEvent.itemId, 'cart-item-123');
        expect(updateEvent.quantity, 2);
      },
    );

    testWidgets('Eksi butonu adet > 1 iken UpdateCartItem ile azaltır', (
      tester,
    ) async {
      final cartItem = MockData.testCartItem.copyWith(
        quantity: 3,
        totalPrice: MockData.testCartItem.unitPrice * 3,
      );
      final cart = MockData.testCart.copyWith(
        items: [cartItem],
        subtotal: cartItem.unitPrice * 3,
        total: cartItem.unitPrice * 3 + MockData.testCart.deliveryFee,
      );

      arrangeBlocStates(cartState: CartLoaded(cart));

      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 200));

      mockCartBloc.recordedEvents.clear();

      await tester.tap(find.byIcon(Icons.remove).first);
      await tester.pump();

      expect(mockCartBloc.recordedEvents, hasLength(1));
      final updateEvent = mockCartBloc.recordedEvents.single as UpdateCartItem;
      expect(updateEvent.itemId, 'cart-item-123');
      expect(updateEvent.quantity, 2);
    });

    testWidgets('Sil ikonu adet 1 iken RemoveFromCart tetikler', (
      tester,
    ) async {
      arrangeBlocStates(cartState: CartLoaded(MockData.testCart));

      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();
      await tester.pump(const Duration(milliseconds: 200));

      mockCartBloc.recordedEvents.clear();

      await tester.tap(find.byIcon(Icons.delete_outline).first);
      await tester.pump();

      expect(mockCartBloc.recordedEvents, hasLength(1));
      final removeEvent = mockCartBloc.recordedEvents.single as RemoveFromCart;
      expect(removeEvent.itemId, 'cart-item-123');
    });
  });
}
