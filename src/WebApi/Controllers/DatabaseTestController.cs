using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Database test controller for database testing and monitoring
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Tags("Database Test")]
public class DatabaseTestController : BaseController
{
    private readonly AppDbContext _context;

    public DatabaseTestController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Test database connection
    /// </summary>
    /// <returns>Connection status</returns>
    [HttpGet("connection")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new { 
                Status = "Connected", 
                Message = "Database connection successful",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Database connection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get indexes for Products table
    /// </summary>
    /// <returns>Table indexes</returns>
    [HttpGet("indexes")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIndexes()
    {
        try
        {
            var indexes = await _context.Database.SqlQueryRaw<string>(
                "SELECT name FROM sys.indexes WHERE is_primary_key = 0 AND is_unique_constraint = 0 AND object_id = OBJECT_ID('Products')"
            ).ToListAsync();
            
            return Ok(new { 
                Table = "Products",
                Indexes = indexes,
                Count = indexes.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Failed to get indexes: {ex.Message}");
        }
    }

    /// <summary>
    /// Get statistics for Products table
    /// </summary>
    /// <returns>Table statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _context.Database.SqlQueryRaw<string>(
                "SELECT name FROM sys.stats WHERE object_id = OBJECT_ID('Products')"
            ).ToListAsync();
            
            return Ok(new { 
                Table = "Products",
                Statistics = stats,
                Count = stats.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Failed to get statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Performance test
    /// </summary>
    /// <returns>Performance test results</returns>
    [HttpGet("performance-test")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PerformanceTest()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var products = await _context.Products.AsNoTracking().Take(1000).ToListAsync();
            stopwatch.Stop();
            
            return Ok(new { 
                Status = "Success",
                Message = $"Fetched {products.Count} products in {stopwatch.ElapsedMilliseconds}ms",
                ProductCount = products.Count,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Performance test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Schema validation
    /// </summary>
    /// <returns>Schema validation results</returns>
    [HttpGet("schema-validation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SchemaValidation()
    {
        try
        {
            var tables = await _context.Database.SqlQueryRaw<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = DB_NAME()"
            ).ToListAsync();
            
            return Ok(new { 
                Status = "Success",
                Tables = tables,
                TableCount = tables.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Schema validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Database health check
    /// </summary>
    /// <returns>Health check results</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Test basic connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Test a simple query
            var userCount = await _context.Users.CountAsync();
            
            stopwatch.Stop();
            
            return Ok(new
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
            return Problem($"Health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get table row counts
    /// </summary>
    /// <returns>Table row counts</returns>
    [HttpGet("table-counts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTableCounts()
    {
        try
        {
            var tableCounts = new Dictionary<string, int>();
            
            // Get counts for major tables
            tableCounts["Users"] = await _context.Users.CountAsync();
            tableCounts["Merchants"] = await _context.Merchants.CountAsync();
            tableCounts["Products"] = await _context.Products.CountAsync();
            tableCounts["Orders"] = await _context.Orders.CountAsync();
            tableCounts["OrderLines"] = await _context.OrderLines.CountAsync();
            tableCounts["Reviews"] = await _context.Reviews.CountAsync();
            tableCounts["Coupons"] = await _context.Coupons.CountAsync();
            tableCounts["Couriers"] = await _context.Couriers.CountAsync();
            
            return Ok(new
            {
                Status = "Success",
                TableCounts = tableCounts,
                TotalRecords = tableCounts.Values.Sum(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Problem($"Failed to get table counts: {ex.Message}");
        }
    }
}
