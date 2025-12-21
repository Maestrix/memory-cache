using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Buffers;
using MemoryCache.Parser;
using MemoryCache.Storage;
using MemoryCache.Models;

namespace MemoryCache.Tcp;

public class TcpServer
{
    private const string GetCommand = "GET";
    private const string SetCommand = "SET";
    private const string DeleteCommand = "DEL";

    private readonly byte[] _successResult = Encoding.UTF8.GetBytes("OK\r\n");
    private readonly byte[] _unknownCommandResult = Encoding.UTF8.GetBytes("-ERR Unknown command\r\n");
    private readonly byte[] _notFoundResult = Encoding.UTF8.GetBytes("(nil)\r\n");

    private readonly IPEndPoint _endPoint;
    private readonly IStorage _storage;

    public TcpServer(string ip, int port, IStorage storage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);
        ArgumentNullException.ThrowIfNull(storage);

        _endPoint = new(IPAddress.Parse(ip), port);
        _storage = storage;
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

    private async Task ProcessClientAsync(Socket clientSocket, CancellationToken token)
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

                    CommandParseResult parseResult = CommandParser.Parse(receivedString.AsMemory(0, length));

                    switch (parseResult.Command.Span)
                    {
                        case SetCommand:
                            UserProfile? userProfile = System.Text.Json.JsonSerializer.Deserialize<UserProfile>(parseResult.Value.Span);
                            _storage.Set(parseResult.Key.ToString(), userProfile);
                            await clientSocket.SendAsync(_successResult, token);
                            break;
                        case DeleteCommand:
                            _storage.Delete(parseResult.Key.ToString());
                            await clientSocket.SendAsync(_successResult, token);
                            break;
                        case GetCommand:
                            UserProfile? value = _storage.Get(parseResult.Key.ToString());
                            if (value is null)
                            {
                                await clientSocket.SendAsync(_notFoundResult, token);
                            }
                            else
                            {
                                await clientSocket.SendAsync(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value), token);
                            }
                            break;
                        default:
                            await clientSocket.SendAsync(_unknownCommandResult, token);
                            break;
                    }
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
