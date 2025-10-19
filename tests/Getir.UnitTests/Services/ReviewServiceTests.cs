using FluentAssertions;
using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.Reviews;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Getir.UnitTests.Services;

/// <summary>
/// Comprehensive unit tests for ReviewService
/// Tests critical review operations: Create, Update, Delete, Rating, Validation, Moderation
/// </summary>
public class ReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ReviewService>> _loggerMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISignalRService> _signalRServiceMock;
    private readonly Mock<IBackgroundTaskService> _backgroundTaskServiceMock;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ReviewService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _signalRServiceMock = new Mock<ISignalRService>();
        _backgroundTaskServiceMock = new Mock<IBackgroundTaskService>();

        _reviewService = new ReviewService(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _loggingServiceMock.Object,
            _cacheServiceMock.Object,
            _backgroundTaskServiceMock.Object,
            _signalRServiceMock.Object);
    }

    #region CreateReviewAsync Tests

    [Fact]
    public async Task CreateReviewAsync_ValidReview_ShouldSucceed()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        var order = new Order
        {
            Id = orderId,
            UserId = reviewerId,
            MerchantId = merchantId,
            Status = OrderStatus.Delivered
        };

        var request = new CreateReviewRequest(
            merchantId,
            "Merchant",
            orderId,
            5,
            "Great service!",
            new List<string> { "Fast Delivery", "Good Quality" });

        SetupOrderMock(order);
        SetupCanReviewMock(true);
        SetupHasReviewedMock(false);
        SetupReviewRepositories();
        SetupRatingRepository();
        SetupReviewsListMock(new List<Review>()); // For UpdateRatingAsync

        // Act
        var result = await _reviewService.CreateReviewAsync(request, reviewerId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Great service!");
        
        _unitOfWorkMock.Verify(u => u.Repository<Review>()
            .AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateReviewAsync_OrderNotDelivered_ShouldFail()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            "Merchant",
            Guid.NewGuid(),
            5,
            "Great!",
            null);

        SetupCanReviewMock(true);
        SetupHasReviewedMock(false);
        SetupOrderMock(null); // Order not found or not delivered

        // Act
        var result = await _reviewService.CreateReviewAsync(request, reviewerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }

    [Fact]
    public async Task CreateReviewAsync_AlreadyReviewed_ShouldFail()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            "Merchant",
            Guid.NewGuid(),
            5,
            "Great!",
            null);

        SetupCanReviewMock(true);
        SetupHasReviewedMock(true); // Already reviewed

        // Act
        var result = await _reviewService.CreateReviewAsync(request, reviewerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_REVIEWED");
    }

    [Fact]
    public async Task CreateReviewAsync_CannotReview_ShouldFail()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var request = new CreateReviewRequest(
            Guid.NewGuid(),
            "Merchant",
            Guid.NewGuid(),
            5,
            "Great!",
            null);

        SetupCanReviewMock(false); // Cannot review

        // Act
        var result = await _reviewService.CreateReviewAsync(request, reviewerId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CANNOT_REVIEW");
    }

    #endregion

    #region UpdateReviewAsync Tests

    [Fact]
    public async Task UpdateReviewAsync_ValidUpdate_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();
        
        var review = new Review
        {
            Id = reviewId,
            ReviewerId = userId,
            RevieweeId = revieweeId,
            RevieweeType = "Merchant",
            Rating = 3,
            Comment = "Old comment"
        };

        var request = new UpdateReviewRequest(
            4,
            "Updated comment",
            new List<string> { "Good Service" });

        SetupReviewMock(review);
        SetupReviewTagsListMock(new List<ReviewTag>());
        SetupReviewRepositories();
        SetupRatingRepository();
        SetupReviewGetMock(review);
        SetupReviewsListMock(new List<Review> { review }); // For UpdateRatingAsync

        // Act
        var result = await _reviewService.UpdateReviewAsync(reviewId, request, userId);

        // Assert
        result.Success.Should().BeTrue();
        review.Rating.Should().Be(4);
        review.Comment.Should().Be("Updated comment");
    }

    [Fact]
    public async Task UpdateReviewAsync_NotOwner_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();
        
        var request = new UpdateReviewRequest(4, "Updated", null);

        SetupReviewMock(null); // Not found (wrong user ID)

        // Act
        var result = await _reviewService.UpdateReviewAsync(reviewId, request, wrongUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    #endregion

    #region DeleteReviewAsync Tests

    [Fact]
    public async Task DeleteReviewAsync_ValidReview_ShouldSoftDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        
        var review = new Review
        {
            Id = reviewId,
            ReviewerId = userId,
            RevieweeId = Guid.NewGuid(),
            RevieweeType = "Merchant",
            IsDeleted = false
        };

        SetupReviewMock(review);
        SetupRatingRepository();
        SetupReviewsListMock(new List<Review>()); // For UpdateRatingAsync (after delete, list is empty)

        // Act
        var result = await _reviewService.DeleteReviewAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeTrue();
        review.IsDeleted.Should().BeTrue();
        review.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteReviewAsync_NotOwner_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();

        SetupReviewMock(null);

        // Act
        var result = await _reviewService.DeleteReviewAsync(reviewId, wrongUserId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    #endregion

    #region GetReviewAsync Tests

    [Fact]
    public async Task GetReviewAsync_ValidReview_ShouldReturnReview()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var user = TestDataGenerator.CreateUser();
        
        var review = new Review
        {
            Id = reviewId,
            ReviewerId = user.Id,
            Reviewer = user,
            RevieweeId = Guid.NewGuid(),
            RevieweeType = "Merchant",
            Rating = 5,
            Comment = "Excellent!",
            IsApproved = true,
            IsDeleted = false,
            ReviewTags = new List<ReviewTag>
            {
                new() { Tag = "Fast" },
                new() { Tag = "Quality" }
            },
            ReviewHelpfuls = new List<ReviewHelpful>
            {
                new() { IsHelpful = true },
                new() { IsHelpful = true },
                new() { IsHelpful = false }
            }
        };

        SetupReviewGetMock(review);

        // Act
        var result = await _reviewService.GetReviewAsync(reviewId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(reviewId);
        result.Value.Rating.Should().Be(5);
        result.Value.Tags.Should().HaveCount(2);
        result.Value.HelpfulCount.Should().Be(2);
    }

    [Fact]
    public async Task GetReviewAsync_NotFound_ShouldReturnError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        SetupReviewGetMock(null);

        // Act
        var result = await _reviewService.GetReviewAsync(reviewId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    #endregion

    #region CanUserReviewAsync Tests

    [Fact]
    public async Task CanUserReviewAsync_DeliveredOrder_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            Status = OrderStatus.Delivered
        };

        SetupOrderMock(order);

        // Act
        var result = await _reviewService.CanUserReviewAsync(userId, Guid.NewGuid(), "Merchant", orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserReviewAsync_OrderNotDelivered_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        SetupOrderMock(null); // Order not found or not delivered

        // Act
        var result = await _reviewService.CanUserReviewAsync(userId, Guid.NewGuid(), "Merchant", orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region HasUserReviewedOrderAsync Tests

    [Fact]
    public async Task HasUserReviewedOrderAsync_HasReview_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        var review = new Review
        {
            ReviewerId = userId,
            OrderId = orderId,
            IsDeleted = false
        };

        SetupHasReviewedOrderMock(review);

        // Act
        var result = await _reviewService.HasUserReviewedOrderAsync(userId, orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasUserReviewedOrderAsync_NoReview_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        SetupHasReviewedOrderMock(null);

        // Act
        var result = await _reviewService.HasUserReviewedOrderAsync(userId, orderId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region ReportReviewAsync Tests

    [Fact]
    public async Task ReportReviewAsync_ValidReport_ShouldSucceed()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        
        var review = new Review { Id = reviewId };
        var request = new ReportReviewRequest("Spam", "This is spam content");

        SetupReviewGetMock(review);
        SetupReviewReportMock(null); // No existing report
        SetupReviewReportRepository();

        // Act
        var result = await _reviewService.ReportReviewAsync(reviewId, request, reporterId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<ReviewReport>()
            .AddAsync(It.IsAny<ReviewReport>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReportReviewAsync_AlreadyReported_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        
        var review = new Review { Id = reviewId };
        var existingReport = new ReviewReport
        {
            ReviewId = reviewId,
            ReporterId = reporterId
        };

        var request = new ReportReviewRequest("Spam", "Details");

        SetupReviewGetMock(review);
        SetupReviewReportMock(existingReport); // Already reported

        // Act
        var result = await _reviewService.ReportReviewAsync(reviewId, request, reporterId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_REPORTED");
    }

    [Fact]
    public async Task ReportReviewAsync_ReviewNotFound_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var request = new ReportReviewRequest("Spam", "Details");

        SetupReviewGetMock(null);

        // Act
        var result = await _reviewService.ReportReviewAsync(reviewId, request, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("REVIEW_NOT_FOUND");
    }

    #endregion

    #region VoteHelpfulAsync Tests

    [Fact]
    public async Task VoteHelpfulAsync_NewVote_ShouldSucceed()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review { Id = reviewId };
        var request = new ReviewHelpfulRequest(true);

        SetupReviewGetMock(review);
        SetupReviewHelpfulMock(null); // No existing vote
        SetupReviewHelpfulRepository();
        SetupHelpfulCountMocks(2, 1); // 2 helpful, 1 not helpful

        // Act
        var result = await _reviewService.VoteHelpfulAsync(reviewId, request, userId);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.IsHelpful.Should().BeTrue();
        result.Value.HelpfulCount.Should().Be(2);
        result.Value.NotHelpfulCount.Should().Be(1);
    }

    [Fact]
    public async Task VoteHelpfulAsync_AlreadyVoted_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review { Id = reviewId };
        var existingVote = new ReviewHelpful
        {
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = true
        };

        var request = new ReviewHelpfulRequest(false);

        SetupReviewGetMock(review);
        SetupReviewHelpfulMock(existingVote); // Already voted

        // Act
        var result = await _reviewService.VoteHelpfulAsync(reviewId, request, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_VOTED");
    }

    #endregion

    #region RemoveHelpfulVoteAsync Tests

    [Fact]
    public async Task RemoveHelpfulVoteAsync_ExistingVote_ShouldSucceed()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var vote = new ReviewHelpful
        {
            ReviewId = reviewId,
            UserId = userId
        };

        SetupReviewHelpfulMock(vote);
        SetupReviewHelpfulRepository();

        // Act
        var result = await _reviewService.RemoveHelpfulVoteAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<ReviewHelpful>().Delete(vote), Times.Once);
    }

    [Fact]
    public async Task RemoveHelpfulVoteAsync_NoVote_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupReviewHelpfulMock(null);

        // Act
        var result = await _reviewService.RemoveHelpfulVoteAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("VOTE_NOT_FOUND");
    }

    #endregion

    #region LikeReviewAsync Tests

    [Fact]
    public async Task LikeReviewAsync_NewLike_ShouldSucceed()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review { Id = reviewId };

        SetupReviewGetMock(review);
        SetupReviewLikeMock(null); // No existing like
        SetupReviewLikeRepository();

        // Act
        var result = await _reviewService.LikeReviewAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<ReviewLike>()
            .AddAsync(It.IsAny<ReviewLike>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LikeReviewAsync_AlreadyLiked_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var review = new Review { Id = reviewId };
        var existingLike = new ReviewLike
        {
            ReviewId = reviewId,
            UserId = userId
        };

        SetupReviewGetMock(review);
        SetupReviewLikeMock(existingLike);

        // Act
        var result = await _reviewService.LikeReviewAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_LIKED");
    }

    #endregion

    #region UnlikeReviewAsync Tests

    [Fact]
    public async Task UnlikeReviewAsync_ExistingLike_ShouldSucceed()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var like = new ReviewLike
        {
            ReviewId = reviewId,
            UserId = userId
        };

        SetupReviewLikeMock(like);
        SetupReviewLikeRepository();

        // Act
        var result = await _reviewService.UnlikeReviewAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.Repository<ReviewLike>().Delete(like), Times.Once);
    }

    [Fact]
    public async Task UnlikeReviewAsync_NoLike_ShouldFail()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupReviewLikeMock(null);

        // Act
        var result = await _reviewService.UnlikeReviewAsync(reviewId, userId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_LIKED");
    }

    #endregion

    #region UpdateRatingAsync Tests

    [Fact]
    public async Task UpdateRatingAsync_WithReviews_ShouldCalculateCorrectly()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            new() { Rating = 5, IsApproved = true, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Rating = 4, IsApproved = true, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Rating = 5, IsApproved = true, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new() { Rating = 3, IsApproved = true, IsDeleted = false, CreatedAt = DateTime.UtcNow }
        };

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = "Merchant"
        };

        SetupReviewsListMock(reviews);
        SetupRatingMock(rating);

        // Act
        var result = await _reviewService.UpdateRatingAsync(entityId, "Merchant");

        // Assert
        result.Success.Should().BeTrue();
        rating.TotalReviews.Should().Be(4);
        rating.AverageRating.Should().BeApproximately(4.25m, 0.01m); // (5+4+5+3)/4 = 4.25
        rating.FiveStarCount.Should().Be(2);
        rating.FourStarCount.Should().Be(1);
        rating.ThreeStarCount.Should().Be(1);
    }

    [Fact]
    public async Task UpdateRatingAsync_NoReviews_ShouldResetRating()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            EntityType = "Merchant",
            AverageRating = 4.5m,
            TotalReviews = 10
        };

        SetupReviewsListMock(new List<Review>()); // No reviews
        SetupRatingMock(rating);

        // Act
        var result = await _reviewService.UpdateRatingAsync(entityId, "Merchant");

        // Assert
        result.Success.Should().BeTrue();
        rating.AverageRating.Should().Be(0m);
        rating.TotalReviews.Should().Be(0);
    }

    #endregion

    #region GetTagFrequenciesAsync Tests

    [Fact]
    public async Task GetTagFrequenciesAsync_WithTags_ShouldReturnFrequencies()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var reviews = new List<Review>
        {
            new() { Id = Guid.NewGuid(), IsApproved = true },
            new() { Id = Guid.NewGuid(), IsApproved = true }
        };

        var tags = new List<ReviewTag>
        {
            new() { ReviewId = reviews[0].Id, Tag = "Fast", IsPositive = true },
            new() { ReviewId = reviews[0].Id, Tag = "Quality", IsPositive = true },
            new() { ReviewId = reviews[1].Id, Tag = "Fast", IsPositive = true }
        };

        SetupReviewsListMock(reviews);
        SetupReviewTagsListMock(tags);

        // Act
        var result = await _reviewService.GetTagFrequenciesAsync(entityId, "Merchant");

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(t => t.Tag == "Fast" && t.Count == 2);
        result.Value.Should().Contain(t => t.Tag == "Quality" && t.Count == 1);
    }

    #endregion

    #region Helper Methods

    private void SetupOrderMock(Order? order)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Order>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Order>()).Returns(readRepoMock.Object);
    }

    private void SetupCanReviewMock(bool canReview)
    {
        var order = canReview ? new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Delivered
        } : null;

        SetupOrderMock(order);
    }

    private void SetupHasReviewedMock(bool hasReviewed)
    {
        var review = hasReviewed ? new Review
        {
            Id = Guid.NewGuid(),
            IsDeleted = false
        } : null;

        SetupHasReviewedOrderMock(review);
    }

    private void SetupHasReviewedOrderMock(Review? review)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Review>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Review>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewMock(Review? review)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Review>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Review>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewGetMock(Review? review)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Review>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Review>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewRepositories()
    {
        var reviewRepoMock = new Mock<IGenericRepository<Review>>();
        var reviewTagRepoMock = new Mock<IGenericRepository<ReviewTag>>();
        
        _unitOfWorkMock.Setup(u => u.Repository<Review>()).Returns(reviewRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<ReviewTag>()).Returns(reviewTagRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupReviewTagsListMock(List<ReviewTag> tags)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<ReviewTag>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ReviewTag, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReviewTag, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ReviewTag>()).Returns(readRepoMock.Object);
    }

    private void SetupRatingRepository()
    {
        var ratingRepoMock = new Mock<IGenericRepository<Rating>>();
        var ratingReadRepoMock = new Mock<IReadOnlyRepository<Rating>>();
        
        ratingReadRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Rating, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Rating?)null);

        _unitOfWorkMock.Setup(u => u.Repository<Rating>()).Returns(ratingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadRepository<Rating>()).Returns(ratingReadRepoMock.Object);
    }

    private void SetupRatingMock(Rating? rating)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Rating>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Rating, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(rating);

        var ratingRepoMock = new Mock<IGenericRepository<Rating>>();
        
        _unitOfWorkMock.Setup(u => u.ReadRepository<Rating>()).Returns(readRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Rating>()).Returns(ratingRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupReviewsListMock(List<Review> reviews)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<Review>>();
        readRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Review, bool>>>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Review, object>>>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<int?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        _unitOfWorkMock.Setup(u => u.ReadRepository<Review>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewReportMock(ReviewReport? report)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<ReviewReport>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ReviewReport, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ReviewReport>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewReportRepository()
    {
        var reportRepoMock = new Mock<IGenericRepository<ReviewReport>>();
        _unitOfWorkMock.Setup(u => u.Repository<ReviewReport>()).Returns(reportRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupReviewHelpfulMock(ReviewHelpful? vote)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<ReviewHelpful>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ReviewHelpful, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(vote);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ReviewHelpful>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewHelpfulRepository()
    {
        var helpfulRepoMock = new Mock<IGenericRepository<ReviewHelpful>>();
        _unitOfWorkMock.Setup(u => u.Repository<ReviewHelpful>()).Returns(helpfulRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    private void SetupHelpfulCountMocks(int helpfulCount, int notHelpfulCount)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<ReviewHelpful>>();
        
        readRepoMock.Setup(r => r.CountAsync(
            It.Is<System.Linq.Expressions.Expression<Func<ReviewHelpful, bool>>>(expr => 
                expr.ToString().Contains("IsHelpful")),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(helpfulCount);
        
        readRepoMock.Setup(r => r.CountAsync(
            It.Is<System.Linq.Expressions.Expression<Func<ReviewHelpful, bool>>>(expr => 
                expr.ToString().Contains("!IsHelpful") || expr.ToString().Contains("Not")),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(notHelpfulCount);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ReviewHelpful>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewLikeMock(ReviewLike? like)
    {
        var readRepoMock = new Mock<IReadOnlyRepository<ReviewLike>>();
        readRepoMock.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ReviewLike, bool>>>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(like);

        _unitOfWorkMock.Setup(u => u.ReadRepository<ReviewLike>()).Returns(readRepoMock.Object);
    }

    private void SetupReviewLikeRepository()
    {
        var likeRepoMock = new Mock<IGenericRepository<ReviewLike>>();
        _unitOfWorkMock.Setup(u => u.Repository<ReviewLike>()).Returns(likeRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(1));
    }

    #endregion
}

