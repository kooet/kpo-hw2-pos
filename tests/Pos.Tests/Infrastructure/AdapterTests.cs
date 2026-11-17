using System.Text;
using Pos.Application.Models;
using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Storage;
using Xunit;

namespace Pos.Tests;

public sealed class AdapterTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pos-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void JsonStorage_SavesAndLoadsData()
    {
        var path = Path.Combine(_directory, "pos.json");
        var factory = new EvaluationFactory();

        var first = new JsonFileStorage(path, factory);
        var rubric = first.Get();
        var criterion = rubric.AddCriterion(new CriterionName("ДЗ"), new MaxScore(10), new Weight(0.4m));
        first.Save(rubric);
        var student = TestData.NewStudent();
        student.Grade(factory.Create(rubric, criterion.Id, 7.5m, TestData.Today, "молодец"));
        first.Add(student);

        var second = new JsonFileStorage(path, factory);

        var loadedCriterion = Assert.Single(second.Get().Criteria);
        Assert.Equal(criterion.Id, loadedCriterion.Id);
        Assert.Equal(0.4m, loadedCriterion.Weight.Value);
        var loadedStudent = Assert.Single(second.GetAll());
        Assert.Equal(student.Id, loadedStudent.Id);
        var evaluation = Assert.Single(loadedStudent.Evaluations);
        Assert.Equal(7.5m, evaluation.Score.Value);
        Assert.Equal(TestData.Today, evaluation.Date);
        Assert.Equal("молодец", evaluation.Comment);
    }

    [Fact]
    public void JsonStorage_FileBreakingDomainRules_IsNotLoaded()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "broken.json");
        var criterionId = Guid.NewGuid();
        File.WriteAllText(path, $$"""
            {
              "criteria": [ { "id": "{{criterionId}}", "name": "ДЗ", "maxScore": 10, "weight": 1 } ],
              "students": [
                {
                  "id": "{{Guid.NewGuid()}}",
                  "fullName": "Иванова Анна",
                  "evaluations": [ { "criterionId": "{{criterionId}}", "score": 12, "date": "2026-11-11" } ]
                }
              ]
            }
            """);

        var error = Assert.Throws<InvalidDataException>(() => new JsonFileStorage(path, new EvaluationFactory()));

        Assert.IsType<DomainException>(error.InnerException);
    }

    [Fact]
    public void JsonStorage_NotJson_IsReported()
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, "garbage.json");
        File.WriteAllText(path, "это не json");

        Assert.Throws<InvalidDataException>(() => new JsonFileStorage(path, new EvaluationFactory()));
    }

    [Fact]
    public void CsvExporter_WritesExcelFriendlyCsv()
    {
        var path = Path.Combine(_directory, "report.csv");
        var rows = new[]
        {
            new FinalGradeView("Иванова Анна", 8.333m, 3, 3, "правило"),
            new FinalGradeView("Петров; Борис", 4.735m, 2, 3, "правило"),
        };

        new CsvFinalReportExporter().Export(rows, path);

        var bytes = File.ReadAllBytes(path);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes[..3]);
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        Assert.Equal("Студент;Итог;Оценено критериев;Всего критериев;Правило", lines[0]);
        Assert.Equal("Иванова Анна;8,33;3;3;правило", lines[1]);
        Assert.Equal("\"Петров; Борис\";4,74;2;3;правило", lines[2]);
    }
}
