FitFileChecker.cs 
receives file from input, verifies the file can be opened and is a valid FIT file.


FitFileDecoder.cs 
receives fit file from Checker, uses Garmin’s Decode class from binary to read the file 
and extracts messages. 


MessageBroadcaster.cs 
receives decoded messages, wraps and configures Garmin’s MesgBroadcaster, identifies 
each message and sends it to the correct handler.


GeneralMessageHandler.cs
receives messages from the Broadcaster, organizes and stores it. 


MessageNormalizer.cs
receives messages from Handler, converts SDK messages into our consistent format, 
easier to process any message type if theyre in one format.


MessageCollection.cs
receives normalized message, stores the normalized messages for further processing. 
