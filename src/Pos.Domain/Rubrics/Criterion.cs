using Pos.Domain.Common;
using Pos.Domain.Grading;

namespace Pos.Domain.Rubrics;

/// <summary>
/// Критерий — entity внутри агрегата <see cref="Rubric"/>: у него есть идентичность,
/// и при смене веса это всё тот же критерий. Максимальный балл после создания не меняется,
/// иначе уже выставленные оценки могли бы оказаться за пределами критерия.
/// </summary>
public sealed class Criterion
{
    public Criterion(CriterionId id, CriterionName name, MaxScore maxScore, Weight weight)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MaxScore = maxScore ?? throw new ArgumentNullException(nameof(maxScore));
        Weight = weight ?? throw new ArgumentNullException(nameof(weight));
    }

    public CriterionId Id { get; }

    public CriterionName Name { get; }

    public MaxScore MaxScore { get; }

    public Weight Weight { get; private set; }

    public void EnsureAccepts(Score score)
    {
        ArgumentNullException.ThrowIfNull(score);

        if (score.Value > MaxScore.Value)
        {
            throw new DomainException(
                $"Балл {score} выходит за пределы критерия «{Name}»: максимум {MaxScore}.");
        }
    }

    internal void ChangeWeight(Weight weight) => Weight = weight;
}
