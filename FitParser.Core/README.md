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

TODO: 
// so our goal is to be able to select what data we want to extract and between what time periods 
// so our next step would be getting the message extracted in a way we can do this / right here \ 
next we will work on moving the CSV exported out of MessageNormalizer 
and also implementing logic to show units, correct timestamps and be able to select the data you want

right now the everything from the files are extracted we have message index, message name, 
message number, field name, field number, c# data type, value
and we should keep it like that, im even wondering whether i should delete message name and field name 
cause they can be derived from their respective numbers. 

None the less lets keep this modular and abstract and do all the further processing and extraction i t
alked about before in the exporter! Should I name the FitParser.Core to FitParser.Extracter? cause thats what it does? 
