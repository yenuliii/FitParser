namespace FitParser.Web.Models;

public sealed class FieldResponse
{
    public int FieldNumber { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public int Occurrences { get; set; }
}