import '../../core/errors/result.dart';
import '../entities/review.dart';
import '../repositories/review_repository.dart';

/// Review Service
///
/// Centralized service for review operations.
/// Replaces 3 separate UseCase classes.
class ReviewService {
  final ReviewRepository _repository;

  const ReviewService(this._repository);

  Future<Result<Review>> submitReview(SubmitReviewRequest request) async {
    return await _repository.submitReview(request);
  }

  Future<Result<List<Review>>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    return await _repository.getMerchantReviews(
      merchantId,
      page: page,
      pageSize: pageSize,
    );
  }

  Future<Result<void>> markReviewAsHelpful(String reviewId) async {
    return await _repository.markReviewAsHelpful(reviewId);
  }
}
