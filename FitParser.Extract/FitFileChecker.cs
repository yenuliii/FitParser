/*
    This file is responsible for:

    1. Checking whether the file exists.
    2. Checking whether the file can be opened.
    3. Checking whether the file is a FIT file.
    4. Optionally checking the file's integrity.
    5. Returning validation errors.
*/

using Dynastream.Fit;

namespace FitParser.Core;

public sealed class FitFileChecker
{
    public (
        bool IsValid,
        string? ErrorMessage
    ) Validate(
        string filePath,
        bool checkIntegrity = false)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return (
                false,
                "An input file path is required."
            );
        }

        if (!System.IO.File.Exists(filePath))
        {
            return (
                false,
                "The input file does not exist."
            );
        }

        try
        {
            using FileStream stream =
                System.IO.File.OpenRead(filePath);

            Decode decoder = new Decode();

            // Checks the FIT file signature in the header.
            if (!decoder.IsFIT(stream))
            {
                return (
                    false,
                    "The file is not a valid FIT file."
                );
            }

            if (checkIntegrity)
            {
                stream.Position = 0;

                // Checks the header, data size, and CRC.
                if (!decoder.CheckIntegrity(stream))
                {
                    return (
                        false,
                        "The FIT file failed the integrity check."
                    );
                }
            }

            return (
                true,
                null
            );
        }
        catch (UnauthorizedAccessException exception)
        {
            return (
                false,
                $"The file could not be accessed: {exception.Message}"
            );
        }
        catch (IOException exception)
        {
            return (
                false,
                $"The file could not be opened: {exception.Message}"
            );
        }
        catch (FitException exception)
        {
            return (
                false,
                $"FIT validation failed: {exception.Message}"
            );
        }
    }
}