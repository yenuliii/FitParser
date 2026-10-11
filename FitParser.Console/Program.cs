/* This will be the entry point

   1. it will receive input Fit file
   2. call decoder 
   3. call exporter 
   4. receiver out CSV path 
   5. report success or erros 

   ** Keep this simple, only coordinate the work, NO LOGIC! 
*/

using FitParser.Core;

Console.WriteLine("FIT File Parser");
Console.WriteLine("----------------");

string inputFilePath = GetPath(
    args,
    0,
    "Enter the path to the input FIT file: ");

string outputFilePath = GetPath(
    args,
    1,
    "Enter the path for the output CSV file: ");

try
{
    // Activity and monitoring files can sometimes be partially recovered.
    // Set this to true for files that require complete integrity validation.
    const bool checkIntegrity = false;

    FitFileChecker checker = new FitFileChecker();

    var validationResult =
        checker.Validate(inputFilePath, checkIntegrity);

    if (!validationResult.IsValid)
    {
        Console.WriteLine();
        Console.WriteLine("The FIT file is invalid.");
        Console.WriteLine(validationResult.ErrorMessage);
        return;
    }

    GeneralMessageHandler handler =
        new GeneralMessageHandler();

    MessageBroadcaster broadcaster =
        new MessageBroadcaster(handler);

    FitFileDecoder decoder =
        new FitFileDecoder(broadcaster);

    decoder.DecodeFile(inputFilePath);

    MessageNormalizer normalizer =
        new MessageNormalizer();

    MessageCollection collection =
        new MessageCollection();

    foreach (var rawMessage in handler.Messages)
    {
        NormalizedMessage normalizedMessage =
            normalizer.Normalize(rawMessage);

        collection.Add(normalizedMessage);
    }

    string? outputDirectory =
        Path.GetDirectoryName(outputFilePath);

    if (!string.IsNullOrWhiteSpace(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
    }

    collection.SaveAsCsv(outputFilePath);

    Console.WriteLine();
    Console.WriteLine("FIT file decoded successfully.");
    Console.WriteLine($"Messages decoded: {collection.Count}");
    Console.WriteLine($"Output written to: {outputFilePath}");
}
catch (Exception exception)
{
    Console.WriteLine();
    Console.WriteLine("The FIT file could not be processed.");
    Console.WriteLine($"Error: {exception.Message}");
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
