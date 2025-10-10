import 'package:flutter_test/flutter_test.dart';
import 'package:bloc_test/bloc_test.dart';
import 'package:mockito/mockito.dart';
import 'package:mockito/annotations.dart';

import 'package:getir_mobile/core/errors/app_exceptions.dart';
import 'package:getir_mobile/core/errors/result.dart';
import 'package:getir_mobile/domain/entities/review.dart';
import 'package:getir_mobile/domain/services/review_service.dart';
import 'package:getir_mobile/presentation/bloc/review/review_bloc.dart';
import 'package:getir_mobile/presentation/bloc/review/review_event.dart';
import 'package:getir_mobile/presentation/bloc/review/review_state.dart';

import 'review_bloc_test.mocks.dart';

@GenerateMocks([ReviewService])
void main() {
  group('ReviewBloc', () {
    late ReviewBloc bloc;
    late MockReviewService mockReviewService;

    setUp(() {
      mockReviewService = MockReviewService();
      bloc = ReviewBloc(mockReviewService);
    });

    tearDown(() {
      bloc.close();
    });

    // Helper functions for creating test data
    Review createMockReview({
      String? id,
      int? rating,
      String? comment,
      bool? isApproved,
      int? helpfulCount,
    }) {
      return Review(
        id: id ?? 'review-123',
        reviewerId: 'user-123',
        reviewerName: 'Test User',
        revieweeId: 'merchant-456',
        revieweeType: 'Merchant',
        orderId: 'order-789',
        rating: rating ?? 5,
        comment: comment ?? 'Great service!',
        createdAt: DateTime(2025, 1, 1),
        isApproved: isApproved ?? true,
        helpfulCount: helpfulCount ?? 0,
      );
    }

    SubmitReviewRequest createMockRequest({int? rating, String? comment}) {
      return SubmitReviewRequest(
        revieweeId: 'merchant-456',
        revieweeType: 'Merchant',
        orderId: 'order-789',
        rating: rating ?? 5,
        comment: comment ?? 'Great service!',
      );
    }

    test('initial state should be ReviewInitial', () {
      expect(bloc.state, equals(ReviewInitial()));
    });

    // ==================== SubmitReview Tests ====================
    group('SubmitReview', () {
      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewSubmitted] when submit is successful',
        build: () {
          final review = createMockReview();
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(review));
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest())),
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>().having(
            (state) => state.review.id,
            'review id',
            'review-123',
          ),
        ],
        verify: (_) {
          verify(mockReviewService.submitReview(any)).called(1);
        },
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] when submit fails',
        build: () {
          when(mockReviewService.submitReview(any)).thenAnswer(
            (_) async => Result.failure(
              const ApiException(message: 'Failed to submit review'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest())),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Failed to submit review',
          ),
        ],
        verify: (_) {
          verify(mockReviewService.submitReview(any)).called(1);
        },
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] on network error',
        build: () {
          when(mockReviewService.submitReview(any)).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'No internet connection'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest())),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'No internet connection',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] on validation error',
        build: () {
          when(mockReviewService.submitReview(any)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(message: 'Rating must be between 1-5'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest())),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Rating must be between 1-5',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should submit review with minimum rating (1 star)',
        build: () {
          final review = createMockReview(rating: 1);
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(review));
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest(rating: 1))),
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>().having(
            (state) => state.review.rating,
            'rating',
            1,
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should submit review with maximum rating (5 stars)',
        build: () {
          final review = createMockReview(rating: 5);
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(review));
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest(rating: 5))),
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>().having(
            (state) => state.review.rating,
            'rating',
            5,
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should submit review with empty comment',
        build: () {
          final review = createMockReview(comment: '');
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(review));
          return bloc;
        },
        act: (bloc) => bloc.add(SubmitReview(createMockRequest(comment: ''))),
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>().having(
            (state) => state.review.comment,
            'comment',
            '',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should submit review with long comment (500 chars)',
        build: () {
          final longComment = 'a' * 500;
          final review = createMockReview(comment: longComment);
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(review));
          return bloc;
        },
        act: (bloc) =>
            bloc.add(SubmitReview(createMockRequest(comment: 'a' * 500))),
        expect: () => [ReviewLoading(), isA<ReviewSubmitted>()],
      );
    });

    // ==================== LoadMerchantReviews Tests ====================
    group('LoadMerchantReviews', () {
      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewsLoaded] when loading is successful',
        build: () {
          final reviews = [
            createMockReview(id: 'review-1'),
            createMockReview(id: 'review-2'),
            createMockReview(id: 'review-3'),
          ];
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => Result.success(reviews));
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('merchant-456')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewsLoaded>()
              .having((state) => state.reviews.length, 'reviews count', 3)
              .having((state) => state.hasMore, 'hasMore', false),
        ],
        verify: (_) {
          verify(
            mockReviewService.getMerchantReviews(
              'merchant-456',
              page: 1,
              pageSize: 20,
            ),
          ).called(1);
        },
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewsLoaded] with hasMore=true when 20+ reviews',
        build: () {
          final reviews = List.generate(
            20,
            (i) => createMockReview(id: 'review-$i'),
          );
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => Result.success(reviews));
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('merchant-456')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewsLoaded>()
              .having((state) => state.reviews.length, 'reviews count', 20)
              .having((state) => state.hasMore, 'hasMore', true),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewsLoaded] with empty list when no reviews',
        build: () {
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => Result.success([]));
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('merchant-456')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewsLoaded>()
              .having((state) => state.reviews.isEmpty, 'is empty', true)
              .having((state) => state.hasMore, 'hasMore', false),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should load reviews for specific page (page 2)',
        build: () {
          final reviews = [createMockReview(id: 'review-page2')];
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => Result.success(reviews));
          return bloc;
        },
        act: (bloc) =>
            bloc.add(const LoadMerchantReviews('merchant-456', page: 2)),
        expect: () => [ReviewLoading(), isA<ReviewsLoaded>()],
        verify: (_) {
          verify(
            mockReviewService.getMerchantReviews(
              'merchant-456',
              page: 2,
              pageSize: 20,
            ),
          ).called(1);
        },
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] when loading fails',
        build: () {
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const ApiException(message: 'Failed to load reviews'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('merchant-456')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Failed to load reviews',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] on network error',
        build: () {
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'No internet connection'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('merchant-456')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'No internet connection',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewLoading, ReviewError] when merchant not found',
        build: () {
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(message: 'Merchant not found'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const LoadMerchantReviews('invalid-id')),
        expect: () => [
          ReviewLoading(),
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Merchant not found',
          ),
        ],
      );
    });

    // ==================== MarkReviewHelpful Tests ====================
    group('MarkReviewHelpful', () {
      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewMarkedHelpful] when marking is successful',
        build: () {
          when(
            mockReviewService.markReviewAsHelpful(any),
          ).thenAnswer((_) async => Result.success(null));
          return bloc;
        },
        act: (bloc) => bloc.add(const MarkReviewHelpful('review-123')),
        expect: () => [
          isA<ReviewMarkedHelpful>().having(
            (state) => state.reviewId,
            'reviewId',
            'review-123',
          ),
        ],
        verify: (_) {
          verify(mockReviewService.markReviewAsHelpful('review-123')).called(1);
        },
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewError] when marking fails',
        build: () {
          when(mockReviewService.markReviewAsHelpful(any)).thenAnswer(
            (_) async => Result.failure(
              const ApiException(message: 'Failed to mark review as helpful'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const MarkReviewHelpful('review-123')),
        expect: () => [
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Failed to mark review as helpful',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewError] when review not found',
        build: () {
          when(mockReviewService.markReviewAsHelpful(any)).thenAnswer(
            (_) async => Result.failure(
              const NotFoundException(message: 'Review not found'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const MarkReviewHelpful('invalid-review-id')),
        expect: () => [
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Review not found',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewError] when marking own review',
        build: () {
          when(mockReviewService.markReviewAsHelpful(any)).thenAnswer(
            (_) async => Result.failure(
              const ValidationException(
                message: 'Cannot mark your own review as helpful',
              ),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const MarkReviewHelpful('own-review-123')),
        expect: () => [
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'Cannot mark your own review as helpful',
          ),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should emit [ReviewError] on network error',
        build: () {
          when(mockReviewService.markReviewAsHelpful(any)).thenAnswer(
            (_) async => Result.failure(
              const NetworkException(message: 'No internet connection'),
            ),
          );
          return bloc;
        },
        act: (bloc) => bloc.add(const MarkReviewHelpful('review-123')),
        expect: () => [
          isA<ReviewError>().having(
            (state) => state.message,
            'error message',
            'No internet connection',
          ),
        ],
      );
    });

    // ==================== Edge Cases & Error Handling ====================
    group('Edge Cases', () {
      test('should handle unknown exception gracefully', () async {
        when(
          mockReviewService.submitReview(any),
        ).thenAnswer((_) async => Result.failure(Exception('Unknown error')));

        bloc.add(SubmitReview(createMockRequest()));

        await expectLater(
          bloc.stream,
          emitsInOrder([
            ReviewLoading(),
            isA<ReviewError>().having(
              (state) => state.message,
              'error message',
              'An unexpected error occurred',
            ),
          ]),
        );
      });

      blocTest<ReviewBloc, ReviewState>(
        'should handle rapid consecutive events',
        build: () {
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(createMockReview()));
          return bloc;
        },
        act: (bloc) {
          bloc.add(SubmitReview(createMockRequest()));
          bloc.add(SubmitReview(createMockRequest()));
          bloc.add(SubmitReview(createMockRequest()));
        },
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>(),
          ReviewLoading(),
          isA<ReviewSubmitted>(),
          ReviewLoading(),
          isA<ReviewSubmitted>(),
        ],
      );

      blocTest<ReviewBloc, ReviewState>(
        'should handle mixed event types',
        build: () {
          when(
            mockReviewService.submitReview(any),
          ).thenAnswer((_) async => Result.success(createMockReview()));
          when(
            mockReviewService.getMerchantReviews(
              any,
              page: anyNamed('page'),
              pageSize: anyNamed('pageSize'),
            ),
          ).thenAnswer((_) async => Result.success([createMockReview()]));
          when(
            mockReviewService.markReviewAsHelpful(any),
          ).thenAnswer((_) async => Result.success(null));
          return bloc;
        },
        act: (bloc) {
          bloc.add(SubmitReview(createMockRequest()));
          bloc.add(const LoadMerchantReviews('merchant-456'));
          bloc.add(const MarkReviewHelpful('review-123'));
        },
        expect: () => [
          ReviewLoading(),
          isA<ReviewSubmitted>(),
          ReviewLoading(),
          isA<ReviewsLoaded>(),
          isA<ReviewMarkedHelpful>(),
        ],
      );
    });
  });
}
