using System.Text;
using LoadTester;
using LoadTester.Models;
using NBomber.CSharp;

BenchmarkDotNet.Running.BenchmarkRunner.Run<SerializationBenchmarks>();

/*
var scenario = Scenario.Create("tcp_load_test", async context =>
{
    var step1 = await Step.Run("tcp_set_step", context, async () =>
    {
        var sourceToken = new CancellationTokenSource();

        using SimpleTcpClient client = new("127.0.0.1", 8080);
        await client.ConnectAsync(sourceToken.Token);

        var randomKey = $"key_{context.InvocationNumber}_{context.Random.Next(1000)}";
        var randomValue = new byte[context.Random.Next(10, 100)];
        context.Random.NextBytes(randomValue);

        var userProfile = new UserProfile
        {
            Id = context.Random.Next(),
            UserName = "Иван Петров",
            CreatedAt = DateTime.UtcNow.AddHours(3),
            IsWorker = true
        };

        bool isSuccess = await client.SetAsync("key1", userProfile, sourceToken.Token);

        return isSuccess ? Response.Ok() : Response.Fail();
    });

    return Response.Ok();
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5)) // Фаза разогрева 5 сек
.WithLoadSimulations
(
    Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
);

NBomberRunner.RegisterScenarios(scenario).Run();
*/

/*

var sourceToken = new CancellationTokenSource();

using SimpleTcpClient client = new("127.0.0.1", 8080);
await client.ConnectAsync(sourceToken.Token);

var userProfile = new UserProfile
{
    Id = 1234,
    UserName = "Иван Петров",
    CreatedAt = DateTime.UtcNow.AddHours(3),
    IsWorker = true
};

bool isSuccess = await client.SetAsync("key1", userProfile, sourceToken.Token);

if (isSuccess)
{
    string? cachedValue = await client.GetAsync("key1", sourceToken.Token);

    Console.WriteLine($"VALUE: {cachedValue}");
}
else
{
    Console.WriteLine("ERROR SET");
}

Console.WriteLine("END");

*/