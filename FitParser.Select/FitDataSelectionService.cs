/* 
Cordinates work between the program.cs and the message and field selectors 
*/ 

namespace FitParser.Select;

using FitParser.Models;

public sealed class FitDataSelectionService
{
    private readonly MessageSelector _messageSelector;
    private readonly FieldSelector _fieldSelector;

    public FitDataSelectionService()
    {
        _messageSelector = new MessageSelector();
        _fieldSelector = new FieldSelector();
    }

    public IReadOnlyList<AvailableMessage>
        GetAvailableMessages(
            MessageCollection collection)
    {
        return _messageSelector
            .GetAvailableMessages(collection);
    }

    public MessageCollection SelectMessages(
        MessageCollection collection,
        IReadOnlySet<int> selectedMessageNumbers)
    {
        return _messageSelector
            .SelectMessages(
                collection,
                selectedMessageNumbers);
    }

    public IReadOnlyList<AvailableField>
        GetAvailableFields(
            MessageCollection collection)
    {
        return _fieldSelector
            .GetAvailableFields(
                collection.Messages);
    }

    public MessageCollection SelectFields(
        MessageCollection collection,
        IReadOnlySet<FieldKey> selectedFieldKeys)
    {
        return _fieldSelector
            .SelectFields(
                collection,
                selectedFieldKeys);
    }
}