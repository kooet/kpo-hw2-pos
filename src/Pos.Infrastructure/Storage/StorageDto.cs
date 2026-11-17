namespace Pos.Infrastructure.Storage;

// Формат JSON-файла. Это просто данные без правил: правила проверяет домен при загрузке.

internal sealed class StorageDto
{
    public List<CriterionDto> Criteria { get; set; } = new();

    public List<StudentDto> Students { get; set; } = new();
}

internal sealed class CriterionDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal MaxScore { get; set; }

    public decimal Weight { get; set; }
}

internal sealed class StudentDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public List<EvaluationDto> Evaluations { get; set; } = new();
}

internal sealed class EvaluationDto
{
    public Guid CriterionId { get; set; }

    public decimal Score { get; set; }

    public DateOnly Date { get; set; }

    public string? Comment { get; set; }
}
