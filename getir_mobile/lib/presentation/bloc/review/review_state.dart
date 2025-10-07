import 'package:equatable/equatable.dart';
import '../../../domain/entities/review.dart';

abstract class ReviewState extends Equatable {
  const ReviewState();

  @override
  List<Object?> get props => [];
}

class ReviewInitial extends ReviewState {}

class ReviewLoading extends ReviewState {}

class ReviewSubmitted extends ReviewState {
  final Review review;

  const ReviewSubmitted(this.review);

  @override
  List<Object?> get props => [review];
}

class ReviewsLoaded extends ReviewState {
  final List<Review> reviews;
  final bool hasMore;

  const ReviewsLoaded(this.reviews, {this.hasMore = false});

  @override
  List<Object?> get props => [reviews, hasMore];
}

class ReviewMarkedHelpful extends ReviewState {
  final String reviewId;

  const ReviewMarkedHelpful(this.reviewId);

  @override
  List<Object?> get props => [reviewId];
}

class ReviewError extends ReviewState {
  final String message;

  const ReviewError(this.message);

  @override
  List<Object?> get props => [message];
}
