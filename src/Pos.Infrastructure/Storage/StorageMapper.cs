using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Storage;

/// <summary>
/// Маппинг DTO ↔ домен. Восстановление идёт через обычные конструкторы и фабрику оценок,
/// поэтому испорченный файл (балл 12 из 10, оценка по несуществующему критерию) не загрузится.
/// </summary>
internal static class StorageMapper
{
    public static Rubric ToRubric(IEnumerable<CriterionDto> criteria)
    {
        var rubric = new Rubric();
        foreach (var dto in criteria)
        {
            rubric.Add(new Criterion(
                new CriterionId(dto.Id),
                new CriterionName(dto.Name),
                new MaxScore(dto.MaxScore),
                new Weight(dto.Weight)));
        }

        return rubric;
    }

    public static Student ToStudent(StudentDto dto, Rubric rubric, EvaluationFactory factory)
    {
        var student = new Student(new StudentId(dto.Id), new PersonName(dto.FullName));
        foreach (var evaluation in dto.Evaluations)
        {
            student.Grade(factory.Create(
                rubric,
                new CriterionId(evaluation.CriterionId),
                evaluation.Score,
                evaluation.Date,
                evaluation.Comment));
        }

        return student;
    }

    public static StorageDto ToDto(Rubric rubric, IEnumerable<Student> students) => new()
    {
        Criteria = rubric.Criteria.Select(c => new CriterionDto
        {
            Id = c.Id.Value,
            Name = c.Name.Value,
            MaxScore = c.MaxScore.Value,
            Weight = c.Weight.Value,
        }).ToList(),

        Students = students.Select(s => new StudentDto
        {
            Id = s.Id.Value,
            FullName = s.Name.Value,
            Evaluations = s.Evaluations.Select(e => new EvaluationDto
            {
                CriterionId = e.CriterionId.Value,
                Score = e.Score.Value,
                Date = e.Date,
                Comment = e.Comment,
            }).ToList(),
        }).ToList(),
    };
}
