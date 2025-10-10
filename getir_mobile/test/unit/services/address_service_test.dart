import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/data/datasources/address_datasource.dart';
import 'package:getir_mobile/domain/entities/address.dart';
import 'package:getir_mobile/domain/repositories/address_repository.dart';
import 'package:getir_mobile/domain/services/address_service.dart';

import '../../helpers/mock_data.dart';
import 'address_service_test.mocks.dart';

@GenerateMocks([IAddressRepository])
void main() {
  late AddressService service;
  late MockIAddressRepository mockRepository;

  setUp(() {
    mockRepository = MockIAddressRepository();
    service = AddressService(mockRepository);
  });

  group('AddressService -', () {
    group('getUserAddresses', () {
      test('returns list of addresses when repository call succeeds', () async {
        // Arrange
        final addresses = [MockData.testAddress];
        when(
          mockRepository.getUserAddresses(),
        ).thenAnswer((_) async => Result.success(addresses));

        // Act
        final result = await service.getUserAddresses();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, addresses);
        verify(mockRepository.getUserAddresses()).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getUserAddresses(),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getUserAddresses();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getUserAddresses()).called(1);
      });
    });

    group('getAddressById', () {
      const addressId = 'address-123';

      test('returns address when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getAddressById(addressId),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));

        // Act
        final result = await service.getAddressById(addressId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockRepository.getAddressById(addressId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Address not found');
        when(
          mockRepository.getAddressById(addressId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getAddressById(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.getAddressById(addressId)).called(1);
      });
    });

    group('createAddress', () {
      final createRequest = CreateAddressRequest(
        title: 'Home',
        fullAddress: 'Test Address',
        latitude: 41.0082,
        longitude: 28.9784,
        type: AddressType.home,
        isDefault: true,
      );

      test('creates address when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.createAddress(createRequest),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));

        // Act
        final result = await service.createAddress(createRequest);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockRepository.createAddress(createRequest)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = ValidationException(message: 'Invalid address data');
        when(
          mockRepository.createAddress(createRequest),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.createAddress(createRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.createAddress(createRequest)).called(1);
      });
    });

    group('updateAddress', () {
      const addressId = 'address-123';
      final updateRequest = UpdateAddressRequest(
        title: 'Updated Home',
        fullAddress: 'Updated Address',
      );

      test('updates address when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.updateAddress(addressId, updateRequest),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));

        // Act
        final result = await service.updateAddress(addressId, updateRequest);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(
          mockRepository.updateAddress(addressId, updateRequest),
        ).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Address not found');
        when(
          mockRepository.updateAddress(addressId, updateRequest),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.updateAddress(addressId, updateRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(
          mockRepository.updateAddress(addressId, updateRequest),
        ).called(1);
      });
    });

    group('deleteAddress', () {
      const addressId = 'address-123';

      test('deletes address when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.deleteAddress(addressId),
        ).thenAnswer((_) async => Result.success(null));

        // Act
        final result = await service.deleteAddress(addressId);

        // Assert
        expect(result.isSuccess, true);
        verify(mockRepository.deleteAddress(addressId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Address not found');
        when(
          mockRepository.deleteAddress(addressId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.deleteAddress(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.deleteAddress(addressId)).called(1);
      });
    });

    group('setDefaultAddress', () {
      const addressId = 'address-123';

      test('sets default address when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.setDefaultAddress(addressId),
        ).thenAnswer((_) async => Result.success(MockData.testAddress));

        // Act
        final result = await service.setDefaultAddress(addressId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockRepository.setDefaultAddress(addressId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Address not found');
        when(
          mockRepository.setDefaultAddress(addressId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.setDefaultAddress(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.setDefaultAddress(addressId)).called(1);
      });
    });
  });
}
