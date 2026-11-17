using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Storage;

/// <summary>Хранилище в памяти: данные живут, пока запущена программа.</summary>
public sealed class InMemoryStorage : IStudentRepository, IRubricRepository
{
    private readonly List<Student> _students = new();
    private Rubric _rubric = new();

    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (Find(student.Id) is not null)
        {
            throw new InvalidOperationException("Студент с таким идентификатором уже сохранён.");
        }

        _students.Add(student);
    }

    public Student? Find(StudentId id) => _students.Find(s => s.Id == id);

    public IReadOnlyList<Student> GetAll() => _students.AsReadOnly();

    public void Update(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        var index = _students.FindIndex(s => s.Id == student.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Нельзя обновить студента, которого нет в хранилище.");
        }

        _students[index] = student;
    }

    public void Remove(StudentId id) => _students.RemoveAll(s => s.Id == id);

    public Rubric Get() => _rubric;

    public void Save(Rubric rubric) => _rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
}
