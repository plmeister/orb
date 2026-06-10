using System.Text.Json;
using Microsoft.Extensions.Logging;
using Orb.Abstractions;

namespace Orb.Storage;

public sealed class FileStorage(string root, ILogger<FileStorage> logger) : IStorage
{
  private readonly string _root = root;
  private readonly ILogger<FileStorage> _logger = logger;

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

  public async Task<(bool Success, T? value)> TryGetAsync<T>(string? tenantId, string key, CancellationToken ct)
  {
    var path = BuildPath(tenantId, key);

    if (!File.Exists(path))
      return (false, default);

    await using var stream = File.OpenRead(path);

    try
    {
      var value = await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: ct);
      return (true, value);
    }
    catch (Exception ex)
    {
      _logger.LogError("Exception while getting value from storage - tenantId: {tenantId}, key: {key}, exception: {ex}", tenantId, key, ex);
    }

    return (false, default);
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
