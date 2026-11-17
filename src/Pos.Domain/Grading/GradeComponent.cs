using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>Одна строка расчёта итога: критерий и оценка по нему (если она есть).</summary>
public sealed record GradeComponent(Criterion Criterion, Evaluation? Evaluation)
{
    public bool IsGraded => Evaluation is not null;

    public decimal Weight => Criterion.Weight.Value;

    /// <summary>Доля от максимума по критерию: 7 из 10 → 0.7.</summary>
    public decimal Share => Evaluation is null ? 0m : Evaluation.Score.Value / Criterion.MaxScore.Value;
}
