using MemoryCache.Storage;
using MemoryCache.Tcp;

var sourceToken = new CancellationTokenSource();

TcpServer tcpServer = new("127.0.0.1", 8080, new SimpleStore());
await tcpServer.StartAsync(sourceToken.Token);

Console.ReadLine();
