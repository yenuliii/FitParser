namespace FitParser.Web.Models;

public sealed class SelectFitDataRequest
{
    public string FileId { get; set; } =
        string.Empty;

    public IReadOnlyList<int>
        SelectedMessageNumbers { get; set; } =
        [];

    public IReadOnlyList<SelectedFieldKey>
        SelectedFieldKeys { get; set; } =
        [];

    public int FormatTableBasedOn { get; set; }
}

public sealed record SelectedFieldKey(
    int MessageNumber,
    int FieldNumber);