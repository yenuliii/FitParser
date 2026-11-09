An api is a server that waits for requests.

It will be connections between the frontend and backend

dotnet run --project FitParser.Api
^ this creates a small web server on my computer and it waits for requests it could work, 
(it provides services!)

So this is how it works : 

program.cs
** starts the server and gets everything setup ** 
 
endpoint 
** listens for HTTP requests and sends it to the apiservice ** 

apiservice 
** coordinates works between the services, calls all the services but doesnt have any logic **

TempoaryFileStore 
** has methods to stores the file, generates fileid, gets path, deletes file ** 

Contracts 
** contains data structures for file, message, field ** 