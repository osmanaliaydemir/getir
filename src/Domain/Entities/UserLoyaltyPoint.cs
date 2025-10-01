namespace Getir.Domain.Entities;

/// <summary>
/// Kullanıcı sadakat puanları (Getir'de "Getir Kazandırır")
/// </summary>
public class UserLoyaltyPoint
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public int TotalEarned { get; set; }
    public int TotalSpent { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = default!;
    public virtual ICollection<LoyaltyPointTransaction> Transactions { get; set; } = new List<LoyaltyPointTransaction>();
}

