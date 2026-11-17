using Pos.Domain.Common;

namespace Pos.Domain.Grading;

/// <summary>Итог по студенту по 10-балльной шкале и то, из чего он получен.</summary>
public sealed record FinalGrade
{
    public const decimal MaxValue = 10m;

    public FinalGrade(decimal value, int gradedCriteria, int totalCriteria, string rule)
    {
        if (value < 0 || value > MaxValue)
        {
            throw new DomainException($"Итог должен быть от 0 до {Text.Number(MaxValue)}.");
        }

        if (totalCriteria < 0 || gradedCriteria < 0 || gradedCriteria > totalCriteria)
        {
            throw new ArgumentOutOfRangeException(nameof(gradedCriteria));
        }

        Value = value;
        GradedCriteria = gradedCriteria;
        TotalCriteria = totalCriteria;
        Rule = rule;
    }

    public decimal Value { get; }

    public int GradedCriteria { get; }

    public int TotalCriteria { get; }

    public string Rule { get; }

    public bool IsComplete => GradedCriteria == TotalCriteria;
}
