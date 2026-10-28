using Dynastream.Fit;

namespace FitParser.Core;

public sealed class GeneralMessageHandler
{
    private readonly List<Mesg> _messages = new();
    private readonly List<MesgDefinition> _messageDefinitions = new();

    public IReadOnlyList<Mesg> Messages => _messages;

    public IReadOnlyList<MesgDefinition> MessageDefinitions =>
        _messageDefinitions;

    public void HandleMessage(
        object sender,
        MesgEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        _messages.Add(eventArgs.mesg);
    }

    public void HandleMessageDefinition(
        object sender,
        MesgDefinitionEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);

        _messageDefinitions.Add(eventArgs.mesgDef);
    }

    public void Clear()
    {
        _messages.Clear();
        _messageDefinitions.Clear();
    }
}
