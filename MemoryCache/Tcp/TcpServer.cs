using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using MemoryCache.Parser;

namespace MemoryCache.Tcp;

public class TcpServer
{
    private readonly IPEndPoint _endPoint;

    public TcpServer(string ip, int port)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        
        _endPoint = new(IPAddress.Parse(ip), port);
    }

    public async Task StartAsync(CancellationToken token)
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(_endPoint);
        socket.Listen(10);

        Console.WriteLine($"Сервер запущен на {_endPoint.Address}:{_endPoint.Port}");

        while (true)
        {
            Socket clientSocket = await socket.AcceptAsync(token);

            _ = ProcessClientAsync(clientSocket, token);
        }
    }

    private static async Task ProcessClientAsync(Socket clientSocket, CancellationToken token)
    {
        Console.WriteLine($"Подключен клиент {clientSocket.RemoteEndPoint}");

        try
        {
            var pool = ArrayPool<byte>.Shared;
            byte[] buffer = pool.Rent(1024);

            try
            {
                while (true)
                {
                    int bytes = await clientSocket.ReceiveAsync(buffer, token);

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
            }
            finally
            {
                pool.Return(buffer, clearArray: false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"""
            Ошибка обработки клиента {clientSocket.RemoteEndPoint}.
            Описание: {ex.Message}
            """);
        }
        finally
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket.Dispose();
        }
    }
}
