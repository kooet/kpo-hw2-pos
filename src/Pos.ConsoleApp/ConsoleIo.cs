namespace Pos.ConsoleApp;

/// <summary>
/// Ввод-вывод консоли. Читает из любого TextReader: клавиатуры, файла сценария или строки в тесте.
/// </summary>
public sealed class ConsoleIo
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly bool _echoInput;

    /// <param name="echoInput">Печатать прочитанные строки — нужно при вводе из файла, чтобы вывод читался как диалог.</param>
    public ConsoleIo(TextReader input, TextWriter output, bool echoInput = false)
    {
        _input = input;
        _output = output;
        _echoInput = echoInput;
    }

    public void Write(string text) => _output.Write(text);

    public void WriteLine(string text = "") => _output.WriteLine(text);

    /// <summary>Следующая строка ввода. Строки сценария, начинающиеся с «#», — комментарии.</summary>
    /// <exception cref="EndOfInputException">Ввод закончился.</exception>
    public string ReadLine()
    {
        while (true)
        {
            var line = _input.ReadLine() ?? throw new EndOfInputException();
            if (line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (_echoInput)
            {
                _output.WriteLine(line);
            }

            return line.Trim();
        }
    }
}

public sealed class EndOfInputException : Exception
{
    public EndOfInputException()
        : base("Ввод закончился.")
    {
    }
}
