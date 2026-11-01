/*
This is the entry point.

1. Receive input FIT file.
2. Call the extraction service.
3. Display available messages.
4. Let the user select messages.
5. Display available fields.
6. Let the user select fields.
7. Call the export service.
8. Report success or errors.
*/

using FitParser.Extract;
using FitParser.Select;
using FitParser.Export;
using FitParser.Models; 

Console.WriteLine("Yeni's FIT File Parser");
Console.WriteLine("----------------------");

string inputFilePath =
    GetPath(
        args,
        0,
        "Enter the path to the input FIT file: ");

try
{
    FitDataExtractionService extractionService =
        new(
            checkIntegrity: false);

    FitExtractionResult extractionResult =
        extractionService.Extract(
            inputFilePath);

    if (!extractionResult.IsSuccess)
    {
        Console.WriteLine();
        Console.WriteLine(
            "The FIT file could not be extracted.");

        Console.WriteLine(
            extractionResult.ErrorMessage);

        return;
    }

    MessageCollection collection =
        extractionResult.Collection!;

    FitDataSelectionService selectionService =
        new();

    IReadOnlyList<AvailableMessage>
        availableMessages =
            selectionService.GetAvailableMessages(
                collection);

    DisplayAvailableMessages(
        availableMessages);

    HashSet<int> selectedMessageNumbers =
        GetSelectedMessageNumbers(
            availableMessages);

    MessageCollection selectedMessages =
        selectionService.SelectMessages(
            collection,
            selectedMessageNumbers);

    IReadOnlyList<AvailableField>
        availableFields =
            selectionService.GetAvailableFields(
                selectedMessages);

    if (availableFields.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            "No fields were found in the selected messages.");

        return;
    }

    DisplayAvailableFields(
        availableFields);

    HashSet<FieldKey> selectedFieldKeys =
        GetSelectedFieldKeys(
            availableFields);

    MessageCollection selectedCollection =
        selectionService.SelectFields(
            selectedMessages,
            selectedFieldKeys);

    if (selectedCollection.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            "No fields were selected for extraction.");

        return;
    }

    ExportFormat outputFormat =
        GetOutputFormat();

    string outputFilePath =
        GetPath(
            args,
            1,
            "Enter the output CSV path: ");

    string? outputDirectory =
        Path.GetDirectoryName(
            outputFilePath);

    if (!string.IsNullOrWhiteSpace(
            outputDirectory))
    {
        Directory.CreateDirectory(
            outputDirectory);
    }

    FitDataExportService exportService =
        new();

    ExportResult exportResult =
        exportService.Export(
            selectedCollection,
            outputFormat,
            outputFilePath);

    if (!exportResult.IsSuccess)
    {
        Console.WriteLine();
        Console.WriteLine(
            exportResult.ErrorMessage);

        return;
    }

    int selectedFieldCount =
        selectedCollection.Messages
            .SelectMany(message => message.Fields)
            .Count();

    Console.WriteLine();
    Console.WriteLine(
        "FIT file exported successfully.");

    Console.WriteLine(
        $"Messages decoded: {collection.Count}");

    Console.WriteLine(
        $"Messages selected: " +
        $"{selectedMessages.Count}");

    Console.WriteLine(
        $"Fields selected: " +
        $"{selectedFieldCount}");

    Console.WriteLine(
        $"Output format: " +
        $"{outputFormat}");

    Console.WriteLine(
        $"Output written to: " +
        $"{outputFilePath}");
}
catch (Exception exception)
{
    Console.WriteLine();
    Console.WriteLine(
        "The application could not complete " +
        "the operation.");

    Console.WriteLine(
        $"Error: {exception.Message}");
}

static void DisplayAvailableMessages(
    IReadOnlyList<AvailableMessage>
        availableMessages)
{
    Console.WriteLine();
    Console.WriteLine(
        "Messages found in the FIT file:");

    Console.WriteLine(
        "--------------------------------");

    foreach (AvailableMessage message
        in availableMessages)
    {
        Console.WriteLine(
            $"Message Number: " +
            $"{message.MessageNumber} | " +
            $"Name: {message.MessageName} | " +
            $"Occurrences: {message.Count}");
    }

    Console.WriteLine();
}

static HashSet<int> GetSelectedMessageNumbers(
    IReadOnlyList<AvailableMessage>
        availableMessages)
{
    HashSet<int> availableMessageNumbers =
        availableMessages
            .Select(message =>
                message.MessageNumber)
            .ToHashSet();

    while (true)
    {
        Console.Write(
            "Enter message numbers separated " +
            "by commas or type 'all': ");

        string? input =
            Console.ReadLine();

        if (string.Equals(
                input?.Trim(),
                "all",
                StringComparison.OrdinalIgnoreCase))
        {
            return availableMessageNumbers;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine(
                "Please select at least one message.");

            continue;
        }

        string[] parts =
            input.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries);

        HashSet<int> selectedMessageNumbers =
            new();

        bool isValid = true;

        foreach (string part in parts)
        {
            if (!int.TryParse(
                    part.Trim(),
                    out int messageNumber))
            {
                Console.WriteLine(
                    $"Invalid message number: " +
                    $"{part.Trim()}");

                isValid = false;
                break;
            }

            if (!availableMessageNumbers.Contains(
                    messageNumber))
            {
                Console.WriteLine(
                    $"Message number {messageNumber} " +
                    "was not found in the file.");

                isValid = false;
                break;
            }

            selectedMessageNumbers.Add(
                messageNumber);
        }

        if (isValid &&
            selectedMessageNumbers.Count > 0)
        {
            return selectedMessageNumbers;
        }

        Console.WriteLine(
            "Please enter valid message numbers.");
    }
}

static void DisplayAvailableFields(
    IReadOnlyList<AvailableField>
        availableFields)
{
    Console.WriteLine();
    Console.WriteLine(
        "Fields found in the selected messages:");

    Console.WriteLine(
        "--------------------------------------");

    int? currentMessageNumber = null;

    foreach (AvailableField field
        in availableFields)
    {
        bool isNewMessage =
            currentMessageNumber !=
            field.MessageNumber;

        if (isNewMessage)
        {
            Console.WriteLine();
            Console.WriteLine(
                $"{field.MessageName} " +
                $"({field.MessageNumber})");

            currentMessageNumber =
                field.MessageNumber;
        }

        Console.WriteLine(
            $"  Field Number: " +
            $"{field.FieldNumber} | " +
            $"Name: {field.FieldName} | " +
            $"Occurrences: {field.Count}");
    }

    Console.WriteLine();
}

static HashSet<FieldKey> GetSelectedFieldKeys(
    IReadOnlyList<AvailableField>
        availableFields)
{
    HashSet<FieldKey> availableFieldKeys =
        availableFields
            .Select(field =>
                new FieldKey(
                    field.MessageNumber,
                    field.FieldNumber))
            .ToHashSet();

    while (true)
    {
        Console.Write(
            "Enter fields using " +
            "messageNumber:fieldNumber " +
            "separated by commas, or type 'all': ");

        string? input =
            Console.ReadLine();

        if (string.Equals(
                input?.Trim(),
                "all",
                StringComparison.OrdinalIgnoreCase))
        {
            return availableFieldKeys;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine(
                "Please select at least one field.");

            continue;
        }

        string[] parts =
            input.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries);

        HashSet<FieldKey> selectedFieldKeys =
            new();

        bool isValid = true;

        foreach (string part in parts)
        {
            string[] fieldParts =
                part.Trim().Split(
                    ':',
                    StringSplitOptions.RemoveEmptyEntries);

            if (fieldParts.Length != 2)
            {
                Console.WriteLine(
                    $"Invalid field selection: " +
                    $"{part.Trim()}");

                isValid = false;
                break;
            }

            if (!int.TryParse(
                    fieldParts[0].Trim(),
                    out int messageNumber))
            {
                Console.WriteLine(
                    $"Invalid message number: " +
                    $"{fieldParts[0].Trim()}");

                isValid = false;
                break;
            }

            if (!int.TryParse(
                    fieldParts[1].Trim(),
                    out int fieldNumber))
            {
                Console.WriteLine(
                    $"Invalid field number: " +
                    $"{fieldParts[1].Trim()}");

                isValid = false;
                break;
            }

            FieldKey fieldKey =
                new(
                    messageNumber,
                    fieldNumber);

            if (!availableFieldKeys.Contains(
                    fieldKey))
            {
                Console.WriteLine(
                    $"Message {messageNumber}, " +
                    $"Field {fieldNumber} " +
                    "was not found.");

                isValid = false;
                break;
            }

            selectedFieldKeys.Add(
                fieldKey);
        }

        if (isValid &&
            selectedFieldKeys.Count > 0)
        {
            return selectedFieldKeys;
        }

        Console.WriteLine(
            "Please enter valid field selections.");
    }
}

static ExportFormat GetOutputFormat()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Available output formats:");

        Console.WriteLine("1. CSV");

        Console.Write(
            "Select an output format: ");

        string? input =
            Console.ReadLine();

        if (input?.Trim() == "1" ||
            string.Equals(
                input?.Trim(),
                "csv",
                StringComparison.OrdinalIgnoreCase))
        {
            return ExportFormat.Csv;
        }

        Console.WriteLine(
            "Invalid format. Please select CSV.");
    }
}

static string GetPath(
    string[] arguments,
    int argumentIndex,
    string prompt)
{
    if (arguments.Length > argumentIndex &&
        !string.IsNullOrWhiteSpace(
            arguments[argumentIndex]))
    {
        return arguments[argumentIndex].Trim();
    }

    while (true)
    {
        Console.Write(prompt);

        string? path =
            Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(path))
        {
            return path.Trim();
        }

        Console.WriteLine(
            "A file path is required.");
    }
}