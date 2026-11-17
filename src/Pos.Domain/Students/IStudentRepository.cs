namespace Pos.Domain.Students;

/// <summary>Порт хранения студентов. Реализация (память, JSON-файл) — снаружи домена.</summary>
public interface IStudentRepository
{
    void Add(Student student);

    Student? Find(StudentId id);

    IReadOnlyList<Student> GetAll();

    void Update(Student student);

    void Remove(StudentId id);
}
