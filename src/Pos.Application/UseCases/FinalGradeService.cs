using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Application.Ports;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class FinalGradeService
{
    private readonly IStudentRepository _students;
    private readonly IRubricRepository _rubrics;
    private readonly FinalGradeCalculator _calculator;
    private readonly GradingRules _rules;
    private readonly IFinalReportExporter _exporter;

    public FinalGradeService(
        IStudentRepository students,
        IRubricRepository rubrics,
        FinalGradeCalculator calculator,
        GradingRules rules,
        IFinalReportExporter exporter)
    {
        _students = students;
        _rubrics = rubrics;
        _calculator = calculator;
        _rules = rules;
        _exporter = exporter;
    }

    public FinalGradeView CalculateFor(StudentId studentId)
    {
        var student = _students.GetRequired(studentId);
        var grade = _calculator.Calculate(student, _rubrics.Get(), _rules.Current);
        return FinalGradeView.From(student, grade);
    }

    public IReadOnlyList<FinalGradeView> Statement()
    {
        var rubric = _rubrics.Get();
        var strategy = _rules.Current;

        return _students.GetAll()
            .Select(s => FinalGradeView.From(s, _calculator.Calculate(s, rubric, strategy)))
            .ToList();
    }

    /// <returns>Сколько строк выгружено.</returns>
    public int Export(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new UseCaseException("Не указано, куда выгружать ведомость.");
        }

        var rows = Statement();
        _exporter.Export(rows, destination);
        return rows.Count;
    }
}
