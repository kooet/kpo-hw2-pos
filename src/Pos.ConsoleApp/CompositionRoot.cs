using Microsoft.Extensions.DependencyInjection;
using Pos.Application.Ports;
using Pos.Application.UseCases;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Storage;

namespace Pos.ConsoleApp;

/// <summary>
/// Composition root — единственное место, которое знает все слои сразу и связывает порты с адаптерами.
/// </summary>
public static class CompositionRoot
{
    public static ServiceProvider Build(ConsoleIo io, string? dataFile)
    {
        var services = new ServiceCollection();

        // Domain
        services.AddSingleton<EvaluationFactory>();
        services.AddSingleton<FinalGradeCalculator>();
        services.AddSingleton<IFinalGradeStrategy, WeightedAllCriteriaStrategy>();
        services.AddSingleton<IFinalGradeStrategy, WeightedGradedOnlyStrategy>();

        // Application
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<GradingRules>();
        services.AddSingleton<StudentService>();
        services.AddSingleton<RubricService>();
        services.AddSingleton<EvaluationService>();
        services.AddSingleton<FinalGradeService>();

        // Adapters
        if (dataFile is null)
        {
            AddStorage(services, _ => new InMemoryStorage());
        }
        else
        {
            AddStorage(services, sp => new JsonFileStorage(dataFile, sp.GetRequiredService<EvaluationFactory>()));
        }

        services.AddSingleton<IFinalReportExporter, CsvFinalReportExporter>();
        services.AddSingleton(io);
        services.AddSingleton<PosConsole>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static void AddStorage<TStorage>(IServiceCollection services, Func<IServiceProvider, TStorage> create)
        where TStorage : class, IStudentRepository, IRubricRepository
    {
        services.AddSingleton<TStorage>(create);
        services.AddSingleton<IStudentRepository>(sp => sp.GetRequiredService<TStorage>());
        services.AddSingleton<IRubricRepository>(sp => sp.GetRequiredService<TStorage>());
    }
}
