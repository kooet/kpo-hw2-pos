using Pos.Domain.Students;

namespace Pos.Application.Models;

public sealed record StudentView(StudentId Id, string FullName, int EvaluationCount)
{
    public static StudentView From(Student student) =>
        new(student.Id, student.Name.Value, student.Evaluations.Count);
}
