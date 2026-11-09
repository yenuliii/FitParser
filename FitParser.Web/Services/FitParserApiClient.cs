namespace FitParser.Web.Services;

using System.Net.Http.Json;
using FitParser.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

public sealed class FitParserApiClient
{
    private readonly HttpClient _httpClient;

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

        return result
            ?? throw new InvalidOperationException(
                "The API returned an empty response.");
    }
}