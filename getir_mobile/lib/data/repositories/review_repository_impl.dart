import 'package:dio/dio.dart';
import '../../core/errors/app_exceptions.dart';
import '../../core/errors/result.dart';
import '../../domain/entities/review.dart';
import '../../domain/repositories/review_repository.dart';
import '../datasources/review_datasource.dart';

class ReviewRepositoryImpl implements IReviewRepository {
  final ReviewDataSource _dataSource;

  ReviewRepositoryImpl(this._dataSource);

  @override
  Future<Result<Review>> submitReview(SubmitReviewRequest request) async {
    try {
      final review = await _dataSource.submitReview(request);
      return Result.success(review);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to submit review: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<List<Review>>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final reviews = await _dataSource.getMerchantReviews(
        merchantId,
        page: page,
        pageSize: pageSize,
      );
      return Result.success(reviews);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to get merchant reviews: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<List<Review>>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final reviews = await _dataSource.getCourierReviews(
        courierId,
        page: page,
        pageSize: pageSize,
      );
      return Result.success(reviews);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get courier reviews: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<Review>> getReviewById(String reviewId) async {
    try {
      final review = await _dataSource.getReviewById(reviewId);
      return Result.success(review);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to get review: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> markReviewAsHelpful(String reviewId) async {
    try {
      await _dataSource.markReviewAsHelpful(reviewId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(
          message: 'Failed to mark review as helpful: ${e.toString()}',
        ),
      );
    }
  }

  @override
  Future<Result<Review>> updateReview(
    String reviewId, {
    int? rating,
    String? comment,
  }) async {
    try {
      final review = await _dataSource.updateReview(
        reviewId,
        rating: rating,
        comment: comment,
      );
      return Result.success(review);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to update review: ${e.toString()}'),
      );
    }
  }

  @override
  Future<Result<void>> deleteReview(String reviewId) async {
    try {
      await _dataSource.deleteReview(reviewId);
      return Result.success(null);
    } on DioException catch (e) {
      return Result.failure(ExceptionFactory.fromDioError(e));
    } on AppException catch (e) {
      return Result.failure(e);
    } catch (e) {
      return Result.failure(
        ApiException(message: 'Failed to delete review: ${e.toString()}'),
      );
    }
  }
}
