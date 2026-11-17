namespace Pos.Domain.Grading;

internal static class WeightedAverage
{
    public static decimal Of(IEnumerable<GradeComponent> components)
    {
        decimal weightSum = 0;
        decimal weightedShares = 0;

        foreach (var component in components)
        {
            weightSum += component.Weight;
            weightedShares += component.Weight * component.Share;
        }

        return weightSum == 0 ? 0 : FinalGrade.MaxValue * weightedShares / weightSum;
    }
}
