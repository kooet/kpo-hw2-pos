using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.ConsoleApp;

Console.OutputEncoding = Encoding.UTF8;

LaunchOptions options;
try
{
    options = LaunchOptions.Parse(args);
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine(LaunchOptions.Usage);
    return 1;
}

using var scenario = options.ScenarioFile is null ? null : new StreamReader(options.ScenarioFile);
var io = new ConsoleIo(scenario ?? Console.In, Console.Out, echoInput: scenario is not null);

try
{
    using var provider = CompositionRoot.Build(io, options.DataFile);
    provider.GetRequiredService<PosConsole>().Run();
}
catch (InvalidDataException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

return 0;
