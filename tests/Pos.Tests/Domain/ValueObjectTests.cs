using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Xunit;

namespace Pos.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void PersonName_Empty_Throws(string value)
    {
        Assert.Throws<DomainException>(() => new PersonName(value));
    }

    [Fact]
    public void PersonName_CollapsesSpaces_AndComparesByValue()
    {
        var name = new PersonName("  Иванова   Анна ");

        Assert.Equal("Иванова Анна", name.Value);
        Assert.Equal(new PersonName("Иванова Анна"), name);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.001)]
    public void Score_NegativeOrTooPrecise_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new Score((decimal)value));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.5)]
    [InlineData(101.0)]
    public void Weight_OutOfRange_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new Weight((decimal)value));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1001.0)]
    public void MaxScore_OutOfRange_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new MaxScore((decimal)value));
    }

    [Fact]
    public void CriterionName_SameAs_IgnoresCase()
    {
        Assert.True(new CriterionName("Контрольная").SameAs(new CriterionName("контрольная")));
    }

    [Fact]
    public void FinalGrade_AboveTen_Throws()
    {
        Assert.Throws<DomainException>(() => new FinalGrade(10.01m, 1, 1, "правило"));
    }
}
