using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Getir.IntegrationTests.Setup;

/// <summary>
/// Interceptor to automatically set RowVersion for InMemory database
/// </summary>
public class RowVersionInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            // Check if entity has RowVersion property
            var rowVersionProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "RowVersion");
            
            if (rowVersionProperty != null)
            {
                if (entry.State == EntityState.Added)
                {
                    // Set initial RowVersion for new entities
                    rowVersionProperty.CurrentValue = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Increment RowVersion for modified entities
                    var currentValue = rowVersionProperty.CurrentValue as byte[];
                    if (currentValue != null && currentValue.Length > 0)
                    {
                        var newValue = new byte[currentValue.Length];
                        Array.Copy(currentValue, newValue, currentValue.Length);
                        newValue[^1]++;
                        rowVersionProperty.CurrentValue = newValue;
                    }
                    else
                    {
                        rowVersionProperty.CurrentValue = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
                    }
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

