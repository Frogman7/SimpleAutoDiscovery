# SimpleAutoDiscovery (or **SAD** for short)

## Description
A C# NetStandard 2.0 library solution for implementing a lightweight auto discovery server and client

## Usage

Please see the "ExampleClient" and "ExampleServer" projects included with the source code for a complete example.

### Server
#### Setup and Starting
```csharp
// Creates a new auto discovery server that will receive messages on port 8756, reply on port 8757 with the
// the message "Hello there!" and will not setup the socket for reuse
var sadServer = new SADServer.AutoDiscoveryServer(8756, 8757, Encoding.ASCII.GetBytes("Hello there!"), false);

// Starts the server listening for broadcasts
sadServer.StartListening();
```

#### Stopping
```csharp
sadServer.StopListening();
```


### Client
#### Create Server Information class
For now you'll need to create your own implementation of the ServerInformationBase class depending on your needs.
```csharp
// Custom Server Information inheriting from ServerInformationBase
{
  // The message received from the server as a string
  public string ServerMessage { get; }

  public CustomServerInfo(IPEndPoint serverEndpoint, byte[] message) : base(serverEndpoint, message)
  {
  	this.ServerMessage = Encoding.ASCII.GetString(message);
  }
}
```

#### Finding servers
```csharp
// Creates a new auto discovery client that will send broadcasts on port 8756, receive responses on port 8757,
// and does not setup the socket for port reuse.
var sadClient = new SADClient.AutoDiscoveryClient<CustomServerInfo>(8756, 8757, false);

// Searches for servers synchronously for 1000 milliseconds and broadcasts the message "Anyone" and returns
// a collection of CustomServerInfos representing the servers found.
IEnumerable<CustomServerInfo> servers = sadClient.FindServers(1000, Encoding.ASCII.GetBytes("Anyone?"));
```


## Development Status
This is more or less a 'proof' of concept at this point, has minimal testing from me, and definately shouldn't be considered production ready.

#### Planned Features
- Asynchronous support on client when searching for servers
- Support for UDP multicasting
- Nuget package
