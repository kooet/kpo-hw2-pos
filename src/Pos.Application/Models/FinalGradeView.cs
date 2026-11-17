using Pos.Domain.Grading;
using Pos.Domain.Students;

namespace Pos.Application.Models;

public sealed record FinalGradeView(string Student, decimal Value, int GradedCriteria, int TotalCriteria, string Rule)
{
    public static FinalGradeView From(Student student, FinalGrade grade) =>
        new(student.Name.Value, grade.Value, grade.GradedCriteria, grade.TotalCriteria, grade.Rule);
}
