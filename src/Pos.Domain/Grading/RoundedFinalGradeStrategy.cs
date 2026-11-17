namespace Pos.Domain.Grading;

/// <summary>
/// Decorator: округляет итог любого правила до целого (0.5 — вверх, как в ведомостях ВШЭ).
/// Округление не дублируется в каждой стратегии и не размазывается по интерфейсу.
/// </summary>
public sealed class RoundedFinalGradeStrategy : IFinalGradeStrategy
{
    private readonly IFinalGradeStrategy _inner;

    public RoundedFinalGradeStrategy(IFinalGradeStrategy inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public string Title => $"{_inner.Title}, с округлением до целого";

    public decimal Calculate(IReadOnlyList<GradeComponent> components) =>
        Math.Round(_inner.Calculate(components), 0, MidpointRounding.AwayFromZero);
}
