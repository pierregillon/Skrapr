namespace Skrapr.Presentation.Telemetry;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxlength)
        => value.Length <= maxlength ? value : value[..maxlength];
}