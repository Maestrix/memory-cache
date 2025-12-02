using MemoryCache.Storage;
using MemoryCache.Tcp;

var sourceToken = new CancellationTokenSource();

using var store = new SimpleStore();
TcpServer tcpServer = new("127.0.0.1", 8080, store);
await tcpServer.StartAsync(sourceToken.Token);

Console.ReadLine();
