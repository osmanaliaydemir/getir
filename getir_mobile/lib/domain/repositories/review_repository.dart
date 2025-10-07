import '../entities/review.dart';

abstract class ReviewRepository {
  Future<Review> submitReview(SubmitReviewRequest request);
  Future<List<Review>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  });
  Future<List<Review>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  });
  Future<Review> getReviewById(String reviewId);
  Future<void> markReviewAsHelpful(String reviewId);
  Future<Review> updateReview(String reviewId, {int? rating, String? comment});
  Future<void> deleteReview(String reviewId);
}
