namespace MemoryCache.Parser;

public static class CommandParser
{
    public static CommandParseResult Parse(ReadOnlySpan<char> values)
    {
        CommandParseResult result = new();

        int index = values.IndexOf(' ');

        if (index < 0)
            return result;

        ReadOnlySpan<char> command = values.Slice(0, index);

        values = values[(index + 1)..];

        index = values.IndexOf(' ');

        if (index < 0 && values.IsEmpty == false)
        {
            result.Command = command;
            result.Key = values;
            return result;
        }
        else if (index < 0 && values.IsEmpty)
        {
            return result;
        }

        result.Key = values.Slice(0, index);

        if (result.Key.IsEmpty)
            return result;

        result.Command = command;        
        result.Value = values.Slice(index + 1);

        return result;
    }
}
