using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Xunit;

namespace Pos.Tests;

public class RubricTests
{
    [Fact]
    public void AddCriterion_DuplicateNameInAnyCase_Throws()
    {
        var rubric = TestData.CreateRubric(("Контрольная", 100, 0.5m));

        var error = Assert.Throws<DomainException>(() =>
            rubric.AddCriterion(new CriterionName("КОНТРОЛЬНАЯ"), new MaxScore(10), new Weight(1)));

        Assert.Contains("уже есть", error.Message);
        Assert.Single(rubric.Criteria);
    }

    [Fact]
    public void ChangeWeight_KeepsSameCriterion()
    {
        var rubric = TestData.CreateRubric(("ДЗ", 10, 0.3m));
        var criterion = rubric.GetCriterion("ДЗ");

        rubric.ChangeWeight(criterion.Id, new Weight(0.5m));

        Assert.Same(criterion, rubric.Get(criterion.Id));
        Assert.Equal(0.5m, criterion.Weight.Value);
    }

    [Fact]
    public void Get_UnknownCriterion_Throws()
    {
        var rubric = TestData.CreateRubric(("ДЗ", 10, 1));

        Assert.Throws<DomainException>(() => rubric.Get(CriterionId.New()));
        Assert.Throws<DomainException>(() => rubric.Remove(CriterionId.New()));
    }
}
