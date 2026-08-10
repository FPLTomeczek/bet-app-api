using System.Reflection;
using System.Text.RegularExpressions;

namespace BetApp.Api.Tests;

/// <summary>
/// Guards the *convention* of the error-code contract, not its coverage: nothing here can
/// tell whether the front has a Polish translation for a given code, because the OpenAPI
/// schema types `errors` as a plain string dictionary and cannot carry this union.
/// What it can do is stop a typo or a duplicate from reaching the wire.
/// </summary>
public class ErrorCodesTests
{
    private static readonly string[] Codes = typeof(ErrorCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
        .Select(f => (string)f.GetRawConstantValue()!)
        .ToArray();

    [Fact]
    public void Codes_AreNotEmpty()
    {
        Assert.NotEmpty(Codes);
    }

    [Fact]
    public void Codes_AreUnique()
    {
        // A duplicated value would make two distinct failures indistinguishable to the client.
        Assert.Equal(Codes.Length, Codes.Distinct().Count());
    }

    [Fact]
    public void Codes_UseSnakeCase()
    {
        // Flat snake_case keeps them usable as bare object keys in the front's lookup table.
        var invalid = Codes.Where(c => !Regex.IsMatch(c, "^[a-z][a-z0-9]*(_[a-z0-9]+)*$")).ToArray();

        Assert.Empty(invalid);
    }
}
