namespace FitParser.Web.Models;

public sealed class AnalyzeFitResponse
{
    public string FileId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int MessageCount { get; set; }

    public IReadOnlyList<MessageResponse> Messages { get; set; } =
        [];
}