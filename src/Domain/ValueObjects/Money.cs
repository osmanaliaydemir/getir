namespace Getir.Domain.ValueObjects;

public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    private readonly decimal _amount;

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative", nameof(amount));
        
        _amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public decimal Amount => _amount;

    public static Money Zero => new(0m);

    public static Money operator +(Money left, Money right) => new(left._amount + right._amount);
    public static Money operator -(Money left, Money right) => new(left._amount - right._amount);
    public static Money operator *(Money money, decimal multiplier) => new(money._amount * multiplier);
    public static Money operator *(decimal multiplier, Money money) => new(money._amount * multiplier);
    public static Money operator /(Money money, decimal divisor) => new(money._amount / divisor);

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

    public static implicit operator decimal(Money money) => money._amount;
    public static implicit operator Money(decimal amount) => new(amount);

    public bool Equals(Money other) => _amount == other._amount;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => _amount.GetHashCode();
    public int CompareTo(Money other) => _amount.CompareTo(other._amount);

    public override string ToString() => _amount.ToString("C");
    public string ToString(string format) => _amount.ToString(format);
}
