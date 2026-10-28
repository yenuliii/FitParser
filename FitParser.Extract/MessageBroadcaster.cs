using Dynastream.Fit;

using GarminMessageBroadcaster =
    Dynastream.Fit.MesgBroadcaster;

namespace FitParser.Core;

public sealed class MessageBroadcaster
{
    private readonly GarminMessageBroadcaster _garminBroadcaster;
    private readonly GeneralMessageHandler _messageHandler;

    public MessageBroadcaster(GeneralMessageHandler messageHandler)
    {
        ArgumentNullException.ThrowIfNull(messageHandler);

        _messageHandler = messageHandler;
        _garminBroadcaster = new GarminMessageBroadcaster();

        _garminBroadcaster.MesgEvent +=
            _messageHandler.HandleMessage;

        _garminBroadcaster.MesgDefinitionEvent +=
            _messageHandler.HandleMessageDefinition;
    }

    public void ConnectTo(Decode decoder)
    {
        ArgumentNullException.ThrowIfNull(decoder);

        decoder.MesgEvent += _garminBroadcaster.OnMesg;
        decoder.MesgDefinitionEvent +=
            _garminBroadcaster.OnMesgDefinition;
    }

    public void DisconnectFrom(Decode decoder)
    {
        ArgumentNullException.ThrowIfNull(decoder);

        decoder.MesgEvent -= _garminBroadcaster.OnMesg;
        decoder.MesgDefinitionEvent -=
            _garminBroadcaster.OnMesgDefinition;
    }
}
