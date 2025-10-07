import 'package:equatable/equatable.dart';
import '../../../domain/entities/review.dart';

abstract class ReviewEvent extends Equatable {
  const ReviewEvent();

  @override
  List<Object?> get props => [];
}

class SubmitReview extends ReviewEvent {
  final SubmitReviewRequest request;

  const SubmitReview(this.request);

  @override
  List<Object?> get props => [request];
}

class LoadMerchantReviews extends ReviewEvent {
  final String merchantId;
  final int page;

  const LoadMerchantReviews(this.merchantId, {this.page = 1});

  @override
  List<Object?> get props => [merchantId, page];
}

class MarkReviewHelpful extends ReviewEvent {
  final String reviewId;

  const MarkReviewHelpful(this.reviewId);

  @override
  List<Object?> get props => [reviewId];
}

