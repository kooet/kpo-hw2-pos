using Pos.Domain.Common;

namespace Pos.Domain.Grading;

/// <summary>
/// Балл. Сам по себе знает только, что он неотрицательный и не точнее сотых;
/// верхнюю границу задаёт критерий (<see cref="Rubrics.Criterion.EnsureAccepts"/>).
/// </summary>
public sealed record Score
{
    public Score(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException("Балл не может быть отрицательным.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new DomainException("Балл можно указать не точнее сотых.");
        }

        Value = value;
    }

    public decimal Value { get; }

    public override string ToString() => Text.Number(Value);
}
