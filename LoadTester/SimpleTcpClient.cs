using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTester;

public class SimpleTcpClient(string host, int port) : IDisposable
{
    private readonly TcpClient _client = new();
    private NetworkStream? _stream;
    private readonly string _host = host;
    private readonly int _port = port;

    public async Task ConnectAsync(CancellationToken token)
    {
        if (_client.Connected)
            throw new InvalidOperationException("Соединение установлено ранее");

        await _client.ConnectAsync(_host, _port, token);
        _stream = _client.GetStream();
    }

    public async Task<bool> SetAsync(string key, byte[] value, CancellationToken token)
    {
        if (!_client.Connected || _stream == null)
            throw new InvalidOperationException("Соединение не установлено");

        byte[] commandBytes = Encoding.UTF8.GetBytes("SET");
        byte[] spaceSeparator = Encoding.UTF8.GetBytes(" ");
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        byte[] message = new byte[commandBytes.Length + spaceSeparator.Length + keyBytes.Length + spaceSeparator.Length + value.Length];

        int offset = 0;

        Buffer.BlockCopy(commandBytes, 0, message, offset, commandBytes.Length);
        offset += commandBytes.Length;

        Buffer.BlockCopy(spaceSeparator, 0, message, offset, spaceSeparator.Length);
        offset += spaceSeparator.Length;

        Buffer.BlockCopy(keyBytes, 0, message, offset, keyBytes.Length);
        offset += keyBytes.Length;

        Buffer.BlockCopy(spaceSeparator, 0, message, offset, spaceSeparator.Length);
        offset += spaceSeparator.Length;

        Buffer.BlockCopy(value, 0, message, offset, value.Length);

        var arrayPool = ArrayPool<byte>.Shared;
        byte[] buffer = arrayPool.Rent(10);

        try
        {
            await _stream.WriteAsync(message.AsMemory(0, message.Length), token);
                       
            int read = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);

            if (read > 0)
            {
                string setResult = Encoding.UTF8.GetString(buffer, 0, read);

                if (setResult.StartsWith("OK"))
                    return true;
            }

            return false;
        }
        finally
        {
            await _stream.FlushAsync(token);
            arrayPool.Return(buffer, false);
        }
    }

    public async Task<string?> GetAsync(string key, CancellationToken token)
    {
        if (!_client.Connected || _stream == null)
            throw new InvalidOperationException("Соединение не установлено");

        byte[] commandBytes = Encoding.UTF8.GetBytes("GET");
        byte[] spaceSeparator = Encoding.UTF8.GetBytes(" ");
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);

        byte[] message = new byte[commandBytes.Length + spaceSeparator.Length + keyBytes.Length];

        int offset = 0;

        Buffer.BlockCopy(commandBytes, 0, message, offset, commandBytes.Length);
        offset += commandBytes.Length;

        Buffer.BlockCopy(spaceSeparator, 0, message, offset, spaceSeparator.Length);
        offset += spaceSeparator.Length;

        Buffer.BlockCopy(keyBytes, 0, message, offset, keyBytes.Length);

        var arrayPool = ArrayPool<byte>.Shared;
        byte[] buffer = arrayPool.Rent(1024);

        try
        {
            await _stream.WriteAsync(message.AsMemory(0, message.Length), token);

            int read = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);

            if(read > 0)
            {
                string getResult = Encoding.UTF8.GetString(buffer, 0, read);

                if (getResult.StartsWith("(nil)\r\n"))
                    return null;

                return getResult;
            }

            return null;
        }
        finally
        {
            await _stream.FlushAsync(token);
            arrayPool.Return(buffer, false);
        }
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();

        GC.SuppressFinalize(this);
    }
}
