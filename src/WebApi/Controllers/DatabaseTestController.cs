using Getir.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Getir.WebApi.Controllers;

/// <summary>
/// Veritabanı testi ve izleme için veritabanı test controller'ı
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
    /// Veritabanı bağlantısını test et
    /// </summary>
    /// <returns>Bağlantı durumu</returns>
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
    /// Products tablosu için indeksleri getir
    /// </summary>
    /// <returns>Tablo indeksleri</returns>
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
    /// Products tablosu için istatistikleri getir
    /// </summary>
    /// <returns>Tablo istatistikleri</returns>
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
    /// Performans testi
    /// </summary>
    /// <returns>Performans testi sonuçları</returns>
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
    /// Şema doğrulama
    /// </summary>
    /// <returns>Şema doğrulama sonuçları</returns>
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
    /// Veritabanı sağlık kontrolü
    /// </summary>
    /// <returns>Sağlık kontrolü sonuçları</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Temel bağlantıyı test et
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Basit bir sorguyu test et
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
    /// Tablo satır sayılarını getir
    /// </summary>
    /// <returns>Tablo satır sayıları</returns>
    [HttpGet("table-counts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTableCounts()
    {
        try
        {
            var tableCounts = new Dictionary<string, int>();
            
            // Büyük tablolar için kayıt sayıları
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
