using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Xunit;

namespace Pos.Tests;

public class FinalGradeTests
{
    private readonly FinalGradeCalculator _calculator = new();

    // ДЗ: 9 из 10 (0.9), Контрольная: не выставлена, Активность: 4 из 5 (0.8)
    private readonly Rubric _rubric = TestData.CreateRubric(
        ("ДЗ", 10, 0.4m),
        ("Контрольная", 100, 0.35m),
        ("Активность", 5, 0.25m));

    private Student GradedStudent()
    {
        var student = TestData.NewStudent();
        student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 9));
        student.Grade(TestData.MakeEvaluation(_rubric, "Активность", 4));
        return student;
    }

    [Fact]
    public void AllCriteria_MissingEvaluationCountsAsZero()
    {
        var grade = _calculator.Calculate(GradedStudent(), _rubric, new WeightedAllCriteriaStrategy());

        // 10 * (0.4 * 0.9 + 0.35 * 0 + 0.25 * 0.8) / 1.0 = 5.6
        Assert.Equal(5.6m, grade.Value);
        Assert.Equal(2, grade.GradedCriteria);
        Assert.Equal(3, grade.TotalCriteria);
        Assert.False(grade.IsComplete);
    }

    [Fact]
    public void GradedOnly_IgnoresMissingCriteria()
    {
        var grade = _calculator.Calculate(GradedStudent(), _rubric, new WeightedGradedOnlyStrategy());

        // 10 * (0.4 * 0.9 + 0.25 * 0.8) / (0.4 + 0.25) = 5.6 / 0.65
        Assert.Equal(8.62m, Math.Round(grade.Value, 2));
    }

    [Fact]
    public void NoEvaluations_GivesZeroForBothRules()
    {
        var student = TestData.NewStudent();

        Assert.Equal(0, _calculator.Calculate(student, _rubric, new WeightedAllCriteriaStrategy()).Value);
        Assert.Equal(0, _calculator.Calculate(student, _rubric, new WeightedGradedOnlyStrategy()).Value);
    }

    [Fact]
    public void AllMaximumScores_GiveExactlyTen()
    {
        var student = TestData.NewStudent();
        student.Grade(TestData.MakeEvaluation(_rubric, "ДЗ", 10));
        student.Grade(TestData.MakeEvaluation(_rubric, "Контрольная", 100));
        student.Grade(TestData.MakeEvaluation(_rubric, "Активность", 5));

        var grade = _calculator.Calculate(student, _rubric, new WeightedAllCriteriaStrategy());

        Assert.Equal(10m, grade.Value);
        Assert.True(grade.IsComplete);
    }

    [Fact]
    public void Result_FollowsCurrentEvaluations()
    {
        var student = GradedStudent();
        var before = _calculator.Calculate(student, _rubric, new WeightedAllCriteriaStrategy());

        student.CancelEvaluation(_rubric.GetCriterion("Активность").Id);
        var after = _calculator.Calculate(student, _rubric, new WeightedAllCriteriaStrategy());

        Assert.Equal(5.6m, before.Value);
        Assert.Equal(3.6m, after.Value);
    }

    [Fact]
    public void EvaluationForCriterionMissingFromRubric_Throws()
    {
        var student = GradedStudent();
        var otherRubric = TestData.CreateRubric(("ДЗ", 10, 1));

        Assert.Throws<DomainException>(() =>
            _calculator.Calculate(student, otherRubric, new WeightedAllCriteriaStrategy()));
    }

    [Theory]
    [InlineData(7.5, 8.0)]
    [InlineData(7.49, 7.0)]
    [InlineData(8.5, 9.0)]
    public void RoundedDecorator_RoundsHalfUp(double raw, double expected)
    {
        var rounded = new RoundedFinalGradeStrategy(new ConstantStrategy((decimal)raw));

        Assert.Equal((decimal)expected, rounded.Calculate(Array.Empty<GradeComponent>()));
        Assert.Equal("константа, с округлением до целого", rounded.Title);
    }

    [Fact]
    public void RoundedDecorator_WrapsRealStrategy()
    {
        var strategy = new RoundedFinalGradeStrategy(new WeightedGradedOnlyStrategy());

        var grade = _calculator.Calculate(GradedStudent(), _rubric, strategy);

        Assert.Equal(9m, grade.Value);
    }

    private sealed class ConstantStrategy : IFinalGradeStrategy
    {
        private readonly decimal _value;

        public ConstantStrategy(decimal value)
        {
            _value = value;
        }

        public string Title => "константа";

        public decimal Calculate(IReadOnlyList<GradeComponent> components) => _value;
    }
}
