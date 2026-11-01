/*
*/

namespace FitParser.Models;

using System.Collections.ObjectModel;
public sealed class NormalizedMessage
{
    public string MessageName { get; }

    public int MessageNumber { get; }

    public int MessageIndex { get; }

    public IReadOnlyList<NormalizedField> Fields { get; }

    public NormalizedMessage(
        string messageName,
        int messageNumber,
        int messageIndex,
        IEnumerable<NormalizedField> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            messageName);

        ArgumentNullException.ThrowIfNull(fields);

        if (messageIndex <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(messageIndex),
                "Message index must be greater than zero.");
        }

        MessageName = messageName;
        MessageNumber = messageNumber;
        MessageIndex = messageIndex;

        Fields = new ReadOnlyCollection<NormalizedField>(
            fields.ToList());
    }
}