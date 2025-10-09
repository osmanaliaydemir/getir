import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';
import 'package:dio/dio.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/data/datasources/review_datasource.dart';
import 'package:getir_mobile/data/repositories/review_repository_impl.dart';
import 'package:getir_mobile/domain/entities/review.dart';

import 'review_repository_impl_test.mocks.dart';

@GenerateMocks([ReviewDataSource])
void main() {
  late ReviewRepositoryImpl repository;
  late MockReviewDataSource mockDataSource;

  setUp(() {
    mockDataSource = MockReviewDataSource();
    repository = ReviewRepositoryImpl(mockDataSource);
  });

  final testReview = Review(
    id: 'review-123',
    reviewerId: 'user-123',
    reviewerName: 'Test User',
    revieweeId: 'merchant-123',
    revieweeType: 'Merchant',
    orderId: 'order-123',
    rating: 5,
    comment: 'Great service!',
    createdAt: DateTime(2025, 1, 1),
  );

  const submitRequest = SubmitReviewRequest(
    revieweeId: 'merchant-123',
    revieweeType: 'Merchant',
    orderId: 'order-123',
    rating: 5,
    comment: 'Great service!',
  );

  group('ReviewRepositoryImpl -', () {
    group('submitReview', () {
      test('returns success with review when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.submitReview(submitRequest),
        ).thenAnswer((_) async => testReview);

        // Act
        final result = await repository.submitReview(submitRequest);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testReview);
        verify(mockDataSource.submitReview(submitRequest)).called(1);
      });

      test('returns ValidationException when validation fails', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/reviews'),
          response: Response(
            requestOptions: RequestOptions(path: '/reviews'),
            statusCode: 400,
            data: {'message': 'Invalid review data'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.submitReview(submitRequest),
        ).thenThrow(dioException);

        // Act
        final result = await repository.submitReview(submitRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<ValidationException>());
      });

      test('returns NotFoundException when order not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/reviews'),
          response: Response(
            requestOptions: RequestOptions(path: '/reviews'),
            statusCode: 404,
            data: {'message': 'Order not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.submitReview(submitRequest),
        ).thenThrow(dioException);

        // Act
        final result = await repository.submitReview(submitRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });
    });

    group('getMerchantReviews', () {
      const merchantId = 'merchant-123';

      test('returns success with reviews when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => [testReview]);

        // Act
        final result = await repository.getMerchantReviews(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, [testReview]);
      });

      test('returns empty list when no reviews', () async {
        // Arrange
        when(
          mockDataSource.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => []);

        // Act
        final result = await repository.getMerchantReviews(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(
            path: '/merchants/$merchantId/reviews',
          ),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenThrow(dioException);

        // Act
        final result = await repository.getMerchantReviews(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });

    group('markReviewAsHelpful', () {
      const reviewId = 'review-123';

      test('returns success when datasource succeeds', () async {
        // Arrange
        when(
          mockDataSource.markReviewAsHelpful(reviewId),
        ).thenAnswer((_) async => {});

        // Act
        final result = await repository.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isSuccess, true);
        verify(mockDataSource.markReviewAsHelpful(reviewId)).called(1);
      });

      test('returns NotFoundException when review not found', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/reviews/$reviewId/helpful'),
          response: Response(
            requestOptions: RequestOptions(path: '/reviews/$reviewId/helpful'),
            statusCode: 404,
            data: {'message': 'Review not found'},
          ),
          type: DioExceptionType.badResponse,
        );
        when(
          mockDataSource.markReviewAsHelpful(reviewId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<NotFoundException>());
      });

      test('returns failure when DioException occurs', () async {
        // Arrange
        final dioException = DioException(
          requestOptions: RequestOptions(path: '/reviews/$reviewId/helpful'),
          type: DioExceptionType.connectionTimeout,
        );
        when(
          mockDataSource.markReviewAsHelpful(reviewId),
        ).thenThrow(dioException);

        // Act
        final result = await repository.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, isA<TimeoutException>());
      });
    });
  });
}
