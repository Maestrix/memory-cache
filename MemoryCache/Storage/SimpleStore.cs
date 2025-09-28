namespace MemoryCache.Storage;

public class SimpleStore
{
    private readonly Dictionary<string, byte[]> _storage;

    public SimpleStore()
    {
        _storage = [];
    }

    /// <summary>
    /// Добавляет или обновляет значение по ключу
    /// </summary>
    /// <param name="key">ключ</param>
    /// <param name="value">значение</param>
    public void Set(string key, byte[] value)
    {
        if (_storage.TryAdd(key, value) == false)
        {
            _storage[key] = value;
        }
    }

    /// <summary>
    /// Возвращает значение по ключу или null, если ключ не найден
    /// </summary>
    /// <param name="key">ключ</param>
    /// <returns>Найденное значение</returns>
    public byte[]? Get(string key)
    {
        return _storage.GetValueOrDefault(key);
    }

    /// <summary>
    /// удаляет ключ и значение
    /// </summary>
    /// <param name="key">ключ</param>
    public void Delete(string key)
    {
        _storage.Remove(key);
    }
}
