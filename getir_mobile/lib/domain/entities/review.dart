import 'package:equatable/equatable.dart';

/// Review entity for merchants and couriers
class Review extends Equatable {
  final String id;
  final String reviewerId;
  final String reviewerName;
  final String revieweeId; // Merchant or Courier ID
  final String revieweeType; // "Merchant" or "Courier"
  final String orderId;
  final int rating; // 1-5 stars
  final String comment;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final bool isApproved;
  final int helpfulCount; // Number of helpful votes

  const Review({
    required this.id,
    required this.reviewerId,
    required this.reviewerName,
    required this.revieweeId,
    required this.revieweeType,
    required this.orderId,
    required this.rating,
    required this.comment,
    required this.createdAt,
    this.updatedAt,
    this.isApproved = true,
    this.helpfulCount = 0,
  });

  @override
  List<Object?> get props => [
    id,
    reviewerId,
    reviewerName,
    revieweeId,
    revieweeType,
    orderId,
    rating,
    comment,
    createdAt,
    updatedAt,
    isApproved,
    helpfulCount,
  ];

  Review copyWith({
    String? id,
    String? reviewerId,
    String? reviewerName,
    String? revieweeId,
    String? revieweeType,
    String? orderId,
    int? rating,
    String? comment,
    DateTime? createdAt,
    DateTime? updatedAt,
    bool? isApproved,
    int? helpfulCount,
  }) {
    return Review(
      id: id ?? this.id,
      reviewerId: reviewerId ?? this.reviewerId,
      reviewerName: reviewerName ?? this.reviewerName,
      revieweeId: revieweeId ?? this.revieweeId,
      revieweeType: revieweeType ?? this.revieweeType,
      orderId: orderId ?? this.orderId,
      rating: rating ?? this.rating,
      comment: comment ?? this.comment,
      createdAt: createdAt ?? this.createdAt,
      updatedAt: updatedAt ?? this.updatedAt,
      isApproved: isApproved ?? this.isApproved,
      helpfulCount: helpfulCount ?? this.helpfulCount,
    );
  }
}

/// Review submission model
class SubmitReviewRequest {
  final String revieweeId;
  final String revieweeType;
  final String orderId;
  final int rating;
  final String comment;

  const SubmitReviewRequest({
    required this.revieweeId,
    required this.revieweeType,
    required this.orderId,
    required this.rating,
    required this.comment,
  });

  Map<String, dynamic> toJson() {
    return {
      'revieweeId': revieweeId,
      'revieweeType': revieweeType,
      'orderId': orderId,
      'rating': rating,
      'comment': comment,
    };
  }
}
