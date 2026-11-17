using System.Text.Encodings.Web;
using System.Text.Json;
using Pos.Domain.Common;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Infrastructure.Storage;

/// <summary>
/// Хранилище в JSON-файле: рубрикатор и студенты лежат в одном файле
/// и перезаписываются после каждого изменения.
/// </summary>
public sealed class JsonFileStorage : IStudentRepository, IRubricRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly string _path;
    private readonly List<Student> _students = new();
    private Rubric _rubric = new();

    public JsonFileStorage(string path, EvaluationFactory factory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Не указан путь к файлу данных.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(factory);

        _path = path;
        if (File.Exists(path))
        {
            Load(factory);
        }
    }

    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        if (Find(student.Id) is not null)
        {
            throw new InvalidOperationException("Студент с таким идентификатором уже сохранён.");
        }

        _students.Add(student);
        Flush();
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
        Flush();
    }

    public void Remove(StudentId id)
    {
        _students.RemoveAll(s => s.Id == id);
        Flush();
    }

    public Rubric Get() => _rubric;

    public void Save(Rubric rubric)
    {
        _rubric = rubric ?? throw new ArgumentNullException(nameof(rubric));
        Flush();
    }

    private void Load(EvaluationFactory factory)
    {
        try
        {
            var data = JsonSerializer.Deserialize<StorageDto>(File.ReadAllText(_path), Options) ?? new StorageDto();
            _rubric = StorageMapper.ToRubric(data.Criteria);
            _students.AddRange(data.Students.Select(s => StorageMapper.ToStudent(s, _rubric, factory)));
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Файл данных {_path} повреждён: это не JSON нужного формата.", ex);
        }
        catch (DomainException ex)
        {
            throw new InvalidDataException($"Файл данных {_path} нарушает правила модели: {ex.Message}", ex);
        }
    }

    private void Flush()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_path, JsonSerializer.Serialize(StorageMapper.ToDto(_rubric, _students), Options));
    }
}
