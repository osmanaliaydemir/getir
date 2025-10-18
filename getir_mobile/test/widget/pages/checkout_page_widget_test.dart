import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/pages/checkout/checkout_page.dart';
import 'package:getir_mobile/presentation/bloc/cart/cart_bloc.dart';
import 'package:getir_mobile/presentation/bloc/address/address_bloc.dart';
import 'package:getir_mobile/presentation/bloc/order/order_bloc.dart';
import 'package:getir_mobile/presentation/bloc/merchant/merchant_bloc.dart';
import 'package:getir_mobile/presentation/bloc/working_hours/working_hours_bloc.dart';
import 'package:getir_mobile/presentation/bloc/working_hours/working_hours_state.dart';
import 'package:getir_mobile/domain/entities/order.dart';
import 'package:getir_mobile/core/localization/app_localizations.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([
  CartBloc,
  AddressBloc,
  OrderBloc,
  MerchantBloc,
  WorkingHoursBloc,
])
import 'checkout_page_widget_test.mocks.dart';

void main() {
  late MockCartBloc mockCartBloc;
  late MockAddressBloc mockAddressBloc;
  late MockOrderBloc mockOrderBloc;
  late MockMerchantBloc mockMerchantBloc;
  late MockWorkingHoursBloc mockWorkingHoursBloc;

  setUp(() {
    mockCartBloc = MockCartBloc();
    mockAddressBloc = MockAddressBloc();
    mockOrderBloc = MockOrderBloc();
    mockMerchantBloc = MockMerchantBloc();
    mockWorkingHoursBloc = MockWorkingHoursBloc();
  });

  Widget createWidgetUnderTest() {
    return MaterialApp(
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      home: MultiBlocProvider(
        providers: [
          BlocProvider<CartBloc>.value(value: mockCartBloc),
          BlocProvider<AddressBloc>.value(value: mockAddressBloc),
          BlocProvider<OrderBloc>.value(value: mockOrderBloc),
          BlocProvider<MerchantBloc>.value(value: mockMerchantBloc),
          BlocProvider<WorkingHoursBloc>.value(value: mockWorkingHoursBloc),
        ],
        child: const CheckoutPage(),
      ),
    );
  }

  group('CheckoutPage Widget Tests -', () {
    testWidgets('renders checkout page', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));
      when(mockAddressBloc.state).thenReturn(AddressInitial());
      when(
        mockAddressBloc.stream,
      ).thenAnswer((_) => Stream.value(AddressInitial()));
      when(mockOrderBloc.state).thenReturn(OrderInitial());
      when(
        mockOrderBloc.stream,
      ).thenAnswer((_) => Stream.value(OrderInitial()));
      when(mockMerchantBloc.state).thenReturn(MerchantInitial());
      when(
        mockMerchantBloc.stream,
      ).thenAnswer((_) => Stream.value(MerchantInitial()));
      when(mockWorkingHoursBloc.state).thenReturn(WorkingHoursInitial());
      when(
        mockWorkingHoursBloc.stream,
      ).thenAnswer((_) => Stream.value(WorkingHoursInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.byType(CheckoutPage), findsOneWidget);
    });

    testWidgets('displays payment methods', (tester) async {
      // Arrange
      final cartState = CartLoaded(MockData.testCart);
      when(mockCartBloc.state).thenReturn(cartState);
      when(mockCartBloc.stream).thenAnswer((_) => Stream.value(cartState));
      when(mockAddressBloc.state).thenReturn(AddressInitial());
      when(
        mockAddressBloc.stream,
      ).thenAnswer((_) => Stream.value(AddressInitial()));
      when(mockOrderBloc.state).thenReturn(OrderInitial());
      when(
        mockOrderBloc.stream,
      ).thenAnswer((_) => Stream.value(OrderInitial()));
      when(mockMerchantBloc.state).thenReturn(MerchantInitial());
      when(
        mockMerchantBloc.stream,
      ).thenAnswer((_) => Stream.value(MerchantInitial()));
      when(mockWorkingHoursBloc.state).thenReturn(WorkingHoursInitial());
      when(
        mockWorkingHoursBloc.stream,
      ).thenAnswer((_) => Stream.value(WorkingHoursInitial()));

      // Act
      await tester.pumpWidget(createWidgetUnderTest());
      await tester.pump();

      // Assert
      expect(find.text('Nakit'), findsOneWidget);
      expect(find.text('Kart'), findsOneWidget);
    });
  });
}
