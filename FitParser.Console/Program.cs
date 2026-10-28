/*
This will be the entry point.

1. Receive input FIT file.
2. Decode and normalize the file.
3. Display available messages.
4. Let the user select messages.
5. Receive output path and format.
6. Export the selected messages.
7. Report success or errors.

Keep this file focused on coordinating the workflow
and handling console input.
*/

using FitParser.Core;
using FitParser.Export;

Console.WriteLine("Yeni's FIT File Parser");
Console.WriteLine("----------------------");

string inputFilePath = GetPath(
    args,
    0,
    "Enter the path to the input FIT file: ");

try
{
    const bool checkIntegrity = false;

    FitFileChecker checker = new();

    var validationResult =
        checker.Validate(inputFilePath, checkIntegrity);

    if (!validationResult.IsValid)
    {
        Console.WriteLine();
        Console.WriteLine("The FIT file is invalid.");
        Console.WriteLine(validationResult.ErrorMessage);
        return;
    }

    GeneralMessageHandler handler = new();

    MessageBroadcaster broadcaster =
        new(handler);

    FitFileDecoder decoder =
        new(broadcaster);

    decoder.DecodeFile(inputFilePath);

    MessageNormalizer normalizer = new();

    MessageCollection collection = new();

    foreach (var rawMessage in handler.Messages)
    {
        NormalizedMessage normalizedMessage =
            normalizer.Normalize(rawMessage);

        collection.Add(normalizedMessage);
    }

    if (collection.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine("No messages were found in the FIT file.");
        return;
    }

    MessageSelector selector = new();

    IReadOnlyList<AvailableMessage> availableMessages =
        selector.GetAvailableMessages(collection);

    DisplayAvailableMessages(availableMessages);

    HashSet<int> selectedMessageNumbers =
        GetSelectedMessageNumbers(availableMessages);

    MessageCollection selectedCollection =
        selector.SelectMessage(
            collection,
            selectedMessageNumbers);

    string outputFormat =
        GetOutputFormat();

    string outputFilePath = GetPath(
        args,
        1,
        "Enter the output CSV path: ");

    string? outputDirectory =
        Path.GetDirectoryName(outputFilePath);

    if (!string.IsNullOrWhiteSpace(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
    }

    if (outputFormat == "csv")
    {
        CsvExporter exporter = new();

        exporter.Export(
            selectedCollection,
            outputFilePath);
    }

    Console.WriteLine();
    Console.WriteLine("FIT file decoded successfully.");
    Console.WriteLine(
        $"Messages decoded: {collection.Count}");
    Console.WriteLine(
        $"Messages selected: {selectedCollection.Count}");
    Console.WriteLine(
        $"Output format: {outputFormat.ToUpperInvariant()}");
    Console.WriteLine(
        $"Output written to: {outputFilePath}");
}
catch (Exception exception)
{
    Console.WriteLine();
    Console.WriteLine("The FIT file could not be processed.");
    Console.WriteLine($"Error: {exception.Message}");
}

static void DisplayAvailableMessages(
    IReadOnlyList<AvailableMessage> availableMessages)
{
    Console.WriteLine();
    Console.WriteLine("Messages found in the FIT file:");
    Console.WriteLine("--------------------------------");

    foreach (AvailableMessage message in availableMessages)
    {
        Console.WriteLine(
            $"Message Number: {message.MessageNumber} | " +
            $"Name: {message.MessageName} | " +
            $"Occurrences: {message.Count}");
    }

    Console.WriteLine();
}

static HashSet<int> GetSelectedMessageNumbers(
    IReadOnlyList<AvailableMessage> availableMessages)
{
    HashSet<int> availableMessageNumbers =
        availableMessages
            .Select(message => message.MessageNumber)
            .ToHashSet();

    while (true)
    {
        Console.Write(
            "Enter message numbers separated by commas " +
            "or type 'all': ");

        string? input = Console.ReadLine();

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

        HashSet<int> selectedMessageNumbers = new();
        bool isValid = true;

        foreach (string part in parts)
        {
            if (!int.TryParse(
                    part.Trim(),
                    out int messageNumber))
            {
                isValid = false;
                break;
            }

            if (!availableMessageNumbers.Contains(messageNumber))
            {
                Console.WriteLine(
                    $"Message number {messageNumber} " +
                    "was not found in the file.");

                isValid = false;
                break;
            }

            selectedMessageNumbers.Add(messageNumber);
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

static string GetOutputFormat()
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("Available output formats:");
        Console.WriteLine("1. CSV");

        Console.Write("Select an output format: ");

        string? input = Console.ReadLine();

        if (input?.Trim() == "1" ||
            string.Equals(
                input?.Trim(),
                "csv",
                StringComparison.OrdinalIgnoreCase))
        {
            return "csv";
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
        !string.IsNullOrWhiteSpace(arguments[argumentIndex]))
    {
        return arguments[argumentIndex];
    }

    while (true)
    {
        Console.Write(prompt);

        string? path = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(path))
        {
            return path.Trim();
        }

        Console.WriteLine("A file path is required.");
    }
}