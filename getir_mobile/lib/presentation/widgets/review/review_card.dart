import 'package:flutter/material.dart';
import 'package:timeago/timeago.dart' as timeago;
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_typography.dart';
import '../../../domain/entities/review.dart';

/// Review Card Widget
/// Displays a single review with rating, comment, and helpful votes
class ReviewCard extends StatelessWidget {
  final Review review;
  final VoidCallback? onHelpfulTap;

  const ReviewCard({super.key, required this.review, this.onHelpfulTap});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header (User + Rating)
          Row(
            children: [
              // User avatar
              CircleAvatar(
                backgroundColor: AppColors.primaryLight.withOpacity(0.3),
                child: Text(
                  review.reviewerName.substring(0, 1).toUpperCase(),
                  style: const TextStyle(
                    color: AppColors.primary,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              const SizedBox(width: 12),
              // User name & time
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      review.reviewerName,
                      style: AppTypography.bodyMedium.copyWith(
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    Text(
                      timeago.format(review.createdAt, locale: 'tr'),
                      style: AppTypography.bodySmall.copyWith(
                        color: AppColors.textSecondary,
                      ),
                    ),
                  ],
                ),
              ),
              // Star rating
              _buildStarRating(review.rating),
            ],
          ),

          const SizedBox(height: 12),

          // Comment
          Text(review.comment, style: AppTypography.bodyMedium),

          const SizedBox(height: 12),

          // Footer (Helpful votes)
          Row(
            children: [
              InkWell(
                onTap: onHelpfulTap,
                borderRadius: BorderRadius.circular(20),
                child: Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    color: Colors.grey[100],
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(color: Colors.grey[300]!),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(
                        Icons.thumb_up_outlined,
                        size: 16,
                        color: Colors.grey[600],
                      ),
                      const SizedBox(width: 6),
                      Text(
                        'Faydalı (${review.helpfulCount})',
                        style: AppTypography.bodySmall.copyWith(
                          color: Colors.grey[700],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildStarRating(int rating) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: List.generate(5, (index) {
        return Icon(
          index < rating ? Icons.star : Icons.star_border,
          color: index < rating ? Colors.amber : Colors.grey[400],
          size: 16,
        );
      }),
    );
  }
}

/// Review List Widget
/// Displays a list of reviews with pagination
class ReviewList extends StatelessWidget {
  final List<Review> reviews;
  final bool isLoading;
  final VoidCallback? onLoadMore;
  final Function(String reviewId)? onHelpfulTap;

  const ReviewList({
    super.key,
    required this.reviews,
    this.isLoading = false,
    this.onLoadMore,
    this.onHelpfulTap,
  });

  @override
  Widget build(BuildContext context) {
    if (reviews.isEmpty && !isLoading) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.rate_review_outlined, size: 64, color: Colors.grey[400]),
            const SizedBox(height: 16),
            Text(
              'Henüz değerlendirme yok',
              style: AppTypography.bodyLarge.copyWith(
                color: AppColors.textSecondary,
              ),
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: reviews.length + (isLoading ? 1 : 0),
      itemBuilder: (context, index) {
        if (index >= reviews.length) {
          return const Center(
            child: Padding(
              padding: EdgeInsets.all(16.0),
              child: CircularProgressIndicator(),
            ),
          );
        }

        final review = reviews[index];
        return ReviewCard(
          review: review,
          onHelpfulTap: () => onHelpfulTap?.call(review.id),
        );
      },
    );
  }
}

/// Review Summary Widget
/// Displays average rating and distribution
class ReviewSummary extends StatelessWidget {
  final double averageRating;
  final int totalReviews;
  final Map<int, int>? ratingDistribution; // {5: 120, 4: 45, 3: 10, 2: 5, 1: 2}

  const ReviewSummary({
    super.key,
    required this.averageRating,
    required this.totalReviews,
    this.ratingDistribution,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            blurRadius: 4,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          // Average rating
          Column(
            children: [
              Text(
                averageRating.toStringAsFixed(1),
                style: const TextStyle(
                  fontSize: 48,
                  fontWeight: FontWeight.bold,
                  color: AppColors.primary,
                ),
              ),
              Row(
                children: List.generate(5, (index) {
                  return Icon(
                    index < averageRating.floor()
                        ? Icons.star
                        : Icons.star_border,
                    color: Colors.amber,
                    size: 16,
                  );
                }),
              ),
              const SizedBox(height: 4),
              Text(
                '$totalReviews değerlendirme',
                style: AppTypography.bodySmall.copyWith(
                  color: AppColors.textSecondary,
                ),
              ),
            ],
          ),

          const SizedBox(width: 24),

          // Rating distribution
          if (ratingDistribution != null)
            Expanded(
              child: Column(
                children: List.generate(5, (index) {
                  final star = 5 - index;
                  final count = ratingDistribution![star] ?? 0;
                  final percentage = totalReviews > 0
                      ? (count / totalReviews)
                      : 0.0;

                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 2),
                    child: Row(
                      children: [
                        Text('$star', style: AppTypography.bodySmall),
                        const SizedBox(width: 4),
                        Icon(Icons.star, size: 12, color: Colors.amber),
                        const SizedBox(width: 8),
                        Expanded(
                          child: LinearProgressIndicator(
                            value: percentage,
                            backgroundColor: Colors.grey[200],
                            valueColor: const AlwaysStoppedAnimation<Color>(
                              Colors.amber,
                            ),
                          ),
                        ),
                        const SizedBox(width: 8),
                        Text(
                          '$count',
                          style: AppTypography.bodySmall.copyWith(
                            color: AppColors.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  );
                }),
              ),
            ),
        ],
      ),
    );
  }
}
