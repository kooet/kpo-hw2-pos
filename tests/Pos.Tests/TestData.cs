using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests;

internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 11, 11);

    public static Rubric CreateRubric(params (string Name, decimal Max, decimal Weight)[] criteria)
    {
        var rubric = new Rubric();
        foreach (var (name, max, weight) in criteria)
        {
            rubric.AddCriterion(new CriterionName(name), new MaxScore(max), new Weight(weight));
        }

        return rubric;
    }

    public static Criterion GetCriterion(this Rubric rubric, string name) =>
        rubric.Criteria.Single(c => c.Name.Value == name);

    public static Student NewStudent(string name = "Иванова Анна") => Student.Enroll(new PersonName(name));

    public static Evaluation MakeEvaluation(Rubric rubric, string criterion, decimal points) =>
        new EvaluationFactory().Create(rubric, rubric.GetCriterion(criterion).Id, points, Today);
}

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now)
    {
        _now = now;
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
