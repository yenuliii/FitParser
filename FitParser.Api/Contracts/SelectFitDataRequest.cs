namespace FitParser.Api.Contracts;

using FitParser.Export;
using FitParser.Select;

public sealed class SelectFitDataRequest
{
    public string FileId { get; set; } =
        string.Empty;

    public List<int> SelectedMessageNumbers { get; set; } =
        [];

    public List<FieldKey> SelectedFieldKeys { get; set; } =
        [];

    public FormatTableBasedOn FormatTableBasedOn { get; set; } =
        FormatTableBasedOn.Original;
}