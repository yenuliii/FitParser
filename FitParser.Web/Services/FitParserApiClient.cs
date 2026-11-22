using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace FitParser.Web.Services;

public sealed class FitParserApiClient
{
    private readonly HttpClient _httpClient;

    public FitParserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExportResponse> ExportAllAsync(
        IBrowserFile file)
    {
        using MultipartFormDataContent form =
            new();

        await using Stream fileStream =
            file.OpenReadStream(
                maxAllowedSize: 100_000_000);

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
                "api/fit/export-all",
                form);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            return ExportResponse.Failure(error);
        }

        byte[] csvBytes =
            await response.Content.ReadAsByteArrayAsync();

        return ExportResponse.Success(csvBytes);
    }

    public async Task<AnalyzeResponse> AnalyzeAsync(
    IBrowserFile file)
    {
        using MultipartFormDataContent form =
            new();

        await using Stream fileStream =
            file.OpenReadStream(
                maxAllowedSize: 100_000_000);

        using StreamContent fileContent =
            new(fileStream);

        form.Add(
            fileContent,
            "file",
            file.Name);

        using HttpResponseMessage response =
            await _httpClient.PostAsync(
                "api/fit/analyze",
                form);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync();

            return AnalyzeResponse.Failure(error);
        }

        AnalyzeResponse? result =
            await response.Content
                .ReadFromJsonAsync<AnalyzeResponse>();

        return result
            ?? AnalyzeResponse.Failure(
                "The API returned an empty response.");
    }
}

public sealed class ExportResponse
{
    public bool IsSuccess { get; }

    public byte[]? CsvBytes { get; }

    public string? ErrorMessage { get; }

    private ExportResponse(
        bool isSuccess,
        byte[]? csvBytes,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        CsvBytes = csvBytes;
        ErrorMessage = errorMessage;
    }

    public static ExportResponse Success(
        byte[] csvBytes)
    {
        return new ExportResponse(
            true,
            csvBytes,
            null);
    }

    public static ExportResponse Failure(
        string errorMessage)
    {
        return new ExportResponse(
            false,
            null,
            errorMessage);
    }
}

public sealed class AnalyzeResponse
{
    public List<AvailableMessageModel> Messages { get; set; } = [];

    public List<AvailableFieldModel> Fields { get; set; } = [];

    public string? ErrorMessage { get; set; }

    public bool IsSuccess =>
        ErrorMessage is null;

    public static AnalyzeResponse Failure(
        string errorMessage)
    {
        return new AnalyzeResponse
        {
            ErrorMessage = errorMessage
        };
    }
}

public sealed class AvailableMessageModel
{
    public int MessageNumber { get; set; }

    public string MessageName { get; set; } = string.Empty;

    public int Count { get; set; }
}

public sealed class AvailableFieldModel
{
    public int MessageNumber { get; set; }

    public string MessageName { get; set; } = string.Empty;

    public int FieldNumber { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public int Count { get; set; }
}