import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../entities/review.dart';
import '../repositories/review_repository.dart';

/// Submit Review Use Case
class SubmitReviewUseCase {
  final ReviewRepository _repository;

  SubmitReviewUseCase(this._repository);

  Future<Result<Review>> call(SubmitReviewRequest request) async {
    return await _repository.submitReview(request);
  }
}

/// Get Merchant Reviews Use Case
class GetMerchantReviewsUseCase {
  final ReviewRepository _repository;

  GetMerchantReviewsUseCase(this._repository);

  Future<Result<List<Review>>> call(String merchantId, {int page = 1}) async {
    if (merchantId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Merchant ID cannot be empty',
          code: 'EMPTY_MERCHANT_ID',
        ),
      );
    }

    return await _repository.getMerchantReviews(merchantId, page: page);
  }
}

/// Mark Review As Helpful Use Case
class MarkReviewAsHelpfulUseCase {
  final ReviewRepository _repository;

  MarkReviewAsHelpfulUseCase(this._repository);

  Future<Result<void>> call(String reviewId) async {
    if (reviewId.isEmpty) {
      return Result.failure(
        const ValidationException(
          message: 'Review ID cannot be empty',
          code: 'EMPTY_REVIEW_ID',
        ),
      );
    }

    return await _repository.markReviewAsHelpful(reviewId);
  }
}
