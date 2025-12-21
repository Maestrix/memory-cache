using MemoryCache.Models;

namespace MemoryCache.Storage;

public interface IStorage
{
    /// <summary>
    /// Добавляет или обновляет значение по ключу
    /// </summary>
    /// <param name="key">ключ</param>
    /// <param name="value">значение</param>
    void Set(string key, UserProfile? value);

    /// <summary>
    /// Возвращает значение по ключу или null, если ключ не найден
    /// </summary>
    /// <param name="key">ключ</param>
    /// <returns>Найденное значение</returns>
    UserProfile? Get(string key);

    /// <summary>
    /// удаляет ключ и значение
    /// </summary>
    /// <param name="key">ключ</param>
    void Delete(string key);
}
