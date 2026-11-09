namespace FitParser.Web.Models;

public sealed class MessageResponse
{
    public int MessageNumber { get; set; }

    public string MessageName { get; set; } = string.Empty;

    public int Occurrences { get; set; }

    public IReadOnlyList<FieldResponse> Fields { get; set; } =
        [];
}