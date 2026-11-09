/*
methods for storing temporary files 
SaveAsync = save uploaded fit file and return a file id
GetPath = Return location of file with file id
DeleteAsync = delete temporary file with this id 
*/ 

namespace FitParser.Api.Storage;

public interface ITemporaryFileStore
{
    Task<string> SaveAsync(
        Stream contents,
        string originalFileName,
        CancellationToken cancellationToken = default);

    string GetPathOfFileId(
        string fileId);

    Task DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default);
}