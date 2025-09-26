using System;

namespace MemoryCache.Parser;

public ref struct CommandParseResult
{
    public ReadOnlySpan<char> Command { get; set; }
    public ReadOnlySpan<char> Key { get; set; }
    public ReadOnlySpan<char> Value { get; set; }
}
