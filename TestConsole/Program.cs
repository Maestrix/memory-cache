
using MemoryCache.Parser;

var result = CommandParser.Parse("SET  key  ");

Console.WriteLine($"Command = '{result.Command.ToString()}'");
Console.WriteLine($"Key = '{result.Key.ToString()}'");
Console.WriteLine($"Value = '{result.Value.ToString()}'");
