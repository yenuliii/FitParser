/* 
1. List the fields available in each message type of MessageCollection.
2. Returns a new MessageCollection containing only the selected fields.

Finds all fields inside the selected messages.
Reports field names, numbers, and occurrence counts.
Selects individual fields.
Creates a new collection containing only those fields.
Preserves each message’s MessageIndex.

*/ 

namespace FitParser.Select;

using FitParser.Models;

public sealed record FieldKey(
    int MessageNumber,
    int FieldNumber);

public sealed record AvailableField(
    int MessageNumber,
    string MessageName,
    int FieldNumber,
    string FieldName,
    int Count);

public sealed class FieldSelector
{
    public IReadOnlyList<AvailableField> GetAvailableFields(
        IEnumerable<NormalizedMessage> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        return messages
            .SelectMany(message =>
                message.Fields.Select(field => new
                {
                    Message = message,
                    Field = field
                }))
            .GroupBy(item => new FieldKey(
                item.Message.MessageNumber,
                item.Field.FieldNumber))
            .Select(group =>
            {
                var first = group.First();

                return new AvailableField(
                    first.Message.MessageNumber,
                    first.Message.MessageName,
                    first.Field.FieldNumber,
                    first.Field.FieldName,
                    group.Count());
            })
            .OrderBy(field => field.MessageName)
            .ThenBy(field => field.MessageNumber)
            .ThenBy(field => field.FieldNumber)
            .ToList()
            .AsReadOnly();
    }

    public MessageCollection SelectFields(
        IEnumerable<NormalizedMessage> messages,
        IReadOnlySet<FieldKey> selectedFieldKeys)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(selectedFieldKeys);

        MessageCollection selectedMessages = new();

        foreach (NormalizedMessage message in messages)
        {
            IReadOnlyList<NormalizedField> matchingFields =
                message.Fields
                    .Where(field =>
                        selectedFieldKeys.Contains(
                            new FieldKey(
                                message.MessageNumber,
                                field.FieldNumber)))
                    .ToList()
                    .AsReadOnly();

            if (matchingFields.Count == 0)
            {
                continue;
            }

            NormalizedMessage filteredMessage =
                new(
                    message.MessageName,
                    message.MessageNumber,
                    message.MessageIndex,
                    matchingFields);

            selectedMessages.Add(filteredMessage);
        }

        return selectedMessages;
    }

    public MessageCollection SelectFields(
        MessageCollection collection,
        IReadOnlySet<FieldKey> selectedFieldKeys)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return SelectFields(
            collection.Messages,
            selectedFieldKeys);
    }
}