using Pos.Domain.Common;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

/// <summary>
/// Единственное место, где создаётся <see cref="Evaluation"/>.
/// Проверяет оба правила, для которых нужен рубрикатор: критерий существует и балл в его пределах.
/// Ни студент, ни рубрикатор поодиночке это проверить не могут — это разные агрегаты.
/// </summary>
public sealed class EvaluationFactory
{
    public Evaluation Create(Rubric rubric, CriterionId criterionId, decimal points, DateOnly date, string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        var criterion = rubric.Find(criterionId)
            ?? throw new DomainException("Оценку можно выставить только по критерию из рубрикатора.");

        var score = new Score(points);
        criterion.EnsureAccepts(score);

        return new Evaluation(criterion.Id, score, date, NormalizeComment(comment));
    }

    private static string? NormalizeComment(string? comment)
    {
        var text = comment?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (text.Length > Evaluation.MaxCommentLength)
        {
            throw new DomainException($"Комментарий не может быть длиннее {Evaluation.MaxCommentLength} символов.");
        }

        return text;
    }
}
