using System.Text.Json;
using Orb.Abstractions;

namespace Orb.Storage;

public sealed class FileStorage(string root) : IStorage
{
    private readonly string _root = root;

    public async Task PutAsync<T>(string? tenantId, string key, T value, CancellationToken ct)
    {
        var path = BuildPath(tenantId, key);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var stream = File.Create(path);

            await JsonSerializer.SerializeAsync(stream, value, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string? tenantId, string key, CancellationToken ct)
    {
        var path = BuildPath(tenantId, key);

        if (!File.Exists(path))
            return default;

        await using var stream = File.OpenRead(path);

        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct);
    }

    public Task DeleteAsync(string? tenantId, string key, CancellationToken ct)
    {
        var path = BuildPath(tenantId, key);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string BuildPath(string? tenantId, string key)
    {
        return Path.Combine(_root, tenantId ?? "default", key + ".json");
    }
}
