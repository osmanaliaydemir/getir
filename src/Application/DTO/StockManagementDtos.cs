using Getir.Domain.Enums;

namespace Getir.Application.DTO;

// Stock Update DTOs
public record UpdateStockRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int NewStockQuantity,
    string? Reason = null,
    string? Notes = null);

public record BulkUpdateStockRequest(
    List<UpdateStockRequest> StockUpdates,
    string? Reason = null);

// Stock History DTOs
public record StockHistoryResponse(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    int PreviousQuantity,
    int NewQuantity,
    int ChangeAmount,
    Domain.Enums.StockChangeType ChangeType,
    string? Reason,
    string? Notes,
    Guid? ChangedBy,
    string? ChangedByName,
    DateTime ChangedAt,
    string? OrderId,
    string? OrderNumber);

public record StockHistoryRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    DateTime? FromDate,
    DateTime? ToDate,
    StockChangeType? ChangeType,
    int Page = 1,
    int PageSize = 50);

// Stock Alert DTOs
public record StockAlertResponse(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    int CurrentStock,
    int MinimumStock,
    int MaximumStock,
    Domain.Enums.StockAlertType AlertType,
    string Message,
    DateTime CreatedAt,
    bool IsResolved,
    DateTime? ResolvedAt);

public record StockAlertRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int MinimumStock,
    int MaximumStock,
    bool IsActive = true);

// Stock Report DTOs
public record StockReportRequest(
    Guid? MerchantId,
    DateTime? FromDate,
    DateTime? ToDate,
    StockReportType ReportType,
    List<Guid>? ProductIds = null,
    List<Guid>? CategoryIds = null,
    bool IncludeVariants = true);

public record StockReportResponse(
    DateTime GeneratedAt,
    StockReportType ReportType,
    StockSummaryResponse Summary,
    List<StockItemReportResponse> Items,
    List<StockMovementResponse> Movements,
    List<StockAlertResponse> Alerts);

public record StockSummaryResponse(
    int TotalProducts,
    int TotalVariants,
    int LowStockItems,
    int OutOfStockItems,
    int OverstockItems,
    decimal TotalValue,
    decimal AverageStockValue,
    int TotalMovements,
    int StockInMovements,
    int StockOutMovements);

public record StockItemReportResponse(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string CategoryName,
    int CurrentStock,
    int MinimumStock,
    int MaximumStock,
    decimal UnitPrice,
    decimal TotalValue,
    StockStatus Status,
    DateTime LastMovement,
    int MovementCount,
    decimal MovementValue);

public record StockMovementResponse(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    int Quantity,
    Domain.Enums.StockChangeType ChangeType,
    string? Reason,
    DateTime MovementDate,
    Guid? OrderId,
    string? OrderNumber,
    Guid? ChangedBy,
    string? ChangedByName);

// Stock Synchronization DTOs
public record StockSyncRequest(
    Guid MerchantId,
    string ExternalSystemId,
    List<ExternalStockItem> ExternalItems);

public record ExternalStockItem(
    string ExternalProductId,
    string ExternalVariantId,
    int StockQuantity,
    decimal? Price,
    bool IsAvailable);

public record StockSyncResponse(
    int TotalItems,
    int SyncedItems,
    int FailedItems,
    List<StockSyncError> Errors);

public record StockSyncError(
    string ExternalProductId,
    string ExternalVariantId,
    string ErrorMessage,
    string ErrorCode);

// Stock Settings DTOs
public record StockSettingsResponse(
    Guid MerchantId,
    bool AutoStockReduction,
    bool LowStockAlerts,
    bool OverstockAlerts,
    int DefaultMinimumStock,
    int DefaultMaximumStock,
    bool EnableStockSync,
    string? ExternalSystemId,
    DateTime? LastSyncAt);

public record UpdateStockSettingsRequest(
    bool AutoStockReduction,
    bool LowStockAlerts,
    bool OverstockAlerts,
    int DefaultMinimumStock,
    int DefaultMaximumStock,
    bool EnableStockSync,
    string? ExternalSystemId);

// Enums
public enum StockChangeType
{
    OrderReduction = 0,
    OrderRestoration = 1,
    ManualAdjustment = 2,
    StockIn = 3,
    StockOut = 4,
    Damage = 5,
    Expired = 6,
    Return = 7,
    Transfer = 8,
    Sync = 9
}

public enum StockAlertType
{
    LowStock = 0,
    OutOfStock = 1,
    Overstock = 2,
    NegativeStock = 3,
    ExpiringSoon = 4,
    Expired = 5
}

public enum StockReportType
{
    CurrentStock = 0,
    StockMovements = 1,
    LowStock = 2,
    Overstock = 3,
    StockValue = 4,
    MovementHistory = 5,
    AlertSummary = 6
}

public enum StockStatus
{
    InStock = 0,
    LowStock = 1,
    OutOfStock = 2,
    Overstock = 3,
    Discontinued = 4
}

// Inventory Management DTOs
public record InventoryCountRequest(
    InventoryCountType CountType,
    string? Notes,
    List<InventoryCountItemRequest> Items);

public record InventoryCountItemRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int CountedQuantity,
    string? Notes);

public record InventoryCountResponse(
    Guid Id,
    DateTime CountDate,
    InventoryCountType CountType,
    InventoryCountStatus Status,
    int DiscrepancyCount,
    string? Notes,
    string? CreatedByName,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record InventoryLevelResponse(
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    string CategoryName,
    int CurrentStock,
    int MinimumStock,
    int MaximumStock,
    decimal UnitPrice,
    decimal TotalValue,
    StockStatus Status,
    DateTime LastUpdated);

public record InventoryDiscrepancyResponse(
    Guid Id,
    Guid ProductId,
    Guid? ProductVariantId,
    string ProductName,
    string? VariantName,
    int ExpectedQuantity,
    int ActualQuantity,
    int Variance,
    decimal VariancePercentage,
    InventoryDiscrepancyStatus Status,
    string? ResolutionNotes,
    DateTime CreatedAt,
    DateTime? ResolvedAt);

public record InventoryAdjustmentRequest(
    Guid ProductId,
    Guid? ProductVariantId,
    int NewQuantity);

public record InventoryTurnoverResponse(
    DateTime FromDate,
    DateTime ToDate,
    int TotalItems,
    decimal TotalValue,
    decimal AverageTurnoverRate,
    List<InventoryTurnoverItem> FastMovingItems,
    List<InventoryTurnoverItem> SlowMovingItems);

public record InventoryTurnoverItem(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    int StockOutQuantity,
    decimal TurnoverRate,
    decimal DaysToTurnover,
    decimal UnitPrice,
    decimal StockValue);

public record SlowMovingInventoryResponse(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    decimal UnitPrice,
    decimal TotalValue,
    DateTime? LastMovementDate,
    int DaysSinceLastMovement);

public record InventoryValuationResponse(
    ValuationMethod Method,
    DateTime ValuationDate,
    decimal TotalValue,
    int TotalItems,
    List<InventoryValuationItem> TopValueItems);

public record InventoryValuationItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitValue,
    decimal TotalValue);

// Stock Alert DTOs
public record StockAlertSettingsRequest(
    bool AutoStockReduction,
    bool LowStockAlerts,
    bool OverstockAlerts,
    int DefaultMinimumStock,
    int DefaultMaximumStock,
    bool EnableStockSync,
    string? ExternalSystemId);

public record StockAlertStatisticsResponse(
    int TotalAlerts,
    int LowStockAlerts,
    int OutOfStockAlerts,
    int OverstockAlerts,
    int ResolvedAlerts,
    int PendingAlerts,
    Dictionary<StockAlertType, int> AlertsByType,
    DateTime FromDate,
    DateTime ToDate);

// Stock Synchronization DTOs
public record StockSyncHistoryResponse(
    Guid Id,
    string ExternalSystemId,
    StockSyncType SyncType,
    StockSyncStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int SyncedItemsCount,
    int FailedItemsCount,
    string? ErrorMessage);

public record StockSyncStatusResponse(
    bool IsEnabled,
    string? ExternalSystemId,
    DateTime? LastSyncAt,
    StockSyncStatus? LastSyncStatus,
    int SyncIntervalMinutes,
    DateTime? LastSyncStartedAt);

public record ExternalSystemConfigRequest(
    bool EnableStockSync,
    string? ExternalSystemId,
    string? ApiUrl,
    string? ApiKey,
    int SyncIntervalMinutes);

public record ConnectionTestResponse(
    bool IsSuccessful,
    string Message,
    DateTime TestedAt,
    string? ErrorMessage);