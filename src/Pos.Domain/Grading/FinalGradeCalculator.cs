using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

/// <summary>
/// Domain service: итог зависит сразу от двух агрегатов (оценки студента и текущий рубрикатор),
/// поэтому не принадлежит ни одному из них. Итог каждый раз считается заново из оценок,
/// отдельно он нигде не хранится — значит, всегда согласован с текущим набором оценок.
/// </summary>
public sealed class FinalGradeCalculator
{
    public FinalGrade Calculate(Student student, Rubric rubric, IFinalGradeStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(rubric);
        ArgumentNullException.ThrowIfNull(strategy);

        if (student.Evaluations.Any(e => !rubric.Contains(e.CriterionId)))
        {
            throw new DomainException($"У студента {student.Name} есть оценка по критерию, которого нет в рубрикаторе.");
        }

        var components = rubric.Criteria
            .Select(c => new GradeComponent(c, student.EvaluationFor(c.Id)))
            .ToList();

        return new FinalGrade(
            strategy.Calculate(components),
            components.Count(c => c.IsGraded),
            components.Count,
            strategy.Title);
    }
}
