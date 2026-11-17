using System.Globalization;

namespace Pos.Domain.Common;

internal static class Text
{
    /// <summary>Убирает лишние пробелы: «  Иванова   Анна » → «Иванова Анна».</summary>
    public static string Collapse(string? value) =>
        value is null
            ? string.Empty
            : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string Number(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
