using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Xunit;

namespace Pos.Tests;

public class StudentTests
{
    private readonly Rubric _rubric = TestData.CreateRubric(("ДЗ", 10, 1), ("Контрольная", 100, 1));

    [Fact]
    public void Grade_SecondTimeForSameCriterion_Throws()
    {
        var student = TestData.NewStudent();
        student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 7));

        Assert.Throws<DomainException>(() => student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 9)));
        Assert.Equal(7, student.EvaluationFor(_rubric.GetCriterion("ДЗ").Id)!.Score.Value);
    }

    [Fact]
    public void Correct_ReplacesExistingEvaluation()
    {
        var student = TestData.NewStudent();
        student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 7));

        student.Correct(TestData.MakeEvaluation(_rubric, "ДЗ", 9));

        var evaluation = Assert.Single(student.Evaluations);
        Assert.Equal(9, evaluation.Score.Value);
    }

    [Fact]
    public void Correct_WithoutEvaluation_Throws()
    {
        var student = TestData.NewStudent();

        Assert.Throws<DomainException>(() => student.Correct(TestData.MakeEvaluation(_rubric, "ДЗ", 9)));
    }

    [Fact]
    public void CancelEvaluation_RemovesOnlyThatCriterion()
    {
        var student = TestData.NewStudent();
        student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 7));
        student.Grade(TestData.MakeEvaluation(_rubric, "Контрольная", 80));

        student.CancelEvaluation(_rubric.GetCriterion("ДЗ").Id);

        var left = Assert.Single(student.Evaluations);
        Assert.Equal(_rubric.GetCriterion("Контрольная").Id, left.CriterionId);
    }

    [Fact]
    public void CancelEvaluation_Missing_Throws()
    {
        var student = TestData.NewStudent();

        Assert.Throws<DomainException>(() => student.CancelEvaluation(_rubric.GetCriterion("ДЗ").Id));
    }
}
