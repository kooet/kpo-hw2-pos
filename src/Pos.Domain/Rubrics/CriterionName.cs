using Pos.Domain.Common;

namespace Pos.Domain.Rubrics;

public sealed record CriterionName
{
    public const int MaxLength = 60;

    public CriterionName(string value)
    {
        var normalized = Text.Collapse(value);
        if (normalized.Length == 0)
        {
            throw new DomainException("Название критерия не может быть пустым.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Название критерия не может быть длиннее {MaxLength} символов.");
        }

        Value = normalized;
    }

    public string Value { get; }

    /// <summary>Совпадение без учёта регистра: «Контрольная» и «контрольная» — один критерий.</summary>
    public bool SameAs(CriterionName other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;
}
