# JDial

This is a C# port of JDial. I haven't tested it and I'm probably not going to use it, but maybe it will help someone else.

----------------------------------------

DIAL allows second screen devices (smartphone, laptop, ...) to discover server instances in the local network and 
launch applications on a first screen device (smart TV).

For additional information about the protocol see [Wikipedia](https://en.wikipedia.org/wiki/Discovery_and_Launch) 
and [dial-multiscreen.org](http://www.dial-multiscreen.org).

A list of reserved application names can also be found on the [dial-multiscreen.org](http://www.dial-multiscreen.org/dial-registry/namespace-database) site.

# Usage

## Discover

```
IEnumerable<DialServer> devices = new Discovery().discover();
```

## Creat a DialClientConnection

```
DialServer dialServer = devices.get(0);
DialClient dialClient = new DialClient();

DialClientConnection tv = dialClient.connectTo(dialServer);
```

## Discover applications

```
Application youtube = tv.getApplication(Application.YOUTUBE);
```

## Start applications

```
tv.startApplication(youtube);
```

## Stop applications

```
tv.stopApplication(youtube);
```

## Implement application vendor protocol
```

 class MyCustomContent : DialContent {
 
     public String getContentType() {
         return "application/json; encoding=UTF-8";
     }

     public byte[] getData() {
          return Encoding.UTF8.GetBytes("{}");
     }
 };


myTv.startApplication(youtube, new MyCustomContent())
```

## Legacy support

Some server implementations are not compatible with current versions of the DIAL protocol.
For example some LG TVs support DIAL, but the server implementation can't handle query parameters.
By creating a ProtocolFactoryImpl and setting the `legacyCompatibility` flag the client doesn't set any query parameter.

```
bool supportLegacyDevices = true;
ProtocolFactory factory = new ProtocolFactoryImpl(supportLegacyDevices);
DialClient dialClient = new DialClient(factory);
```

## Logging

Logging is done via NLog.
