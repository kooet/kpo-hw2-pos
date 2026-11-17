using Pos.Application.Models;

namespace Pos.Application.Ports;

/// <summary>Порт выгрузки ведомости итогов. Формат (CSV и т. п.) знает только адаптер.</summary>
public interface IFinalReportExporter
{
    void Export(IReadOnlyList<FinalGradeView> rows, string destination);
}
