using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class RubricService
{
    private readonly IRubricRepository _rubrics;
    private readonly IStudentRepository _students;

    public RubricService(IRubricRepository rubrics, IStudentRepository students)
    {
        _rubrics = rubrics;
        _students = students;
    }

    public CriterionView AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var rubric = _rubrics.Get();
        var criterion = rubric.AddCriterion(new CriterionName(name), new MaxScore(maxScore), new Weight(weight));
        _rubrics.Save(rubric);
        return CriterionView.From(criterion);
    }

    public IReadOnlyList<CriterionView> List() =>
        _rubrics.Get().Criteria.Select(CriterionView.From).ToList();

    public void ChangeWeight(CriterionId id, decimal weight)
    {
        var rubric = _rubrics.Get();
        rubric.ChangeWeight(id, new Weight(weight));
        _rubrics.Save(rubric);
    }

    /// <summary>
    /// Критерий с выставленными оценками удалить нельзя: иначе у студентов останутся оценки
    /// «ни за что». Проверка затрагивает два агрегата, поэтому она здесь, а не в Rubric.
    /// </summary>
    public void Remove(CriterionId id)
    {
        var rubric = _rubrics.Get();
        var criterion = rubric.Get(id);

        var gradedStudents = _students.GetAll().Count(s => s.EvaluationFor(id) is not null);
        if (gradedStudents > 0)
        {
            throw new UseCaseException(
                $"Критерий «{criterion.Name}» нельзя удалить: по нему уже есть оценки ({gradedStudents}). Сначала отмените их.");
        }

        rubric.Remove(id);
        _rubrics.Save(rubric);
    }
}
