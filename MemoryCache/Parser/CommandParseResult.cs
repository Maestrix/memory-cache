using System;

namespace MemoryCache.Parser;

public struct CommandParseResult
{
    public ReadOnlyMemory<char> Command { get; set; }
    public ReadOnlyMemory<char> Key { get; set; }
    public ReadOnlyMemory<char> Value { get; set; }
}
