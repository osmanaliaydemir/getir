namespace Getir.Application.DTO;

public record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string? ImageUrl,
    string? ActionUrl);

public record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsRead,
    string? ImageUrl,
    string? ActionUrl,
    DateTime CreatedAt);

public record MarkAsReadRequest(
    List<Guid> NotificationIds);
