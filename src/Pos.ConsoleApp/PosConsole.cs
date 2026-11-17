using System.Globalization;
using Pos.Application.Common;
using Pos.Application.Models;
using Pos.Application.UseCases;
using Pos.Domain.Common;

namespace Pos.ConsoleApp;

/// <summary>
/// Консольное меню — входящий адаптер. Только спрашивает и печатает,
/// все решения принимают сценарии из слоя Application и домен.
/// </summary>
public sealed class PosConsole
{
    private const string DefaultReportFile = "pos-report.csv";

    private readonly ConsoleIo _io;
    private readonly StudentService _students;
    private readonly RubricService _rubric;
    private readonly EvaluationService _evaluations;
    private readonly FinalGradeService _finalGrades;
    private readonly GradingRules _rules;
    private readonly IReadOnlyList<MenuItem> _menu;

    public PosConsole(
        ConsoleIo io,
        StudentService students,
        RubricService rubric,
        EvaluationService evaluations,
        FinalGradeService finalGrades,
        GradingRules rules)
    {
        _io = io;
        _students = students;
        _rubric = rubric;
        _evaluations = evaluations;
        _finalGrades = finalGrades;
        _rules = rules;

        _menu = new MenuItem[]
        {
            new("Студенты", "1", "Список студентов", ShowStudents),
            new("Студенты", "2", "Добавить студента", AddStudent),
            new("Студенты", "3", "Удалить студента", RemoveStudent),
            new("Рубрикатор", "4", "Список критериев", ShowCriteria),
            new("Рубрикатор", "5", "Добавить критерий", AddCriterion),
            new("Рубрикатор", "6", "Изменить вес критерия", ChangeWeight),
            new("Рубрикатор", "7", "Удалить критерий", RemoveCriterion),
            new("Оценки", "8", "Выставить оценку", Grade),
            new("Оценки", "9", "Исправить оценку", Correct),
            new("Оценки", "10", "Отменить оценку", Cancel),
            new("Оценки", "11", "Оценки студента", ShowEvaluations),
            new("Итог", "12", "Итог студента", ShowFinalGrade),
            new("Итог", "13", "Ведомость итогов", ShowStatement),
            new("Итог", "14", "Правило расчёта итога", ChooseRule),
            new("Итог", "15", "Экспорт ведомости в CSV", Export),
        };
    }

    public void Run()
    {
        _io.WriteLine("ПОС — преподавательская оценка студентов");

        try
        {
            while (true)
            {
                PrintMenu();
                var choice = _io.ReadLine();
                if (choice == "0")
                {
                    _io.WriteLine("Работа завершена.");
                    return;
                }

                var item = _menu.FirstOrDefault(m => m.Key == choice);
                if (item is null)
                {
                    _io.WriteLine("Нет такого пункта меню.");
                    continue;
                }

                Execute(item);
            }
        }
        catch (EndOfInputException)
        {
            _io.WriteLine();
            _io.WriteLine("Ввод закончился, работа завершена.");
        }
    }

    private void PrintMenu()
    {
        _io.WriteLine();
        string? section = null;
        foreach (var item in _menu)
        {
            if (item.Section != section)
            {
                section = item.Section;
                _io.WriteLine(section);
            }

            _io.WriteLine($"  {item.Key,2}. {item.Title}");
        }

        _io.WriteLine("   0. Выход");
        _io.Write("Выберите пункт: ");
    }

    private void Execute(MenuItem item)
    {
        _io.WriteLine($"— {item.Title} —");
        try
        {
            item.Run();
        }
        catch (Exception ex) when (ex is DomainException or UseCaseException)
        {
            _io.WriteLine($"Нельзя: {ex.Message}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _io.WriteLine($"Не удалось записать файл: {ex.Message}");
        }
    }

    private void ShowStudents()
    {
        var students = _students.List();
        if (students.Count == 0)
        {
            _io.WriteLine("Студентов пока нет.");
            return;
        }

        for (var i = 0; i < students.Count; i++)
        {
            _io.WriteLine($"  {i + 1}. {students[i].FullName} (оценок: {students[i].EvaluationCount})");
        }
    }

    private void AddStudent()
    {
        var student = _students.Enroll(Ask("ФИО: "));
        _io.WriteLine($"Добавлен студент: {student.FullName}.");
    }

    private void RemoveStudent()
    {
        var student = ChooseStudent();
        _students.Remove(student.Id);
        _io.WriteLine($"Студент {student.FullName} удалён вместе с его оценками.");
    }

    private void ShowCriteria()
    {
        var criteria = _rubric.List();
        if (criteria.Count == 0)
        {
            _io.WriteLine("В рубрикаторе пока нет критериев.");
            return;
        }

        var totalWeight = criteria.Sum(c => c.Weight);
        for (var i = 0; i < criteria.Count; i++)
        {
            var c = criteria[i];
            _io.WriteLine(
                $"  {i + 1}. {c.Name}: максимум {Format(c.MaxScore)}, вес {Format(c.Weight)} ({Format(c.Weight / totalWeight * 100)}% итога)");
        }
    }

    private void AddCriterion()
    {
        var name = Ask("Название: ");
        var maxScore = AskNumber("Максимальный балл: ");
        var weight = AskNumber("Вес в итоге (например, 0.3): ");

        var criterion = _rubric.AddCriterion(name, maxScore, weight);
        _io.WriteLine($"Добавлен критерий «{criterion.Name}»: максимум {Format(criterion.MaxScore)}, вес {Format(criterion.Weight)}.");
    }

    private void ChangeWeight()
    {
        var criterion = ChooseCriterion();
        var weight = AskNumber($"Новый вес (сейчас {Format(criterion.Weight)}): ");
        _rubric.ChangeWeight(criterion.Id, weight);
        _io.WriteLine($"Вес критерия «{criterion.Name}» изменён.");
    }

    private void RemoveCriterion()
    {
        var criterion = ChooseCriterion();
        _rubric.Remove(criterion.Id);
        _io.WriteLine($"Критерий «{criterion.Name}» удалён.");
    }

    private void Grade()
    {
        var (student, criterion, points, comment) = AskEvaluation();
        _evaluations.Grade(student.Id, criterion.Id, points, comment);
        _io.WriteLine($"{student.FullName}: «{criterion.Name}» — {Format(points)} из {Format(criterion.MaxScore)}.");
    }

    private void Correct()
    {
        var (student, criterion, points, comment) = AskEvaluation();
        _evaluations.Correct(student.Id, criterion.Id, points, comment);
        _io.WriteLine($"Оценка исправлена. {student.FullName}: «{criterion.Name}» — {Format(points)} из {Format(criterion.MaxScore)}.");
    }

    private void Cancel()
    {
        var student = ChooseStudent();
        var criterion = ChooseCriterion();
        _evaluations.Cancel(student.Id, criterion.Id);
        _io.WriteLine($"Оценка студента {student.FullName} по критерию «{criterion.Name}» отменена.");
    }

    private void ShowEvaluations()
    {
        var student = ChooseStudent();
        var evaluations = _evaluations.ListFor(student.Id);
        if (evaluations.Count == 0)
        {
            _io.WriteLine($"У студента {student.FullName} пока нет оценок.");
            return;
        }

        _io.WriteLine($"Оценки студента {student.FullName}:");
        foreach (var e in evaluations)
        {
            var comment = e.Comment is null ? string.Empty : $" — {e.Comment}";
            _io.WriteLine($"  {e.Criterion}: {Format(e.Score)} из {Format(e.MaxScore)}, {e.Date:dd.MM.yyyy}{comment}");
        }
    }

    private void ShowFinalGrade()
    {
        var student = ChooseStudent();
        var grade = _finalGrades.CalculateFor(student.Id);

        _io.WriteLine($"Итог: {student.FullName} — {Format(grade.Value)} из 10.");
        _io.WriteLine($"Оценено критериев: {grade.GradedCriteria} из {grade.TotalCriteria}. Правило: {grade.Rule}.");
    }

    private void ShowStatement()
    {
        var rows = _finalGrades.Statement();
        if (rows.Count == 0)
        {
            _io.WriteLine("Студентов пока нет.");
            return;
        }

        _io.WriteLine($"Правило: {rows[0].Rule}.");
        foreach (var row in rows)
        {
            _io.WriteLine($"  {row.Student}: {Format(row.Value)} (оценено {row.GradedCriteria} из {row.TotalCriteria})");
        }
    }

    private void ChooseRule()
    {
        var rules = _rules.Available;
        for (var i = 0; i < rules.Count; i++)
        {
            _io.WriteLine($"  {i + 1}. {rules[i]}");
        }

        var index = AskIndex("Правило: ", rules.Count);
        var round = AskYesNo("Округлять итог до целого? (д/н): ");

        _rules.Select(index, round);
        _io.WriteLine($"Теперь итог считается так: {_rules.Current.Title}.");
    }

    private void Export()
    {
        var destination = Ask($"Файл (Enter — {DefaultReportFile}): ");
        if (destination.Length == 0)
        {
            destination = DefaultReportFile;
        }

        var count = _finalGrades.Export(destination);
        _io.WriteLine($"Ведомость выгружена: {Path.GetFullPath(destination)} (студентов: {count}).");
    }

    private (StudentView Student, CriterionView Criterion, decimal Points, string Comment) AskEvaluation()
    {
        var student = ChooseStudent();
        var criterion = ChooseCriterion();
        var points = AskNumber($"Балл (максимум {Format(criterion.MaxScore)}): ");
        var comment = Ask("Комментарий (Enter — без комментария): ");
        return (student, criterion, points, comment);
    }

    private StudentView ChooseStudent()
    {
        var students = _students.List();
        if (students.Count == 0)
        {
            throw new UseCaseException("студентов пока нет, сначала добавьте студента.");
        }

        for (var i = 0; i < students.Count; i++)
        {
            _io.WriteLine($"  {i + 1}. {students[i].FullName}");
        }

        return students[AskIndex("Студент: ", students.Count)];
    }

    private CriterionView ChooseCriterion()
    {
        var criteria = _rubric.List();
        if (criteria.Count == 0)
        {
            throw new UseCaseException("в рубрикаторе нет критериев, сначала добавьте критерий.");
        }

        for (var i = 0; i < criteria.Count; i++)
        {
            _io.WriteLine($"  {i + 1}. {criteria[i].Name} (максимум {Format(criteria[i].MaxScore)})");
        }

        return criteria[AskIndex("Критерий: ", criteria.Count)];
    }

    private string Ask(string prompt)
    {
        _io.Write(prompt);
        return _io.ReadLine();
    }

    /// <summary>Номер из списка; возвращает индекс с нуля.</summary>
    private int AskIndex(string prompt, int count)
    {
        while (true)
        {
            if (int.TryParse(Ask(prompt), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                && number >= 1 && number <= count)
            {
                return number - 1;
            }

            _io.WriteLine($"Введите номер от 1 до {count}.");
        }
    }

    /// <summary>
    /// Число с точкой или запятой. Диапазон здесь не проверяется — это делает домен,
    /// чтобы правило «балл в пределах критерия» жило в одном месте.
    /// </summary>
    private decimal AskNumber(string prompt)
    {
        while (true)
        {
            var text = Ask(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            _io.WriteLine("Введите число, например 7 или 7.5.");
        }
    }

    private bool AskYesNo(string prompt)
    {
        while (true)
        {
            switch (Ask(prompt).ToLowerInvariant())
            {
                case "д" or "да" or "y" or "yes":
                    return true;
                case "н" or "нет" or "n" or "no":
                    return false;
                default:
                    _io.WriteLine("Ответьте «д» или «н».");
                    break;
            }
        }
    }

    private static string Format(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero).ToString("0.##", CultureInfo.InvariantCulture);

    private sealed record MenuItem(string Section, string Key, string Title, Action Run);
}
