using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Getir.WebApi.Endpoints;

public static class DatabaseTestEndpoints
{
    public static void MapDatabaseTestEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/database-test")
            .WithTags("Database Test");

        // Database connection test
        group.MapGet("/connection", async (AppDbContext context) =>
        {
            try
            {
                await context.Database.CanConnectAsync();
                return Results.Ok(new { 
                    Status = "Connected", 
                    Message = "Database connection successful",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Database connection failed: {ex.Message}");
            }
        });

        // Get indexes for Products table
        group.MapGet("/indexes", async (AppDbContext context) =>
        {
            try
            {
                var indexes = await context.Database.SqlQueryRaw<string>(
                    "SELECT name FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0 AND object_id = OBJECT_ID('Products')"
                ).ToListAsync();
                
                return Results.Ok(new { 
                    Table = "Products",
                    Indexes = indexes,
                    Count = indexes.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to get indexes: {ex.Message}");
            }
        });

        // Get statistics for Products table
        group.MapGet("/statistics", async (AppDbContext context) =>
        {
            try
            {
                var stats = await context.Database.SqlQueryRaw<string>(
                    "SELECT name FROM sys.stats WHERE object_id = OBJECT_ID('Products')"
                ).ToListAsync();
                
                return Results.Ok(new { 
                    Table = "Products",
                    Statistics = stats,
                    Count = stats.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to get statistics: {ex.Message}");
            }
        });

        // Performance test
        group.MapGet("/performance-test", async (AppDbContext context) =>
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var products = await context.Products.AsNoTracking().Take(1000).ToListAsync();
                stopwatch.Stop();
                
                return Results.Ok(new { 
                    Status = "Success",
                    Message = $"Fetched {products.Count} products in {stopwatch.ElapsedMilliseconds}ms",
                    ProductCount = products.Count,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Performance test failed: {ex.Message}");
            }
        });

        // Schema validation
        group.MapGet("/schema-validation", async (AppDbContext context) =>
        {
            try
            {
                var tables = await context.Database.SqlQueryRaw<string>(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = DB_NAME()"
                ).ToListAsync();
                
                return Results.Ok(new { 
                    Status = "Success",
                    Tables = tables,
                    TableCount = tables.Count,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Schema validation failed: {ex.Message}");
            }
        });

        // Database health check
        group.MapGet("/health", async (AppDbContext context) =>
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Test basic connectivity
                var canConnect = await context.Database.CanConnectAsync();
                
                // Test a simple query
                var userCount = await context.Users.CountAsync();
                
                stopwatch.Stop();
                
                return Results.Ok(new
                {
                    Status = "Healthy",
                    CanConnect = canConnect,
                    UserCount = userCount,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Health check failed: {ex.Message}");
            }
        });

        // Get table row counts
        group.MapGet("/table-counts", async (AppDbContext context) =>
        {
            try
            {
                var tableCounts = new Dictionary<string, int>();
                
                // Get counts for major tables
                tableCounts["Users"] = await context.Users.CountAsync();
                tableCounts["Merchants"] = await context.Merchants.CountAsync();
                tableCounts["Products"] = await context.Products.CountAsync();
                tableCounts["Orders"] = await context.Orders.CountAsync();
                tableCounts["OrderLines"] = await context.OrderLines.CountAsync();
                tableCounts["Reviews"] = await context.Reviews.CountAsync();
                tableCounts["Coupons"] = await context.Coupons.CountAsync();
                tableCounts["Couriers"] = await context.Couriers.CountAsync();
                
                return Results.Ok(new
                {
                    Status = "Success",
                    TableCounts = tableCounts,
                    TotalRecords = tableCounts.Values.Sum(),
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to get table counts: {ex.Message}");
            }
        });
    }
}