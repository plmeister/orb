namespace Orb.Abstractions;

public interface IStorage
{
    Task<T?> GetAsync<T>(string tenantId, string key, CancellationToken ct);

    Task PutAsync<T>(string? tenantId, string key, T value, CancellationToken ct);

    Task DeleteAsync(string? tenantId, string key, CancellationToken ct);
}
