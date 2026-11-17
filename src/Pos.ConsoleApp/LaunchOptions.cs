namespace Pos.ConsoleApp;

/// <param name="ScenarioFile">Файл, из которого читаются команды вместо клавиатуры.</param>
/// <param name="DataFile">JSON-файл для хранения данных; если не задан — данные хранятся в памяти.</param>
public sealed record LaunchOptions(string? ScenarioFile, string? DataFile)
{
    public const string Usage = "Запуск: dotnet run --project src/Pos.ConsoleApp -- [файл-сценария] [--data файл.json]";

    public static LaunchOptions Parse(IReadOnlyList<string> args)
    {
        string? scenario = null;
        string? data = null;

        for (var i = 0; i < args.Count; i++)
        {
            if (args[i] == "--data")
            {
                if (i + 1 >= args.Count)
                {
                    throw new ArgumentException("После --data нужно указать путь к файлу.");
                }

                data = args[++i];
            }
            else if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Неизвестный параметр: {args[i]}");
            }
            else if (scenario is null)
            {
                scenario = args[i];
            }
            else
            {
                throw new ArgumentException("Можно указать только один файл сценария.");
            }
        }

        if (scenario is not null && !File.Exists(scenario))
        {
            throw new ArgumentException($"Файл сценария не найден: {scenario}");
        }

        return new LaunchOptions(scenario, data);
    }
}
