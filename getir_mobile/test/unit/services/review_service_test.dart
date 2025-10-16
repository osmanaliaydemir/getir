import 'package:flutter_test/flutter_test.dart';
import 'package:mockito/annotations.dart';
import 'package:mockito/mockito.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/entities/review.dart';
import 'package:getir_mobile/domain/repositories/review_repository.dart';
import 'package:getir_mobile/domain/services/review_service.dart';

import 'review_service_test.mocks.dart';

@GenerateMocks([IReviewRepository])
void main() {
  late ReviewService service;
  late MockIReviewRepository mockRepository;

  setUp(() {
    mockRepository = MockIReviewRepository();
    service = ReviewService(mockRepository);
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
    isApproved: true,
    helpfulCount: 10,
  );

  const submitRequest = SubmitReviewRequest(
    revieweeId: 'merchant-123',
    revieweeType: 'Merchant',
    orderId: 'order-123',
    rating: 5,
    comment: 'Great service!',
  );

  group('ReviewService -', () {
    group('submitReview', () {
      test('submits review when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.submitReview(submitRequest),
        ).thenAnswer((_) async => Result.success(testReview));

        // Act
        final result = await service.submitReview(submitRequest);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, testReview);
        verify(mockRepository.submitReview(submitRequest)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = ValidationException(message: 'Invalid review data');
        when(
          mockRepository.submitReview(submitRequest),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.submitReview(submitRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.submitReview(submitRequest)).called(1);
      });

      test('returns failure when order not found', () async {
        // Arrange
        final exception = NotFoundException(message: 'Order not found');
        when(
          mockRepository.submitReview(submitRequest),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.submitReview(submitRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns failure when already reviewed', () async {
        // Arrange
        final exception = ValidationException(
          message: 'Order already reviewed',
        );
        when(
          mockRepository.submitReview(submitRequest),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.submitReview(submitRequest);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });

    group('getMerchantReviews', () {
      const merchantId = 'merchant-123';

      test('returns reviews when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => Result.success([testReview]));

        // Act
        final result = await service.getMerchantReviews(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, [testReview]);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NetworkException(message: 'Network error');
        when(
          mockRepository.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.getMerchantReviews(merchantId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });

      test('returns empty list when merchant has no reviews', () async {
        // Arrange
        when(
          mockRepository.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => Result.success([]));

        // Act
        final result = await service.getMerchantReviews(merchantId);

        // Assert
        expect(result.isSuccess, true);
        expect(result.data, []);
      });

      test('supports pagination', () async {
        // Arrange
        when(
          mockRepository.getMerchantReviews(
            merchantId,
            page: anyNamed('page'),
            pageSize: anyNamed('pageSize'),
          ),
        ).thenAnswer((_) async => Result.success([testReview]));

        // Act
        final result = await service.getMerchantReviews(
          merchantId,
          page: 2,
          pageSize: 10,
        );

        // Assert
        expect(result.isSuccess, true);
        verify(
          mockRepository.getMerchantReviews(merchantId, page: 2, pageSize: 10),
        ).called(1);
      });
    });

    group('markReviewAsHelpful', () {
      const reviewId = 'review-123';

      test('marks review as helpful when repository call succeeds', () async {
        // Arrange
        when(
          mockRepository.markReviewAsHelpful(reviewId),
        ).thenAnswer((_) async => Result.success(null));

        // Act
        final result = await service.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isSuccess, true);
        verify(mockRepository.markReviewAsHelpful(reviewId)).called(1);
      });

      test('returns failure when repository call fails', () async {
        // Arrange
        final exception = NotFoundException(message: 'Review not found');
        when(
          mockRepository.markReviewAsHelpful(reviewId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
        verify(mockRepository.markReviewAsHelpful(reviewId)).called(1);
      });

      test('returns failure when already marked', () async {
        // Arrange
        final exception = ValidationException(
          message: 'Already marked as helpful',
        );
        when(
          mockRepository.markReviewAsHelpful(reviewId),
        ).thenAnswer((_) async => Result.failure(exception));

        // Act
        final result = await service.markReviewAsHelpful(reviewId);

        // Assert
        expect(result.isFailure, true);
        expect(result.exception, exception);
      });
    });
  });
}
