using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Рубрикатор курса — корень агрегата, в который входят критерии.
/// Все изменения критериев идут через него, поэтому он следит за уникальностью названий.
/// </summary>
public sealed class Rubric
{
    private readonly List<Criterion> _criteria = new();

    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();

    public Criterion AddCriterion(CriterionName name, MaxScore maxScore, Weight weight) =>
        Add(new Criterion(CriterionId.New(), name, maxScore, weight));

    public Criterion Add(Criterion criterion)
    {
        ArgumentNullException.ThrowIfNull(criterion);

        if (Contains(criterion.Id))
        {
            throw new DomainException("Этот критерий уже есть в рубрикаторе.");
        }

        if (_criteria.Any(c => c.Name.SameAs(criterion.Name)))
        {
            throw new DomainException($"Критерий «{criterion.Name}» уже есть в рубрикаторе.");
        }

        _criteria.Add(criterion);
        return criterion;
    }

    public bool Contains(CriterionId id) => Find(id) is not null;

    public Criterion? Find(CriterionId id) => _criteria.Find(c => c.Id == id);

    public Criterion Get(CriterionId id) =>
        Find(id) ?? throw new DomainException("Такого критерия нет в рубрикаторе.");

    public void ChangeWeight(CriterionId id, Weight weight)
    {
        ArgumentNullException.ThrowIfNull(weight);
        Get(id).ChangeWeight(weight);
    }

    public void Remove(CriterionId id) => _criteria.Remove(Get(id));
}
