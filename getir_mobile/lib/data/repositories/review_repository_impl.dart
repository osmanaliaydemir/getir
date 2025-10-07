import '../../domain/entities/review.dart';
import '../../domain/repositories/review_repository.dart';
import '../datasources/review_datasource.dart';

class ReviewRepositoryImpl implements ReviewRepository {
  final ReviewDataSource _dataSource;

  ReviewRepositoryImpl(this._dataSource);

  @override
  Future<Review> submitReview(SubmitReviewRequest request) async {
    return await _dataSource.submitReview(request);
  }

  @override
  Future<List<Review>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    return await _dataSource.getMerchantReviews(
      merchantId,
      page: page,
      pageSize: pageSize,
    );
  }

  @override
  Future<List<Review>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    return await _dataSource.getCourierReviews(
      courierId,
      page: page,
      pageSize: pageSize,
    );
  }

  @override
  Future<Review> getReviewById(String reviewId) async {
    return await _dataSource.getReviewById(reviewId);
  }

  @override
  Future<void> markReviewAsHelpful(String reviewId) async {
    return await _dataSource.markReviewAsHelpful(reviewId);
  }

  @override
  Future<Review> updateReview(
    String reviewId, {
    int? rating,
    String? comment,
  }) async {
    return await _dataSource.updateReview(
      reviewId,
      rating: rating,
      comment: comment,
    );
  }

  @override
  Future<void> deleteReview(String reviewId) async {
    return await _dataSource.deleteReview(reviewId);
  }
}
