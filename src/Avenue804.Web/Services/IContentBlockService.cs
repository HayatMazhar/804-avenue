namespace Avenue804.Web.Services;

public interface IContentBlockService
{
    Task<string?> GetPublishedBodyAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Load multiple published bodies in one query; dictionary keys are slug lowercased.</summary>
    Task<IReadOnlyDictionary<string, string>> GetPublishedBodiesAsync(
        IReadOnlyList<string> slugs,
        CancellationToken cancellationToken = default);
}
