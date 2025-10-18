import '../../core/errors/result.dart';
import '../entities/review.dart';

abstract class IReviewRepository {
  Future<Result<Review>> submitReview(SubmitReviewRequest request);
  Future<Result<List<Review>>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  });
  Future<Result<List<Review>>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  });
  Future<Result<Review>> getReviewById(String reviewId);
  Future<Result<void>> markReviewAsHelpful(String reviewId);
  Future<Result<Review>> updateReview(
    String reviewId, {
    int? rating,
    String? comment,
  });
  Future<Result<void>> deleteReview(String reviewId);
}
