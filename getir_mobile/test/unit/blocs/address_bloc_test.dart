import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:getir_mobile/presentation/bloc/address/address_bloc.dart';
import 'package:getir_mobile/domain/services/address_service.dart';
import 'package:getir_mobile/domain/entities/address.dart';
import 'package:getir_mobile/data/datasources/address_datasource.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/core/errors/app_exceptions.dart';
import '../../helpers/mock_data.dart';

@GenerateMocks([AddressService])
import 'address_bloc_test.mocks.dart';

void main() {
  late AddressBloc bloc;
  late MockAddressService mockService;

  setUp(() {
    mockService = MockAddressService();
    bloc = AddressBloc(mockService);
  });

  group('AddressBloc -', () {
    test('initial state is AddressInitial', () {
      expect(bloc.state, AddressInitial());
    });

    blocTest<AddressBloc, AddressState>(
      'LoadUserAddresses emits [AddressLoading, AddressesLoaded] when addresses loaded',
      build: () {
        when(
          mockService.getUserAddresses(),
        ).thenAnswer((_) async => Result.success([MockData.testAddress]));
        return bloc;
      },
      act: (bloc) => bloc.add(LoadUserAddresses()),
      expect: () => [AddressLoading(), isA<AddressesLoaded>()],
    );

    blocTest<AddressBloc, AddressState>(
      'LoadUserAddresses emits [AddressLoading, AddressesLoaded] with empty list',
      build: () {
        when(
          mockService.getUserAddresses(),
        ).thenAnswer((_) async => Result.success([]));
        return bloc;
      },
      act: (bloc) => bloc.add(LoadUserAddresses()),
      expect: () => [AddressLoading(), const AddressesLoaded([])],
    );

    blocTest<AddressBloc, AddressState>(
      'LoadUserAddresses emits [AddressLoading, AddressError] when loading fails',
      build: () {
        when(mockService.getUserAddresses()).thenAnswer(
          (_) async =>
              Result.failure(const NetworkException(message: 'Network error')),
        );
        return bloc;
      },
      act: (bloc) => bloc.add(LoadUserAddresses()),
      expect: () => [AddressLoading(), isA<AddressError>()],
    );

    blocTest<AddressBloc, AddressState>(
      'LoadAddressById emits [AddressLoading, AddressLoaded] when address found',
      build: () {
        when(
          mockService.getAddressById(any),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));
        return bloc;
      },
      act: (bloc) => bloc.add(const LoadAddressById('address-123')),
      expect: () => [AddressLoading(), isA<AddressLoaded>()],
    );

    blocTest<AddressBloc, AddressState>(
      'CreateAddress emits [AddressLoading, AddressCreated] when address created',
      build: () {
        when(
          mockService.createAddress(any),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));
        return bloc;
      },
      act: (bloc) => bloc.add(
        CreateAddress(
          CreateAddressRequest(
            title: 'Home',
            fullAddress: 'Test Address',
            latitude: 41.0082,
            longitude: 28.9784,
            type: AddressType.home,
            isDefault: true,
          ),
        ),
      ),
      expect: () => [AddressLoading(), isA<AddressCreated>()],
    );

    blocTest<AddressBloc, AddressState>(
      'UpdateAddress emits [AddressLoading, AddressUpdated] when address updated',
      build: () {
        when(
          mockService.updateAddress(any, any),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));
        return bloc;
      },
      act: (bloc) => bloc.add(
        UpdateAddress(
          'address-123',
          const UpdateAddressRequest(
            title: 'Updated Home',
            fullAddress: 'Updated Address',
          ),
        ),
      ),
      expect: () => [AddressLoading(), isA<AddressUpdated>()],
    );

    blocTest<AddressBloc, AddressState>(
      'DeleteAddress emits [AddressLoading, AddressDeleted] when address deleted',
      build: () {
        when(
          mockService.deleteAddress(any),
        ).thenAnswer((_) async => Result.success(null));
        return bloc;
      },
      act: (bloc) => bloc.add(const DeleteAddress('address-123')),
      expect: () => [AddressLoading(), const AddressDeleted('address-123')],
    );

    blocTest<AddressBloc, AddressState>(
      'SetDefaultAddress emits [AddressLoading, AddressDefaultSet] when set as default',
      build: () {
        when(
          mockService.setDefaultAddress(any),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));
        return bloc;
      },
      act: (bloc) => bloc.add(const SetDefaultAddress('address-123')),
      expect: () => [AddressLoading(), isA<DefaultAddressSet>()],
    );
  });
}
