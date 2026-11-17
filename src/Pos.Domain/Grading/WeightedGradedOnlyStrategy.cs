namespace Pos.Domain.Grading;

/// <summary>
/// Текущее правило: учитываются только критерии, по которым уже есть оценка.
/// Нужно в середине курса, чтобы видеть успеваемость, а не «недобор» по будущим работам.
/// </summary>
public sealed class WeightedGradedOnlyStrategy : IFinalGradeStrategy
{
    public string Title => "Взвешенное среднее только по выставленным оценкам";

    public decimal Calculate(IReadOnlyList<GradeComponent> components) =>
        WeightedAverage.Of(components.Where(c => c.IsGraded));
}
