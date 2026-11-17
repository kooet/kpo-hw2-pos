using Pos.Domain.Rubrics;

namespace Pos.Application.Models;

public sealed record CriterionView(CriterionId Id, string Name, decimal MaxScore, decimal Weight)
{
    public static CriterionView From(Criterion criterion) =>
        new(criterion.Id, criterion.Name.Value, criterion.MaxScore.Value, criterion.Weight.Value);
}
