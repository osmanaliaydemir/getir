import '../entities/review.dart';
import '../repositories/review_repository.dart';

/// Submit a review for merchant or courier
class SubmitReviewUseCase {
  final ReviewRepository _repository;

  SubmitReviewUseCase(this._repository);

  Future<Review> call(SubmitReviewRequest request) async {
    return await _repository.submitReview(request);
  }
}

/// Get merchant reviews
class GetMerchantReviewsUseCase {
  final ReviewRepository _repository;

  GetMerchantReviewsUseCase(this._repository);

  Future<List<Review>> call(String merchantId, {int page = 1}) async {
    return await _repository.getMerchantReviews(merchantId, page: page);
  }
}

/// Mark review as helpful
class MarkReviewAsHelpfulUseCase {
  final ReviewRepository _repository;

  MarkReviewAsHelpfulUseCase(this._repository);

  Future<void> call(String reviewId) async {
    return await _repository.markReviewAsHelpful(reviewId);
  }
}

