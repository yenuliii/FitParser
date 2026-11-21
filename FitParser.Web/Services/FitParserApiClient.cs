namespace FitParser.Web.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using FitParser.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

public sealed class FitParserApiClient
{
    private readonly HttpClient _httpClient;

    public AnalyzeFitResponse? LastAnalysis { get; private set; }

    public FitParserApiClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AnalyzeFitResponse> AnalyzeFileAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        await using Stream fileStream =
            file.OpenReadStream(
                maxAllowedSize: 100 * 1024 * 1024,
                cancellationToken);

        using MultipartFormDataContent form =
            new();

        using StreamContent fileContent =
            new(fileStream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(
                "application/octet-stream");

        form.Add(
            fileContent,
            "file",
            file.Name);

        using HttpResponseMessage response =
            await _httpClient.PostAsync(
                "api/fit/analyze",
                form,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"The API could not analyze the file. " +
                $"Status: {response.StatusCode}. " +
                $"Details: {errorMessage}");
        }

        AnalyzeFitResponse? result =
            await response.Content.ReadFromJsonAsync<
                AnalyzeFitResponse>(
                    cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                "The API returned an empty analysis response.");
        }

        LastAnalysis =
            result;

        return result;
    }

    public async Task<ExportedFile> ExportSelectedDataAsync(
        string fileId,
        IReadOnlyCollection<int>
            selectedMessageNumbers,
        IReadOnlyCollection<SelectedFieldKey>
            selectedFieldKeys,
        int formatTableBasedOn = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileId);

        if (selectedMessageNumbers.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one message must be selected.");
        }

        if (selectedFieldKeys.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one field must be selected.");
        }

        SelectFitDataRequest request =
            new()
            {
                FileId =
                    fileId,

                SelectedMessageNumbers =
                    selectedMessageNumbers
                        .ToList()
                        .AsReadOnly(),

                SelectedFieldKeys =
                    selectedFieldKeys
                        .ToList()
                        .AsReadOnly(),

                FormatTableBasedOn =
                    formatTableBasedOn
            };

        using HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "api/fit/export",
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorMessage =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"The selected data could not be exported. " +
                $"Status: {response.StatusCode}. " +
                $"Details: {errorMessage}");
        }

        byte[] fileContents =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        string fileName =
            "fitparser-selected-data.csv";

        if (response.Content.Headers.ContentDisposition
            is ContentDispositionHeaderValue disposition &&
            !string.IsNullOrWhiteSpace(
                disposition.FileNameStar))
        {
            fileName =
                disposition.FileNameStar.Trim('"');
        }
        else if (response.Content.Headers.ContentDisposition
                 is ContentDispositionHeaderValue
                 regularDisposition &&
                 !string.IsNullOrWhiteSpace(
                     regularDisposition.FileName))
        {
            fileName =
                regularDisposition.FileName.Trim('"');
        }

        return new ExportedFile(
            fileContents,
            fileName);
    }

    public void ClearAnalysis()
    {
        LastAnalysis =
            null;
    }
}

public sealed class ExportedFile
{
    public byte[] Contents { get; }

    public string FileName { get; }

    public ExportedFile(
        byte[] contents,
        string fileName)
    {
        ArgumentNullException.ThrowIfNull(
            contents);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName);

        Contents =
            contents;

        FileName =
            fileName;
    }
}