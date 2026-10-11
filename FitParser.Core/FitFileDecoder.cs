/*
    This class user Garmin's SDK Classes and it will be responsible for 
    1. decode the binary data 
    2. receive decoded messages 
    3. preserve the messages 
    5. report decoding errors 

    uses Decode and MesgBroadcaster
 */
 
using Dynastream.Fit;

namespace FitParser.Core;

public sealed class FitFileDecoder
{
    private readonly MessageBroadcaster _messageBroadcaster;

    public FitFileDecoder(MessageBroadcaster messageBroadcaster)
    {
        ArgumentNullException.ThrowIfNull(messageBroadcaster);

        _messageBroadcaster = messageBroadcaster;
    }

    public void DecodeFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using FileStream stream = System.IO.File.OpenRead(filePath);
        Decode decoder = new Decode();

        _messageBroadcaster.ConnectTo(decoder);

        try
        {
            // Garmin's Decode class reads the binary FIT file here.
            decoder.Read(stream);
        }
        finally
        {
            _messageBroadcaster.DisconnectFrom(decoder);
        }
    }
}
