using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Оценка — факт «по этому критерию поставили такой балл». Value object: неизменяема,
/// исправление оценки — это новая оценка вместо старой.
/// Конструктор закрыт: создать оценку можно только через <see cref="EvaluationFactory"/>,
/// которая сверяет её с рубрикатором.
/// </summary>
public sealed record Evaluation
{
    public const int MaxCommentLength = 200;

    internal Evaluation(CriterionId criterionId, Score score, DateOnly date, string? comment)
    {
        CriterionId = criterionId;
        Score = score;
        Date = date;
        Comment = comment;
    }

    public CriterionId CriterionId { get; }

    public Score Score { get; }

    public DateOnly Date { get; }

    public string? Comment { get; }
}
