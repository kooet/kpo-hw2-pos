using Pos.Domain.Grading;
using Pos.Domain.Rubrics;

namespace Pos.Application.Models;

public sealed record EvaluationView(string Criterion, decimal Score, decimal MaxScore, DateOnly Date, string? Comment)
{
    public static EvaluationView From(Criterion criterion, Evaluation evaluation) =>
        new(criterion.Name.Value, evaluation.Score.Value, criterion.MaxScore.Value, evaluation.Date, evaluation.Comment);
}
