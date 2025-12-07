using System.Text;
using LoadTester;

var sourceToken = new CancellationTokenSource();

using SimpleTcpClient client = new("127.0.0.1", 8080);
await client.ConnectAsync(sourceToken.Token);

bool isSuccess = await client.SetAsync("key1", Encoding.UTF8.GetBytes("test1 test1"), sourceToken.Token);

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
