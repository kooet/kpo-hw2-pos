using Microsoft.Extensions.DependencyInjection;
using Pos.Application.UseCases;
using Pos.ConsoleApp;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Storage;
using Xunit;

namespace Pos.Tests;

public class ConsoleTests
{
    private static string Run(params string[] lines)
    {
        var output = new StringWriter();
        var io = new ConsoleIo(new StringReader(string.Join(Environment.NewLine, lines)), output);

        using var provider = CompositionRoot.Build(io, dataFile: null);
        provider.GetRequiredService<PosConsole>().Run();

        return output.ToString();
    }

    [Fact]
    public void EndToEnd_StudentCriterionGradeFinal()
    {
        var output = Run(
            "2", "Иванова Анна",
            "5", "ДЗ", "10", "1",
            "8", "1", "1", "7,5", "",
            "12", "1",
            "0");

        Assert.Contains("Добавлен студент: Иванова Анна.", output);
        Assert.Contains("Добавлен критерий «ДЗ»", output);
        Assert.Contains("Иванова Анна: «ДЗ» — 7.5 из 10.", output);
        Assert.Contains("Итог: Иванова Анна — 7.5 из 10.", output);
        Assert.Contains("Работа завершена.", output);
    }

    [Fact]
    public void DomainError_IsShownAndProgramContinues()
    {
        var output = Run(
            "2", "Иванова Анна",
            "5", "ДЗ", "10", "1",
            "8", "1", "1", "12", "",
            "11", "1",
            "0");

        Assert.Contains("Нельзя: Балл 12 выходит за пределы критерия «ДЗ»: максимум 10.", output);
        Assert.Contains("У студента Иванова Анна пока нет оценок.", output);
    }

    [Fact]
    public void WrongInput_IsAskedAgain()
    {
        var output = Run(
            "99",
            "5", "ДЗ", "десять", "10", "1",
            "0");

        Assert.Contains("Нет такого пункта меню.", output);
        Assert.Contains("Введите число, например 7 или 7.5.", output);
        Assert.Contains("Добавлен критерий «ДЗ»", output);
    }

    [Fact]
    public void ChoosingWithoutStudents_ShowsHint()
    {
        var output = Run("8", "0");

        Assert.Contains("студентов пока нет", output);
    }

    [Fact]
    public void EndOfInput_FinishesQuietly()
    {
        var output = Run("1");

        Assert.Contains("Ввод закончился", output);
    }

    [Fact]
    public void CompositionRoot_UsesJsonStorageWhenFileGiven()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pos-{Guid.NewGuid():N}.json");
        try
        {
            using var provider = CompositionRoot.Build(new ConsoleIo(TextReader.Null, TextWriter.Null), path);

            Assert.IsType<JsonFileStorage>(provider.GetRequiredService<IStudentRepository>());
            Assert.Same(
                provider.GetRequiredService<IStudentRepository>(),
                provider.GetRequiredService<IRubricRepository>());
            Assert.Equal(2, provider.GetRequiredService<GradingRules>().Available.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LaunchOptions_ParsesDataFile()
    {
        var options = LaunchOptions.Parse(new[] { "--data", "pos.json" });

        Assert.Null(options.ScenarioFile);
        Assert.Equal("pos.json", options.DataFile);
    }

    [Theory]
    [InlineData("--data")]
    [InlineData("--unknown")]
    [InlineData("no-such-scenario.txt")]
    public void LaunchOptions_BadArguments_Throw(string argument)
    {
        Assert.Throws<ArgumentException>(() => LaunchOptions.Parse(new[] { argument }));
    }
}
