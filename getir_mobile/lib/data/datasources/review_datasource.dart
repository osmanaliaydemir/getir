import 'package:dio/dio.dart';
import '../../domain/entities/review.dart';

abstract class ReviewDataSource {
  /// Submit a review for merchant or courier
  Future<Review> submitReview(SubmitReviewRequest request);

  /// Get reviews for a merchant
  Future<List<Review>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  });

  /// Get reviews for a courier
  Future<List<Review>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  });

  /// Get review by ID
  Future<Review> getReviewById(String reviewId);

  /// Mark review as helpful
  Future<void> markReviewAsHelpful(String reviewId);

  /// Update review
  Future<Review> updateReview(String reviewId, {int? rating, String? comment});

  /// Delete review
  Future<void> deleteReview(String reviewId);
}

class ReviewDataSourceImpl implements ReviewDataSource {
  final Dio _dio;

  ReviewDataSourceImpl({required Dio dio}) : _dio = dio;

  @override
  Future<Review> submitReview(SubmitReviewRequest request) async {
    try {
      final response = await _dio.post(
        '/api/v1/reviews',
        data: request.toJson(),
      );

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = response.data['data'] ?? response.data;
        return _reviewFromJson(data);
      } else {
        throw Exception('Failed to submit review: ${response.statusCode}');
      }
    } on DioException catch (e) {
      if (e.response?.statusCode == 400) {
        throw Exception('Geçersiz değerlendirme verisi');
      } else if (e.response?.statusCode == 409) {
        throw Exception('Bu sipariş için zaten değerlendirme yaptınız');
      }
      throw Exception('Değerlendirme gönderilemedi: ${e.message}');
    } catch (e) {
      throw Exception('Değerlendirme gönderilemedi: $e');
    }
  }

  @override
  Future<List<Review>> getMerchantReviews(
    String merchantId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/reviews/merchant/$merchantId',
        queryParameters: {'pageNumber': page, 'pageSize': pageSize},
      );

      final data = response.data['data'];
      if (data == null) return [];

      final List<dynamic> items = data['items'] ?? data;
      return items.map((json) => _reviewFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch merchant reviews: $e');
    }
  }

  @override
  Future<List<Review>> getCourierReviews(
    String courierId, {
    int page = 1,
    int pageSize = 20,
  }) async {
    try {
      final response = await _dio.get(
        '/api/v1/reviews/courier/$courierId',
        queryParameters: {'pageNumber': page, 'pageSize': pageSize},
      );

      final data = response.data['data'];
      if (data == null) return [];

      final List<dynamic> items = data['items'] ?? data;
      return items.map((json) => _reviewFromJson(json)).toList();
    } catch (e) {
      throw Exception('Failed to fetch courier reviews: $e');
    }
  }

  @override
  Future<Review> getReviewById(String reviewId) async {
    try {
      final response = await _dio.get('/api/v1/reviews/$reviewId');
      final data = response.data['data'] ?? response.data;
      return _reviewFromJson(data);
    } catch (e) {
      throw Exception('Failed to fetch review: $e');
    }
  }

  @override
  Future<void> markReviewAsHelpful(String reviewId) async {
    try {
      await _dio.post('/api/v1/reviews/$reviewId/helpful');
    } catch (e) {
      throw Exception('Failed to mark review as helpful: $e');
    }
  }

  @override
  Future<Review> updateReview(
    String reviewId, {
    int? rating,
    String? comment,
  }) async {
    try {
      final response = await _dio.put(
        '/api/v1/reviews/$reviewId',
        data: {
          if (rating != null) 'rating': rating,
          if (comment != null) 'comment': comment,
        },
      );

      final data = response.data['data'] ?? response.data;
      return _reviewFromJson(data);
    } catch (e) {
      throw Exception('Failed to update review: $e');
    }
  }

  @override
  Future<void> deleteReview(String reviewId) async {
    try {
      await _dio.delete('/api/v1/reviews/$reviewId');
    } catch (e) {
      throw Exception('Failed to delete review: $e');
    }
  }

  Review _reviewFromJson(Map<String, dynamic> json) {
    return Review(
      id: json['id']?.toString() ?? '',
      reviewerId: json['reviewerId']?.toString() ?? '',
      reviewerName: json['reviewerName']?.toString() ?? 'Anonim',
      revieweeId: json['revieweeId']?.toString() ?? '',
      revieweeType: json['revieweeType']?.toString() ?? 'Merchant',
      orderId: json['orderId']?.toString() ?? '',
      rating: json['rating'] ?? 0,
      comment: json['comment'] ?? '',
      createdAt: json['createdAt'] != null
          ? DateTime.parse(json['createdAt'])
          : DateTime.now(),
      updatedAt: json['updatedAt'] != null
          ? DateTime.parse(json['updatedAt'])
          : null,
      isApproved: json['isApproved'] ?? true,
      helpfulCount: json['helpfulCount'] ?? 0,
    );
  }
}
