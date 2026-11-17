using System.Globalization;
using System.Text;
using Pos.Application.Models;
using Pos.Application.Ports;

namespace Pos.Infrastructure.Export;

/// <summary>
/// Выгрузка ведомости в CSV, который нормально открывается в русском Excel:
/// разделитель «;», дробная часть через запятую, UTF-8 с BOM.
/// </summary>
public sealed class CsvFinalReportExporter : IFinalReportExporter
{
    private const char Separator = ';';

    public void Export(IReadOnlyList<FinalGradeView> rows, string destination)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var directory = Path.GetDirectoryName(Path.GetFullPath(destination));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(destination, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        writer.WriteLine(string.Join(Separator, "Студент", "Итог", "Оценено критериев", "Всего критериев", "Правило"));
        foreach (var row in rows)
        {
            writer.WriteLine(string.Join(
                Separator,
                Escape(row.Student),
                FormatNumber(row.Value),
                row.GradedCriteria.ToString(CultureInfo.InvariantCulture),
                row.TotalCriteria.ToString(CultureInfo.InvariantCulture),
                Escape(row.Rule)));
        }
    }

    private static string FormatNumber(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero)
            .ToString("0.##", CultureInfo.InvariantCulture)
            .Replace('.', ',');

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { Separator, '"', '\n', '\r' }) < 0)
        {
            return value;
        }

        return '"' + value.Replace("\"", "\"\"") + '"';
    }
}
