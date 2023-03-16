# Akka.NET WebApi Template

This template is designed to integrate [Akka.NET](https://getakka.net/) with ASP.NET Web APIs.

See https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/WebApiTemplate.md for complete and current documentation on this template.

## Key HTTP Routes

* https://localhost:{ASP_NET_PORT}/swagger/index.html - Swagger endpoint for testing out Akka.NET-powered APIs
* https://localhost:{ASP_NET_PORT}/healthz/akka - Akka.HealthCheck HTTP endpoint

## Petabridge.Cmd Support

This project is designed to work with [Petabridge.Cmd](https://cmd.petabridge.com/). For instance, if you want to check with the status of your Akka.NET Cluster, just run:

```shell
pbm cluster show
```

> NOTE: Petabridge.Cmd binds to [0.0.0.0:9110] on all hosts by default - if you launch multiple instances of this application on the same host you'll see "socket already in use" exceptions raised by the .NET runtime. These are fine - it just means that we can't open Petabridge.Cmd again on that process.
> 
> You can configure the Petabridge.Cmd host to run on port 0 if you want it to be accessible across multiple instances on the same host.