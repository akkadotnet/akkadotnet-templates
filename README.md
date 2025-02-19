# akkadotnet-templates

![Akka.NET logo](https://raw.githubusercontent.com/akkadotnet/akkadotnet-templates/dev/logo.png)

Production-ready `dotnet new` templates for [Akka.NET](https://getakka.net/).

All of these templates are designed to be simple and provide you with a relatively complete structure to get started developing your own Akka.NET applications from scratch.

**Upon installing these templates via the `dotnet` CLI, you will have access to them from both the `dotnet` CLI itself and as "new project" template options in any .NET IDE - such as Visual Studio and JetBrains Rider!**

## Installation

To install these templates, just install the `Akka.Templates` package from NuGet:

```shell
dotnet new install "Akka.Templates::*"
```

To upgrade these templates to a newer version:

```shell
dotnet new update
```

To uninstall these templates from your local machine:


```shell
dotnet new uninstall Akka.Templates
```

## Available Templates

The following templates are available as part of the `Akka.Templates` package:

| Template     | Short Name  | Description                                                                                                                                                                                                                                                              | Languages |
|--------------|-------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-----------|
| [Akka.Cluster.WebApi](https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/WebApiTemplate.md)  | akka.cluster.webapi  | A template for building ASP.NET HTTP APIs on top of an Akka.NET Cluster. Uses Akka.Cluster.Sharding and, optionally: Akka.Management + Akka.Persistence.Azure + Akka.Azure.Discovery. This template is meant as a starter for building distributed systems with Akka.NET | C# |
| [Akka.Console](https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/ConsoleTemplate.md) | akka.console | This is a simple template designed to incorporate local [Akka.NET](https://getakka.net/) into a console application.                                                                                                                                                     | C#, F# |
| [Akka.Streams](https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/AkkaStreamsTemplate.md) | akka.streams | This is a simple template designed to incorporate [Akka.NET](https://getakka.net/)'s [Akka.Streams APIs](https://getakka.net/articles/streams/introduction.html) into a local console template.                                                                          | C# |

See [the official `dotnet new` documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) for more information on the sorts of options that are available when using project templates.

## Questions, Comments, and Suggestions
We accept pull requests! Please let us know what we can do to make these templates more useful, extensible, or easier to use.
