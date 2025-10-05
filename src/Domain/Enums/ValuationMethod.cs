namespace Getir.Domain.Enums;

/// <summary>
/// Inventory valuation methods
/// </summary>
public enum ValuationMethod
{
    /// <summary>
    /// First In, First Out
    /// </summary>
    FIFO = 0,
    
    /// <summary>
    /// Last In, First Out
    /// </summary>
    LIFO = 1,
    
    /// <summary>
    /// Weighted Average Cost
    /// </summary>
    WeightedAverage = 2,
    
    /// <summary>
    /// Current Market Price
    /// </summary>
    MarketPrice = 3,
    
    /// <summary>
    /// Standard Cost
    /// </summary>
    StandardCost = 4
}
