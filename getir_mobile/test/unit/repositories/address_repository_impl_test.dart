import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/address_datasource.dart';
import 'package:getir_mobile/data/repositories/address_repository_impl.dart';
import 'package:getir_mobile/domain/entities/address.dart';

import '../../helpers/mock_data.dart';
import 'address_repository_impl_test.mocks.dart';

@GenerateMocks([IAddressDataSource])
void main() {
  late AddressRepositoryImpl repository;
  late MockIAddressDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockIAddressDataSource();
    repository = AddressRepositoryImpl(mockDataSource);
  });

  group('AddressRepositoryImpl -', () {
    group('getUserAddresses', () {
      test('returns success with addresses when datasource succeeds', () async {
        // Arrange
        final addresses = [MockData.testAddress];
        when(
          mockDataSource.getUserAddresses(),
        ).thenAnswer((_) async => addresses);

        // Act
        final result = await repository.getUserAddresses();

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, addresses);
        verify(mockDataSource.getUserAddresses()).called(1);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/addresses'),
          type: DioExceptionType.connectionTimeout,
        );
        when(mockDataSource.getUserAddresses()).thenThrow(dioException);

        // Act
        final result = await repository.getUserAddresses();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });

      test('returns failure when AppException occurs', () async {
        // Arrange
        const appException = ApiException(message: 'Address error');
        when(mockDataSource.getUserAddresses()).thenThrow(appException);

        // Act
        final result = await repository.getUserAddresses();

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, appException);
      });
    });

    group('getAddressById', () {
      const addressId = 'address-123';

      test('returns success with address when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.getAddressById(addressId),
        ).thenAnswer((_) async => MockData.testAddress);

        // Act
        final result = await repository.getAddressById(addressId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockDataSource.getAddressById(addressId)).called(1);
      });

      test('returns NotFoundException when address not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/addresses/$addressId'),
          response: Response(
            requestOptions: RequestOptions(path: '/addresses/$addressId'),
            statusCode: 404,
            data: {'message': 'Address not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.getAddressById(addressId)).thenThrow(dioException);

        // Act
        final result = await repository.getAddressById(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
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

      test('returns success with address when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.createAddress(createRequest),
        ).thenAnswer((_) async => MockData.testAddress);

        // Act
        final result = await repository.createAddress(createRequest);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockDataSource.createAddress(createRequest)).called(1);
      });

      test('returns ValidationException when validation fails', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/addresses'),
          response: Response(
            requestOptions: RequestOptions(path: '/addresses'),
            statusCode: 400,
            data: {'message': 'Invalid address data'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.createAddress(createRequest),
        ).thenThrow(dioException);

        // Act
        final result = await repository.createAddress(createRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });
    });

    group('updateAddress', () {
      const addressId = 'address-123';
      final updateRequest = UpdateAddressRequest(
        title: 'Updated Home',
        fullAddress: 'Updated Address',
      );

      test(
        'returns success with updated address when datasource succeeds',
        () async {
          // Arrange
          when(
            mockDataSource.updateAddress(addressId, updateRequest),
          ).thenAnswer((_) async => MockData.testAddress);

          // Act
          final result = await repository.updateAddress(
            addressId,
            updateRequest,
          );

          // Assert
          expect(result.isSuccess, true);
          expect(result.data, MockData.testAddress);
          verify(
            mockDataSource.updateAddress(addressId, updateRequest),
          ).called(1);
        },
      );

      test('returns NotFoundException when address not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/addresses/$addressId'),
          response: Response(
            requestOptions: RequestOptions(path: '/addresses/$addressId'),
            statusCode: 404,
            data: {'message': 'Address not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.updateAddress(addressId, updateRequest),
        ).thenThrow(dioException);

        // Act
        final result = await repository.updateAddress(addressId, updateRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('deleteAddress', () {
      const addressId = 'address-123';

      test('returns success when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.deleteAddress(addressId),
        ).thenAnswer((_) async => {});

        // Act
        final result = await repository.deleteAddress(addressId);

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.deleteAddress(addressId)).called(1);
      });

      test('returns NotFoundException when address not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/addresses/$addressId'),
          response: Response(
            requestOptions: RequestOptions(path: '/addresses/$addressId'),
            statusCode: 404,
            data: {'message': 'Address not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(mockDataSource.deleteAddress(addressId)).thenThrow(dioException);

        // Act
        final result = await repository.deleteAddress(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('setDefaultAddress', () {
      const addressId = 'address-123';

      test('returns success with address when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.setDefaultAddress(addressId),
        ).thenAnswer((_) async => MockData.testAddress);

        // Act
        final result = await repository.setDefaultAddress(addressId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, MockData.testAddress);
        verify(mockDataSource.setDefaultAddress(addressId)).called(1);
      });

      test('returns NotFoundException when address not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/addresses/$addressId/set-default',
          ),
          response: Response(
            requestOptions: RequestOptions(
              path: '/addresses/$addressId/set-default',
            ),
            statusCode: 404,
            data: {'message': 'Address not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.setDefaultAddress(addressId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.setDefaultAddress(addressId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });
  });
}
