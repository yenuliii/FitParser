/* 
1. List the message types available in MessageCollection.
2. Return a new MessageCollection containing only the selected messages.

Finds which message types exist.
Groups messages by MessageNumber.
Reports message names and occurrence counts.
Returns a new collection containing only selected message types.

*/ 

namespace FitParser.Select;

using FitParser.Models;

public sealed record AvailableMessage(
    int MessageNumber,
    string MessageName,
    int Count);

public sealed class MessageSelector
{
    public IReadOnlyList<AvailableMessage>
        GetAvailableMessages(
            MessageCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return collection.Messages
            .GroupBy(message => message.MessageNumber)
            .Select(group =>
            {
                NormalizedMessage firstMessage =
                    group.First();

                return new AvailableMessage(
                    firstMessage.MessageNumber,
                    firstMessage.MessageName,
                    group.Count());
            })
            .OrderBy(message => message.MessageName)
            .ThenBy(message => message.MessageNumber)
            .ToList()
            .AsReadOnly();
    }

    public MessageCollection SelectMessages(
        MessageCollection collection,
        IReadOnlySet<int> selectedMessageNumbers)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(
            selectedMessageNumbers);

        MessageCollection selectedMessages = new();

        IEnumerable<NormalizedMessage> matchingMessages =
            collection.Messages
                .Where(message =>
                    selectedMessageNumbers.Contains(
                        message.MessageNumber));

        selectedMessages.AddRange(
            matchingMessages);

        return selectedMessages;
    }
}