using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTester;

public class SimpleTcpClient : IDisposable
{
    private TcpClient _client = new();
    private NetworkStream _stream;
    private readonly string _host;
    private readonly int _port;

    public SimpleTcpClient(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        if (_client.Connected)
            throw new InvalidOperationException("Соединение установлено ранее");

        await _client.ConnectAsync(_host, _port);
        _stream = _client.GetStream();
    }

    public async Task SetAsync(string key, byte[] value)
    {
        if (!_client.Connected || _stream == null)
            throw new InvalidOperationException("Соединение не установлено");

        // Формируем сообщение: SET + key length + key + value length + value
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var keyLen = BitConverter.GetBytes(keyBytes.Length);
        var valueLen = BitConverter.GetBytes(value.Length);

        var message = new byte[4 + keyBytes.Length + 4 + value.Length];
        int offset = 0;

        // Команда SET (4 байта)
        Encoding.UTF8.GetBytes("SET ", 0, 4, message, offset);
        offset += 4;

        // Длина ключа (4 байта)
        Buffer.BlockCopy(keyLen, 0, message, offset, 4);
        offset += 4;

        // Ключ
        Buffer.BlockCopy(keyBytes, 0, message, offset, keyBytes.Length);
        offset += keyBytes.Length;

        // Длина значения (4 байта)
        Buffer.BlockCopy(valueLen, 0, message, offset, 4);
        offset += 4;

        // Значение
        Buffer.BlockCopy(value, 0, message, offset, value.Length);

        await _stream.WriteAsync(message, 0, message.Length);
        await _stream.FlushAsync();
    }

    public async Task<byte[]> GetAsync(string key)
    {
        if (!_client.Connected || _stream == null)
            throw new InvalidOperationException("Соединение не установлено");

        // Формируем сообщение: GET + key length + key
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var keyLen = BitConverter.GetBytes(keyBytes.Length);

        var message = new byte[4 + 4 + keyBytes.Length];
        int offset = 0;

        // Команда GET (4 байта)
        Encoding.UTF8.GetBytes("GET ", 0, 4, message, offset);
        offset += 4;

        // Длина ключа (4 байта)
        Buffer.BlockCopy(keyLen, 0, message, offset, 4);
        offset += 4;

        // Ключ
        Buffer.BlockCopy(keyBytes, 0, message, offset, keyBytes.Length);

        await _stream.WriteAsync(message, 0, message.Length);
        
        byte[] buffer = new byte[1024];

        await _stream.ReadAsync(buffer, 0, buffer.Length);
        
        return buffer;
    }
    
    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();
    }
}
