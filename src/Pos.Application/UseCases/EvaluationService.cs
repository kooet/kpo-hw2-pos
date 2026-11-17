using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class EvaluationService
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly EvaluationFactory _factory;
    private readonly TimeProvider _clock;

    public EvaluationService(
        IStudentRepository students,
        IRubricRepository rubrics,
        EvaluationFactory factory,
        TimeProvider clock)
    {
        _students = students;
        _rubrics = rubrics;
        _factory = factory;
        _clock = clock;
    }

    public void Grade(StudentId studentId, CriterionId criterionId, decimal points, string? comment)
    {
        var student = _students.GetRequired(studentId);
        student.Grade(Create(criterionId, points, comment));
        _students.Update(student);
    }

    public void Correct(StudentId studentId, CriterionId criterionId, decimal points, string? comment)
    {
        var student = _students.GetRequired(studentId);
        student.Correct(Create(criterionId, points, comment));
        _students.Update(student);
    }

    public void Cancel(StudentId studentId, CriterionId criterionId)
    {
        var student = _students.GetRequired(studentId);
        student.CancelEvaluation(criterionId);
        _students.Update(student);
    }

    /// <summary>Оценки студента в порядке критериев рубрикатора.</summary>
    public IReadOnlyList<EvaluationView> ListFor(StudentId studentId)
    {
        var student = _students.GetRequired(studentId);
        var rubric = _rubrics.Get();

        var result = new List<EvaluationView>();
        foreach (var criterion in rubric.Criteria)
        {
            var evaluation = student.EvaluationFor(criterion.Id);
            if (evaluation is not null)
            {
                result.Add(EvaluationView.From(criterion, evaluation));
            }
        }

        return result;
    }

    private Evaluation Create(CriterionId criterionId, decimal points, string? comment)
    {
        var today = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        return _factory.Create(_rubrics.Get(), criterionId, points, today, comment);
    }
}
