/*
This file will be reponsible for checking the intergrity of the file 
using the fit SDK's IsFit(), CHeckIntegrity() and will handle errors
*/
using Dynastream.Fit;

namespace FitParser.Core;

public sealed class FitFileChecker
{
    public FitValidationResult Validate(
        string filePath,
        bool checkIntegrity = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return FitValidationResult.Invalid(
                "An input file path is required.");
        }

        if (!System.IO.File.Exists(filePath))
        {
            return FitValidationResult.Invalid(
                "The input file does not exist.");
        }

        try
        {
            using FileStream stream = System.IO.File.OpenRead(filePath);
            Decode decoder = new Decode();

            // Checks the FIT file signature in the header.
            if (!decoder.IsFIT(stream))
            {
                return FitValidationResult.Invalid(
                    "The file is not a valid FIT file.");
            }

            if (checkIntegrity)
            {
                stream.Position = 0;

                // Checks the header, data size, and CRC.
                if (!decoder.CheckIntegrity(stream))
                {
                    return FitValidationResult.Invalid(
                        "The FIT file failed the integrity check.");
                }
            }

            return FitValidationResult.Valid();
        }
        catch (UnauthorizedAccessException exception)
        {
            return FitValidationResult.Invalid(
                $"The file could not be accessed: {exception.Message}");
        }
        catch (IOException exception)
        {
            return FitValidationResult.Invalid(
                $"The file could not be opened: {exception.Message}");
        }
        catch (FitException exception)
        {
            return FitValidationResult.Invalid(
                $"FIT validation failed: {exception.Message}");
        }
    }
}
