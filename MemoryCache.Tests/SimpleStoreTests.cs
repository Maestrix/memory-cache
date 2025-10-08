using System;
using System.Text;
using MemoryCache.Storage;
using Shouldly;

namespace MemoryCache.Tests;

public class SimpleStoreTests
{
    [Fact]
    public async Task Correct_thread_safe_working()
    {
        //arrange
        using SimpleStore store = new();

        int operationCount = 50;
        List<Task> tasks = new(operationCount * 2);

        for (int i = 0; i < operationCount; i++)
        {
            tasks.Add(Task.Run(() => store.Set($"key{i}", Encoding.UTF8.GetBytes($"value number of {i}"))));
            tasks.Add(Task.Run(() => store.Get($"key{i}")));
        }

        //act
        await Task.WhenAll(tasks);

        //assert
        var (SetCount, GetCount, DeleteCount) = store.GetStatistics();
        SetCount.ShouldBe(operationCount);
        GetCount.ShouldBe(operationCount);
        DeleteCount.ShouldBe(0);
    }
}
