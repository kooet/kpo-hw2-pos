using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Domain.Students;

namespace Pos.Application.UseCases;

public sealed class StudentService
{
    private readonly IStudentRepository _students;

    public StudentService(IStudentRepository students)
    {
        _students = students;
    }

    public StudentView Enroll(string fullName)
    {
        var student = Student.Enroll(new PersonName(fullName));
        _students.Add(student);
        return StudentView.From(student);
    }

    public IReadOnlyList<StudentView> List() =>
        _students.GetAll().Select(StudentView.From).ToList();

    public void Remove(StudentId id)
    {
        _students.GetRequired(id);
        _students.Remove(id);
    }
}
