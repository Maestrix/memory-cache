using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using MemoryCache.Parser;

namespace MemoryCache.Tcp;

public class TcpServer
{
    public async Task StartAsync()
    {
        int port = 8080;
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint endPoint = new(ipAddress, 8080);

        Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(endPoint);
        socket.Listen(10);

        Console.WriteLine($"Сервер запущен на {ipAddress}:{port}");

        while (true)
        {
            Socket clientSocket = await socket.AcceptAsync();

            _ = ProcessClientAsync(clientSocket);
        }
    }

    private async Task ProcessClientAsync(Socket clientSocket)
    {
        Console.WriteLine($"Подключен клиент {clientSocket.RemoteEndPoint}");

        try
        {
            while (true)
            {
                var pool = ArrayPool<byte>.Shared;
                byte[] buffer = pool.Rent(1024);

                try
                {
                    int bytes = await clientSocket.ReceiveAsync(buffer);

                    if (bytes == 0)
                    {
                        Console.WriteLine($"Клиент {clientSocket.RemoteEndPoint} отключился");
                        break;
                    }

                    string receivedString = Encoding.UTF8.GetString(buffer, 0, bytes);

                    var last = receivedString.Last();
                    var length = last == '\n' ? receivedString.Length - 1 : receivedString.Length;

                    CommandParseResult result = CommandParser.Parse(receivedString.AsMemory(0, length));

                    Console.WriteLine($"Command = '{result.Command}' | Key = '{result.Key}' | Value = '{result.Value}'");
                }
                finally
                {
                    pool.Return(buffer, clearArray: false);
                }
            }
        }
        finally
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket.Dispose();
        }
    }
}
