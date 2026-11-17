using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Xunit;

namespace Pos.Tests;

public class EvaluationFactoryTests
{
    private readonly EvaluationFactory _factory = new();
    private readonly Rubric _rubric = TestData.CreateRubric(("ДЗ", 10, 1));

    [Theory]
    [InlineData(0.0)]
    [InlineData(7.5)]
    [InlineData(10.0)]
    public void Create_ScoreWithinCriterion_Succeeds(double value)
    {
        var points = (decimal)value;
        var criterion = _rubric.GetCriterion("ДЗ");

        var evaluation = _factory.Create(_rubric, criterion.Id, points, TestData.Today, "  хорошо  ");

        Assert.Equal(criterion.Id, evaluation.CriterionId);
        Assert.Equal(points, evaluation.Score.Value);
        Assert.Equal("хорошо", evaluation.Comment);
    }

    [Theory]
    [InlineData(10.01)]
    [InlineData(12.0)]
    public void Create_ScoreAboveMax_Throws(double points)
    {
        var error = Assert.Throws<DomainException>(() =>
            _factory.Create(_rubric, _rubric.GetCriterion("ДЗ").Id, (decimal)points, TestData.Today));

        Assert.Contains("максимум 10", error.Message);
    }

    [Fact]
    public void Create_NegativeScore_Throws()
    {
        Assert.Throws<DomainException>(() =>
            _factory.Create(_rubric, _rubric.GetCriterion("ДЗ").Id, -1, TestData.Today));
    }

    [Fact]
    public void Create_CriterionNotInRubric_Throws()
    {
        var foreignRubric = TestData.CreateRubric(("Эссе", 10, 1));

        var error = Assert.Throws<DomainException>(() =>
            _factory.Create(_rubric, foreignRubric.GetCriterion("Эссе").Id, 5, TestData.Today));

        Assert.Contains("рубрикатора", error.Message);
    }

    [Fact]
    public void Create_EmptyComment_BecomesNull()
    {
        var evaluation = _factory.Create(_rubric, _rubric.GetCriterion("ДЗ").Id, 5, TestData.Today, "   ");

        Assert.Null(evaluation.Comment);
    }
}
