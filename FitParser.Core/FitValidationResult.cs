namespace FitParser.Core;

public sealed class FitValidationResult
{
    public bool IsValid { get; }

    public string? ErrorMessage { get; }

    private FitValidationResult(
        bool isValid,
        string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static FitValidationResult Valid()
    {
        return new FitValidationResult(true, null);
    }

    public static FitValidationResult Invalid(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new FitValidationResult(false, errorMessage);
    }
}
