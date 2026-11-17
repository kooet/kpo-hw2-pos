namespace Pos.Domain.Grading;

/// <summary>Правило, по которому из оценок получается итог (Strategy).</summary>
public interface IFinalGradeStrategy
{
    string Title { get; }

    /// <returns>Итог от 0 до <see cref="FinalGrade.MaxValue"/>.</returns>
    decimal Calculate(IReadOnlyList<GradeComponent> components);
}
