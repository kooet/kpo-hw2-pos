using Pos.Domain.Students;

namespace Pos.Application.Common;

internal static class StudentRepositoryExtensions
{
    public static Student GetRequired(this IStudentRepository students, StudentId id) =>
        students.Find(id) ?? throw new UseCaseException("Студент не найден.");
}
