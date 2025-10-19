namespace MemoryCache.Storage;

public sealed class SimpleStore : IStorage, IDisposable
{
    private readonly Dictionary<string, byte[]> _storage = [];
    private readonly ReaderWriterLockSlim _lock = new();

    private long _setCount;
    private long _getCount;
    private long _deleteCount;

    /// <summary>
    /// Выдает информацию о количестве операций
    /// </summary>
    public (long SetCount, long GetCount, long DeleteCount) GetStatistics()
    {
        return (_setCount, _getCount, _deleteCount);
    }

    public void Set(string key, byte[] value)
    {
        _lock.EnterWriteLock();

        try
        {
            _storage[key] = value;

            Interlocked.Increment(ref _setCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public byte[]? Get(string key)
    {
        _lock.EnterReadLock();

        try
        {
            Interlocked.Increment(ref _getCount);
            return _storage.GetValueOrDefault(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Delete(string key)
    {
        _lock.EnterWriteLock();

        try
        {
            if (_storage.Remove(key))
            {
                Interlocked.Increment(ref _deleteCount);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}
