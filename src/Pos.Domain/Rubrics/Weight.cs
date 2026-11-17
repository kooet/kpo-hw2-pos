using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Вклад критерия в итог. Веса не обязаны давать в сумме 1:
/// итог считается как взвешенное среднее, поэтому веса 0.4 / 0.6 и 2 / 3 равносильны.
/// </summary>
public sealed record Weight
{
    public const decimal Limit = 100m;

    public Weight(decimal value)
    {
        if (value <= 0 || value > Limit)
        {
            throw new DomainException($"Вес критерия должен быть больше 0 и не больше {Text.Number(Limit)}.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Text.Number(Value);
}
