using Getir.Domain.Enums;

namespace Getir.Application.DTO;

// Request DTOs
public record ChangeOrderStatusRequest(
    Guid OrderId,
    OrderStatus NewStatus,
    string? Reason = null,
    string? Notes = null,
    Dictionary<string, object>? AdditionalData = null);

public record RollbackOrderStatusRequest(
    Guid OrderId,
    string? Reason = null);

// Response DTOs
public record OrderStatusTransitionResponse(
    OrderStatus Status,
    string DisplayName,
    string Description,
    bool IsAvailable,
    List<string> RequiredData,
    string? UnavailableReason = null);

public record OrderStatusTransitionLogResponse(
    Guid Id,
    Guid OrderId,
    OrderStatus FromStatus,
    OrderStatus ToStatus,
    string FromStatusDisplayName,
    string ToStatusDisplayName,
    Guid ChangedBy,
    string ChangedByRole,
    string? Reason,
    string? Notes,
    DateTime ChangedAt,
    string? IpAddress,
    bool IsRollback,
    Guid? RollbackFromLogId);

public record OrderStatusValidationResponse(
    bool IsValid,
    string? ErrorMessage,
    string? ErrorCode,
    List<string> RequiredData,
    List<OrderStatusTransitionResponse> AvailableTransitions);

// Bulk operations
public record BulkOrderStatusChangeRequest(
    List<Guid> OrderIds,
    OrderStatus NewStatus,
    string? Reason = null,
    string? Notes = null);

public record BulkOrderStatusChangeResponse(
    int SuccessCount,
    int FailureCount,
    List<OrderStatusChangeResult> Results);

public record OrderStatusChangeResult(
    Guid OrderId,
    bool Success,
    string? ErrorMessage,
    string? ErrorCode);
