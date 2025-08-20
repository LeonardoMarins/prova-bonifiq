namespace ProvaPub.Provider;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
