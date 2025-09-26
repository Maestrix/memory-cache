using MemoryCache.Parser;
using Shouldly;

namespace MemoryCache.Tests;

public class CommandParserTests
{
    [Fact]
    public void String_contains_command_and_key_and_value()
    {
        //arrange
        string input = "SET key1 data1 data2";

        //act
        CommandParseResult result = CommandParser.Parse(input);

        //assert
        result.Command.IsEmpty.ShouldBeFalse();
        result.Key.IsEmpty.ShouldBeFalse();
        result.Value.IsEmpty.ShouldBeFalse();
    }

    [Theory]
    [InlineData("GET key1")] // Test case 1: contans command and key
    [InlineData("GET key1 ")] // Test case 2: contans command, key and space at the end
    public void String_contains_command_and_key(string input)
    {
        //act
        CommandParseResult result = CommandParser.Parse(input);

        //assert
        result.Command.IsEmpty.ShouldBeFalse();
        result.Key.IsEmpty.ShouldBeFalse();
        result.Value.IsEmpty.ShouldBeTrue();
    }

    [Theory]
    [InlineData("GET  key1")] // Test case 1: more one space between command and key
    [InlineData("SET   key1   data")] // Test case 1: more one space between command and key and value
    [InlineData("GET")] // Test case 2: only contains command
    [InlineData("GET ")] // Test case 2: only contains command and space
    public void String_not_contains_key_or_command_or_value(string input)
    {
        //act
        CommandParseResult result = CommandParser.Parse(input);

        //assert
        result.Command.IsEmpty.ShouldBeTrue();
        result.Key.IsEmpty.ShouldBeTrue();
        result.Value.IsEmpty.ShouldBeTrue();
    }
}