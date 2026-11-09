/*
Implements the methods from ITemporaryFileStore.cs
*/

namespace FitParser.Api.Storage;

public sealed class TemporaryFileStore : ITemporaryFileStore
{
    private readonly string _storageDirectory;

    public TemporaryFileStore()
    {
        _storageDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "FitParser");

        Directory.CreateDirectory(
            _storageDirectory);
    }

    public async Task<string> SaveAsync(
        Stream contents,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contents);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            originalFileName);

        string fileId =
            Guid.NewGuid().ToString("N");

        string filePath =
            GetPathOfFileId(fileId);

        await using FileStream fileStream =
            new(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                options: FileOptions.Asynchronous);

        await contents.CopyToAsync(
            fileStream,
            cancellationToken);

        return fileId;
    }

    public string GetPathOfFileId(
        string fileId)
    {
        if (!Guid.TryParseExact(
                fileId,
                "N",
                out _))
        {
            throw new ArgumentException(
                "The temporary file ID is invalid.",
                nameof(fileId));
        }

        return Path.Combine(
            _storageDirectory,
            $"{fileId}.fit");
    }

    public Task DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        string filePath =
            GetPathOfFileId(fileId);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}