import 'package:flutter_bloc/flutter_bloc.dart';
import '../../../domain/usecases/review_usecases.dart';
import 'review_event.dart';
import 'review_state.dart';

class ReviewBloc extends Bloc<ReviewEvent, ReviewState> {
  final SubmitReviewUseCase _submitReviewUseCase;
  final GetMerchantReviewsUseCase _getMerchantReviewsUseCase;
  final MarkReviewAsHelpfulUseCase _markReviewAsHelpfulUseCase;

  ReviewBloc({
    required SubmitReviewUseCase submitReviewUseCase,
    required GetMerchantReviewsUseCase getMerchantReviewsUseCase,
    required MarkReviewAsHelpfulUseCase markReviewAsHelpfulUseCase,
  }) : _submitReviewUseCase = submitReviewUseCase,
       _getMerchantReviewsUseCase = getMerchantReviewsUseCase,
       _markReviewAsHelpfulUseCase = markReviewAsHelpfulUseCase,
       super(ReviewInitial()) {
    on<SubmitReview>(_onSubmitReview);
    on<LoadMerchantReviews>(_onLoadMerchantReviews);
    on<MarkReviewHelpful>(_onMarkReviewHelpful);
  }

  Future<void> _onSubmitReview(
    SubmitReview event,
    Emitter<ReviewState> emit,
  ) async {
    emit(ReviewLoading());
    try {
      final review = await _submitReviewUseCase(event.request);
      emit(ReviewSubmitted(review));
    } catch (e) {
      emit(ReviewError(e.toString()));
    }
  }

  Future<void> _onLoadMerchantReviews(
    LoadMerchantReviews event,
    Emitter<ReviewState> emit,
  ) async {
    emit(ReviewLoading());
    try {
      final reviews = await _getMerchantReviewsUseCase(
        event.merchantId,
        page: event.page,
      );
      emit(ReviewsLoaded(reviews, hasMore: reviews.length >= 20));
    } catch (e) {
      emit(ReviewError(e.toString()));
    }
  }

  Future<void> _onMarkReviewHelpful(
    MarkReviewHelpful event,
    Emitter<ReviewState> emit,
  ) async {
    try {
      await _markReviewAsHelpfulUseCase(event.reviewId);
      emit(ReviewMarkedHelpful(event.reviewId));
    } catch (e) {
      emit(ReviewError(e.toString()));
    }
  }
}
