namespace Getir.Domain.Enums;

/// <summary>
/// Inventory count types
/// </summary>
public enum InventoryCountType
{
    /// <summary>
    /// Full inventory count
    /// </summary>
    Full = 0,
    
    /// <summary>
    /// Partial inventory count
    /// </summary>
    Partial = 1,
    
    /// <summary>
    /// Cycle count
    /// </summary>
    Cycle = 2,
    
    /// <summary>
    /// Spot check
    /// </summary>
    SpotCheck = 3,
    
    /// <summary>
    /// Annual count
    /// </summary>
    Annual = 4
}
