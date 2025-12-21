using System.Text.Json;
using BenchmarkDotNet.Attributes;
using LoadTester.Models;

namespace LoadTester;

[MemoryDiagnoser]
public class SerializationBenchmarks
{
    private readonly UserProfile _user = new()
    {
        Id = 123,
        UserName = "Иван Петров",
        CreatedAt = DateTime.UtcNow.AddHours(3),
        IsWorker = true
    };

    private readonly JsonSerializerOptions _stjOptions = new(JsonSerializerDefaults.General);

    [Benchmark]
    public byte[] SystemTextJson()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_user, _stjOptions);
    }

    [Benchmark]
    public void SourceGenerator()
    {
        using var ms = new MemoryStream();
        _user.SerializeToBinary(ms);
    }
}
