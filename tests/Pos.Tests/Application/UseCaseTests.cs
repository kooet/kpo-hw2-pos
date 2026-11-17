using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Application.Ports;
using Pos.Application.UseCases;
using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Students;
using Pos.Infrastructure.Storage;
using Xunit;

namespace Pos.Tests;

public class UseCaseTests
{
    private readonly InMemoryStorage _storage = new();
    private readonly FakeExporter _exporter = new();
    private readonly StudentService _students;
    private readonly RubricService _rubric;
    private readonly EvaluationService _evaluations;
    private readonly GradingRules _rules;
    private readonly FinalGradeService _finalGrades;

    public UseCaseTests()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero));
        _students = new StudentService(_storage);
        _rubric = new RubricService(_storage, _storage);
        _evaluations = new EvaluationService(_storage, _storage, new EvaluationFactory(), clock);
        _rules = new GradingRules(new IFinalGradeStrategy[] { new WeightedAllCriteriaStrategy(), new WeightedGradedOnlyStrategy() });
        _finalGrades = new FinalGradeService(_storage, _storage, new FinalGradeCalculator(), _rules, _exporter);
    }

    [Fact]
    public void Grade_StoresEvaluationWithTodayDate()
    {
        var student = _students.Enroll("Иванова Анна");
        var criterion = _rubric.AddCriterion("ДЗ", 10, 1);

        _evaluations.Grade(student.Id, criterion.Id, 8, "ok");

        var evaluation = Assert.Single(_evaluations.ListFor(student.Id));
        Assert.Equal(8, evaluation.Score);
        Assert.Equal(new DateOnly(2026, 11, 11), evaluation.Date);
        Assert.Equal("ok", evaluation.Comment);
    }

    [Fact]
    public void Grade_AboveMax_IsRejectedAndNothingStored()
    {
        var student = _students.Enroll("Иванова Анна");
        var criterion = _rubric.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<DomainException>(() => _evaluations.Grade(student.Id, criterion.Id, 12, null));
        Assert.Empty(_evaluations.ListFor(student.Id));
    }

    [Fact]
    public void Grade_UnknownStudent_Throws()
    {
        var criterion = _rubric.AddCriterion("ДЗ", 10, 1);

        Assert.Throws<UseCaseException>(() =>
            _evaluations.Grade(StudentId.New(), criterion.Id, 5, null));
    }

    [Fact]
    public void RemoveCriterion_WithEvaluations_IsRefused()
    {
        var student = _students.Enroll("Иванова Анна");
        var criterion = _rubric.AddCriterion("ДЗ", 10, 1);
        _evaluations.Grade(student.Id, criterion.Id, 5, null);

        Assert.Throws<UseCaseException>(() => _rubric.Remove(criterion.Id));

        _evaluations.Cancel(student.Id, criterion.Id);
        _rubric.Remove(criterion.Id);
        Assert.Empty(_rubric.List());
    }

    [Fact]
    public void SelectingRule_ChangesStatement()
    {
        var student = _students.Enroll("Иванова Анна");
        var homework = _rubric.AddCriterion("ДЗ", 10, 1);
        _rubric.AddCriterion("Экзамен", 10, 1);
        _evaluations.Grade(student.Id, homework.Id, 7, null);

        var allCriteria = Assert.Single(_finalGrades.Statement());
        _rules.Select(1, roundToInteger: false);
        var gradedOnly = Assert.Single(_finalGrades.Statement());
        _rules.Select(0, roundToInteger: true);
        var rounded = Assert.Single(_finalGrades.Statement());

        Assert.Equal(3.5m, allCriteria.Value);
        Assert.Equal(7m, gradedOnly.Value);
        Assert.Equal(4m, rounded.Value);
        Assert.Contains("округлением", rounded.Rule);
    }

    [Fact]
    public void SelectingUnknownRule_Throws()
    {
        Assert.Throws<UseCaseException>(() => _rules.Select(5, roundToInteger: false));
    }

    [Fact]
    public void Export_SendsStatementToPort()
    {
        _students.Enroll("Иванова Анна");
        _students.Enroll("Петров Борис");
        _rubric.AddCriterion("ДЗ", 10, 1);

        var count = _finalGrades.Export("report.csv");

        Assert.Equal(2, count);
        Assert.Equal("report.csv", _exporter.Destination);
        Assert.Equal(new[] { "Иванова Анна", "Петров Борис" }, _exporter.Rows.Select(r => r.Student));
    }

    private sealed class FakeExporter : IFinalReportExporter
    {
        public IReadOnlyList<FinalGradeView> Rows { get; private set; } = Array.Empty<FinalGradeView>();

        public string? Destination { get; private set; }

        public void Export(IReadOnlyList<FinalGradeView> rows, string destination)
        {
            Rows = rows;
            Destination = destination;
        }
    }
}
