using Pos.Domain.Common;

namespace Pos.Domain.Students;

/// <summary>ФИО студента. Value object: два одинаковых ФИО — одно и то же значение.</summary>
public sealed record PersonName
{
    public const int MaxLength = 100;

    public PersonName(string value)
    {
        var normalized = Text.Collapse(value);
        if (normalized.Length == 0)
        {
            throw new DomainException("ФИО студента не может быть пустым.");
        }

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"ФИО студента не может быть длиннее {MaxLength} символов.");
        }

        Value = normalized;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
