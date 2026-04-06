using Avenue804.Web.Domain;

namespace Avenue804.Web.Services;

public interface ILookupService
{
    Task<IReadOnlyList<LookupValue>> GetActiveValuesAsync(string categoryCode, CancellationToken cancellationToken = default);

    Task<LookupValue?> FindValueAsync(int id, string categoryCode, CancellationToken cancellationToken = default);
}
