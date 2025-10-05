-- Migration 011: Order Status Transition Log System

-- 1. OrderStatusTransitionLogs Tablosu
CREATE TABLE OrderStatusTransitionLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    FromStatus INT NOT NULL, -- OrderStatus enum
    ToStatus INT NOT NULL, -- OrderStatus enum
    ChangedBy UNIQUEIDENTIFIER NOT NULL,
    ChangedByRole NVARCHAR(50) NOT NULL,
    Reason NVARCHAR(500) NULL,
    Notes NVARCHAR(MAX) NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    IsRollback BIT NOT NULL DEFAULT 0,
    RollbackFromLogId UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_OrderStatusTransitionLogs_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderStatusTransitionLogs_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(Id),
    CONSTRAINT FK_OrderStatusTransitionLogs_RollbackFromLog FOREIGN KEY (RollbackFromLogId) REFERENCES OrderStatusTransitionLogs(Id)
);
GO

-- 2. Index'ler (performans için)
CREATE INDEX IX_OrderStatusTransitionLogs_OrderId ON OrderStatusTransitionLogs (OrderId);
CREATE INDEX IX_OrderStatusTransitionLogs_ChangedBy ON OrderStatusTransitionLogs (ChangedBy);
CREATE INDEX IX_OrderStatusTransitionLogs_ChangedAt ON OrderStatusTransitionLogs (ChangedAt);
CREATE INDEX IX_OrderStatusTransitionLogs_FromStatus ON OrderStatusTransitionLogs (FromStatus);
CREATE INDEX IX_OrderStatusTransitionLogs_ToStatus ON OrderStatusTransitionLogs (ToStatus);
CREATE INDEX IX_OrderStatusTransitionLogs_IsRollback ON OrderStatusTransitionLogs (IsRollback);
CREATE INDEX IX_OrderStatusTransitionLogs_ChangedByRole ON OrderStatusTransitionLogs (ChangedByRole);
GO

-- 3. Check constraints
ALTER TABLE OrderStatusTransitionLogs 
ADD CONSTRAINT CK_OrderStatusTransitionLogs_FromToStatus 
CHECK (FromStatus != ToStatus);
GO

ALTER TABLE OrderStatusTransitionLogs 
ADD CONSTRAINT CK_OrderStatusTransitionLogs_ValidStatus 
CHECK (FromStatus >= 0 AND FromStatus <= 6 AND ToStatus >= 0 AND ToStatus <= 6);
GO

-- 4. Trigger for automatic status change logging (optional)
CREATE TRIGGER TR_Orders_StatusChange
ON Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only log if status actually changed
    IF UPDATE(Status) AND EXISTS (
        SELECT 1 FROM inserted i 
        INNER JOIN deleted d ON i.Id = d.Id 
        WHERE i.Status != d.Status
    )
    BEGIN
        INSERT INTO OrderStatusTransitionLogs (
            OrderId, FromStatus, ToStatus, ChangedBy, ChangedByRole, 
            ChangedAt, Reason, Notes
        )
        SELECT 
            i.Id,
            d.Status,
            i.Status,
            i.UpdatedBy, -- Assuming we add this field
            'System', -- Default role for system changes
            GETUTCDATE(),
            'System Status Update',
            'Automatic status change detected'
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.Status != d.Status;
    END
END
GO

-- 5. View for order status history
CREATE VIEW vw_OrderStatusHistory AS
SELECT 
    ostl.Id,
    ostl.OrderId,
    o.OrderNumber,
    ostl.FromStatus,
    ostl.ToStatus,
    CASE ostl.FromStatus
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Confirmed'
        WHEN 2 THEN 'Preparing'
        WHEN 3 THEN 'Ready'
        WHEN 4 THEN 'OnTheWay'
        WHEN 5 THEN 'Delivered'
        WHEN 6 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS FromStatusName,
    CASE ostl.ToStatus
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Confirmed'
        WHEN 2 THEN 'Preparing'
        WHEN 3 THEN 'Ready'
        WHEN 4 THEN 'OnTheWay'
        WHEN 5 THEN 'Delivered'
        WHEN 6 THEN 'Cancelled'
        ELSE 'Unknown'
    END AS ToStatusName,
    ostl.ChangedBy,
    u.FirstName + ' ' + u.LastName AS ChangedByName,
    ostl.ChangedByRole,
    ostl.Reason,
    ostl.Notes,
    ostl.ChangedAt,
    ostl.IpAddress,
    ostl.IsRollback,
    ostl.RollbackFromLogId
FROM OrderStatusTransitionLogs ostl
INNER JOIN Orders o ON ostl.OrderId = o.Id
LEFT JOIN Users u ON ostl.ChangedBy = u.Id;
GO

-- 6. Stored procedure for getting order status transitions
CREATE PROCEDURE sp_GetOrderStatusTransitions
    @OrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        OrderId,
        FromStatus,
        ToStatus,
        ChangedBy,
        ChangedByRole,
        Reason,
        Notes,
        ChangedAt,
        IpAddress,
        IsRollback,
        RollbackFromLogId
    FROM OrderStatusTransitionLogs
    WHERE OrderId = @OrderId
    ORDER BY ChangedAt ASC;
END
GO

-- 7. Stored procedure for rollback validation
CREATE PROCEDURE sp_ValidateStatusRollback
    @OrderId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @UserRole NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @LastTransitionId UNIQUEIDENTIFIER;
    DECLARE @LastFromStatus INT;
    DECLARE @LastToStatus INT;
    DECLARE @LastChangedBy UNIQUEIDENTIFIER;
    DECLARE @LastChangedByRole NVARCHAR(50);
    
    -- Get the last non-rollback transition
    SELECT TOP 1 
        @LastTransitionId = Id,
        @LastFromStatus = FromStatus,
        @LastToStatus = ToStatus,
        @LastChangedBy = ChangedBy,
        @LastChangedByRole = ChangedByRole
    FROM OrderStatusTransitionLogs
    WHERE OrderId = @OrderId 
        AND IsRollback = 0
    ORDER BY ChangedAt DESC;
    
    -- Return validation result
    SELECT 
        CASE 
            WHEN @LastTransitionId IS NULL THEN 0
            WHEN @UserRole = 'Admin' THEN 1
            WHEN @UserRole = 'MerchantOwner' AND @LastChangedByRole IN ('MerchantOwner', 'System') THEN 1
            WHEN @UserRole = 'Courier' AND @LastChangedByRole IN ('Courier', 'System') THEN 1
            WHEN @UserRole = 'Customer' AND @LastChangedByRole IN ('Customer', 'System') THEN 1
            ELSE 0
        END AS CanRollback,
        @LastTransitionId AS LastTransitionId,
        @LastFromStatus AS LastFromStatus,
        @LastToStatus AS LastToStatus;
END
GO

PRINT 'Migration 011 completed successfully - Order Status Transition Log System created';
