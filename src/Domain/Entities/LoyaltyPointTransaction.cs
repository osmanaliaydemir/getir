namespace Getir.Domain.Entities;

/// <summary>
/// Sadakat puanı işlem geçmişi
/// </summary>
public class LoyaltyPointTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public int Points { get; set; }
    public string Type { get; set; } = default!; // Earned, Spent, Expired
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = default!;
    public virtual Order? Order { get; set; }
    public virtual UserLoyaltyPoint? UserLoyaltyPoint { get; set; }
}

