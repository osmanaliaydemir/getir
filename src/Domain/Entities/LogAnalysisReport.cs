using System.ComponentModel.DataAnnotations;

namespace Getir.Domain.Entities;

/// <summary>
/// Log analiz ve raporlama için entity
/// </summary>
public class LogAnalysisReport
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ReportType { get; set; } = string.Empty; // DAILY, WEEKLY, MONTHLY, CUSTOM, SECURITY, PERFORMANCE
    
    [Required]
    [MaxLength(200)]
    public string ReportTitle { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? ReportDescription { get; set; }
    
    public DateTime ReportStartDate { get; set; }
    
    public DateTime ReportEndDate { get; set; }
    
    [MaxLength(50)]
    public string? TimeZone { get; set; } = "UTC";
    
    [MaxLength(2000)]
    public string? ReportData { get; set; } // JSON format - report data and metrics
    
    [MaxLength(2000)]
    public string? Summary { get; set; } // JSON format - report summary
    
    [MaxLength(2000)]
    public string? Insights { get; set; } // JSON format - insights and recommendations
    
    [MaxLength(2000)]
    public string? Alerts { get; set; } // JSON format - alerts and warnings
    
    [MaxLength(2000)]
    public string? Charts { get; set; } // JSON format - chart configurations
    
    [MaxLength(100)]
    public string? Status { get; set; } = "GENERATED"; // GENERATING, GENERATED, FAILED, EXPIRED
    
    [MaxLength(50)]
    public string? Format { get; set; } = "JSON"; // JSON, PDF, CSV, EXCEL
    
    [MaxLength(500)]
    public string? FilePath { get; set; } // Path to generated report file
    
    [MaxLength(100)]
    public string? FileName { get; set; } // Generated file name
    
    public long? FileSizeBytes { get; set; }
    
    public Guid? GeneratedByUserId { get; set; }
    
    [MaxLength(100)]
    public string? GeneratedByUserName { get; set; }
    
    [MaxLength(100)]
    public string? GeneratedByRole { get; set; }
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public bool IsPublic { get; set; } = false;
    
    [MaxLength(1000)]
    public string? Recipients { get; set; } // JSON array of email addresses
    
    public bool IsScheduled { get; set; } = false;
    
    [MaxLength(100)]
    public string? SchedulePattern { get; set; } // Cron expression for scheduled reports
    
    public DateTime? NextScheduledRun { get; set; }
    
    public int GenerationTimeMs { get; set; } // Time taken to generate report in milliseconds
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public virtual User? GeneratedByUser { get; set; }
}
