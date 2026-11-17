using Pos.Application.Common;
using Pos.Domain.Grading;

namespace Pos.Application.UseCases;

/// <summary>
/// Какое правило итога сейчас действует. Правила приходят из DI-контейнера,
/// округление накладывается поверх выбранного правила декоратором.
/// </summary>
public sealed class GradingRules
{
    private readonly IReadOnlyList<IFinalGradeStrategy> _strategies;
    private int _selected;

    public GradingRules(IEnumerable<IFinalGradeStrategy> strategies)
    {
        _strategies = strategies.ToList();
        if (_strategies.Count == 0)
        {
            throw new ArgumentException("Нужно хотя бы одно правило расчёта итога.", nameof(strategies));
        }
    }

    public IReadOnlyList<string> Available => _strategies.Select(s => s.Title).ToList();

    public bool RoundToInteger { get; private set; }

    public IFinalGradeStrategy Current =>
        RoundToInteger
            ? new RoundedFinalGradeStrategy(_strategies[_selected])
            : _strategies[_selected];

    public void Select(int index, bool roundToInteger)
    {
        if (index < 0 || index >= _strategies.Count)
        {
            throw new UseCaseException("Нет такого правила расчёта итога.");
        }

        _selected = index;
        RoundToInteger = roundToInteger;
    }
}
