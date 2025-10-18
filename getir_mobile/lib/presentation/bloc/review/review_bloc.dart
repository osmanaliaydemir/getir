import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/services/review_service.dart';
import 'review_event.dart';
import 'review_state.dart';

class ReviewBloc extends Bloc<ReviewEvent, ReviewState> {
  final ReviewService _reviewService;

  ReviewBloc(this._reviewService) : super(ReviewInitial()) {
    on<SubmitReview>(_onSubmitReview);
    on<LoadMerchantReviews>(_onLoadMerchantReviews);
    on<MarkReviewHelpful>(_onMarkReviewHelpful);
  }

  Future<void> _onSubmitReview(
    SubmitReview event,
    Emitter<ReviewState> emit,
  ) async {
    emit(ReviewLoading());

    final result = await _reviewService.submitReview(event.request);

    result.when(
      success: (review) => emit(ReviewSubmitted(review)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ReviewError(message));
      },
    );
  }

  Future<void> _onLoadMerchantReviews(
    LoadMerchantReviews event,
    Emitter<ReviewState> emit,
  ) async {
    emit(ReviewLoading());

    final result = await _reviewService.getMerchantReviews(
      event.merchantId,
      page: event.page,
    );

    result.when(
      success: (reviews) =>
          emit(ReviewsLoaded(reviews, hasMore: reviews.length >= 20)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ReviewError(message));
      },
    );
  }

  Future<void> _onMarkReviewHelpful(
    MarkReviewHelpful event,
    Emitter<ReviewState> emit,
  ) async {
    final result = await _reviewService.markReviewAsHelpful(event.reviewId);

    result.when(
      success: (_) => emit(ReviewMarkedHelpful(event.reviewId)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(ReviewError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
