using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>Максимальный балл по критерию.</summary>
public sealed record MaxScore
{
    public const decimal Limit = 1000m;

    public MaxScore(decimal value)
    {
        if (value <= 0 || value > Limit)
        {
            throw new DomainException($"Максимальный балл должен быть больше 0 и не больше {Text.Number(Limit)}.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Text.Number(Value);
}
