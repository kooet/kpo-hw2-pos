using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Students;

/// <summary>
/// Студент — корень агрегата. Внутри агрегата хранятся его оценки,
/// поэтому правило «не больше одной оценки на критерий» проверяется здесь.
/// </summary>
public sealed class Student
{
    private readonly List<Evaluation> _evaluations = new();

    public Student(StudentId id, PersonName name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public StudentId Id { get; }

    public PersonName Name { get; }

    public IReadOnlyList<Evaluation> Evaluations => _evaluations.AsReadOnly();

    public static Student Enroll(PersonName name) => new(StudentId.New(), name);

    public Evaluation? EvaluationFor(CriterionId criterionId) =>
        _evaluations.Find(e => e.CriterionId == criterionId);

    public void Grade(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        if (EvaluationFor(evaluation.CriterionId) is not null)
        {
            throw new DomainException(
                $"У студента {Name} уже есть оценка по этому критерию. Её можно исправить или отменить.");
        }

        _evaluations.Add(evaluation);
    }

    public void Correct(Evaluation evaluation)
    {
        ArgumentNullException.ThrowIfNull(evaluation);

        var index = _evaluations.FindIndex(e => e.CriterionId == evaluation.CriterionId);
        if (index < 0)
        {
            throw new DomainException($"У студента {Name} нет оценки по этому критерию, исправлять нечего.");
        }

        _evaluations[index] = evaluation;
    }

    public void CancelEvaluation(CriterionId criterionId)
    {
        var removed = _evaluations.RemoveAll(e => e.CriterionId == criterionId);
        if (removed == 0)
        {
            throw new DomainException($"У студента {Name} нет оценки по этому критерию, отменять нечего.");
        }
    }
}
