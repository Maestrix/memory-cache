using MemoryCache.Tcp;

TcpServer tcpServer = new();
await tcpServer.StartAsync();

Console.ReadLine();
