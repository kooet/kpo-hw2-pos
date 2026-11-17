namespace Pos.Domain.Grading;

/// <summary>
/// Итоговое правило: учитываются все критерии рубрикатора, невыставленная оценка считается нулём.
/// </summary>
public sealed class WeightedAllCriteriaStrategy : IFinalGradeStrategy
{
    public string Title => "Взвешенное среднее по всем критериям (нет оценки — 0)";

    public decimal Calculate(IReadOnlyList<GradeComponent> components) => WeightedAverage.Of(components);
}
