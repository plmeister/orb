using Microsoft.Extensions.Logging.Abstractions;
using Orb.Storage;

namespace Orb.Tests;

public class FileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"orb_test_{Guid.NewGuid()}");
    private readonly FileStorage _sut;

    public FileStorageTests()
    {
        _sut = new FileStorage(_root, NullLogger<FileStorage>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task Put_and_try_get_roundtrips_value()
    {
        await _sut.PutAsync("t1", "mykey", 42, CancellationToken.None);
        var (ok, value) = await _sut.TryGetAsync<int>("t1", "mykey", CancellationToken.None);

        Assert.True(ok);
        Assert.Equal(42, value);
    }

    [Fact]
    public async Task TryGet_non_existent_key_returns_false()
    {
        var (ok, value) = await _sut.TryGetAsync<string>("t1", "nonexistent", CancellationToken.None);

        Assert.False(ok);
        Assert.Null(value);
    }

    [Fact]
    public async Task Delete_removes_value()
    {
        await _sut.PutAsync("t1", "todelete", "hello", CancellationToken.None);
        await _sut.DeleteAsync("t1", "todelete", CancellationToken.None);

        var (ok, _) = await _sut.TryGetAsync<string>("t1", "todelete", CancellationToken.None);
        Assert.False(ok);
    }

    [Fact]
    public async Task Delete_non_existent_key_does_not_throw()
    {
        var ex = await Record.ExceptionAsync(
            () => _sut.DeleteAsync("t1", "doesnotexist", CancellationToken.None)
        );
        Assert.Null(ex);
    }

    [Fact]
    public async Task Stores_in_tenant_scoped_directory()
    {
        await _sut.PutAsync("tenantA", "k", "a", CancellationToken.None);
        await _sut.PutAsync("tenantB", "k", "b", CancellationToken.None);

        var (okA, valA) = await _sut.TryGetAsync<string>("tenantA", "k", CancellationToken.None);
        var (okB, valB) = await _sut.TryGetAsync<string>("tenantB", "k", CancellationToken.None);

        Assert.True(okA);
        Assert.Equal("a", valA);
        Assert.True(okB);
        Assert.Equal("b", valB);
    }

    [Fact]
    public async Task Default_tenant_when_null()
    {
        await _sut.PutAsync<string>(null, "k", "default", CancellationToken.None);

        var filePath = Path.Combine(_root, "default", "k.json");
        Assert.True(File.Exists(filePath));
    }
}
