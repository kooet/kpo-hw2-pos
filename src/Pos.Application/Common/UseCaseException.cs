namespace Pos.Application.Common;

/// <summary>Сценарий нельзя выполнить: студент не найден, у критерия есть оценки и т. п.</summary>
public sealed class UseCaseException : Exception
{
    public UseCaseException(string message)
        : base(message)
    {
    }
}
