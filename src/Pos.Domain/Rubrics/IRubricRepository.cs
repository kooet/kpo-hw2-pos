namespace Pos.Domain.Rubrics;

/// <summary>Порт хранения рубрикатора. В модуле один курс, поэтому и рубрикатор один.</summary>
public interface IRubricRepository
{
    Rubric Get();

    void Save(Rubric rubric);
}
