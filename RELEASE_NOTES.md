#### 1.4.0 September 21st 2026 ####

* WebApi template: migrated to .NET Aspire for local development and orchestration. A new `WebApiTemplate.AppHost` project replaces the previous Docker Compose and Azurite setup, and the template now targets .NET 10.
* WebApi template: added a `--Discovery` option to choose the Akka.Cluster discovery backend, either Redis (default) or Azure Table Storage, so a generated app only references the packages for the backend it uses.
* WebApi template: replaced the deprecated Akka.HealthChecks package with the built-in Akka.Hosting health checks, migrated the solution to the `.slnx` format, and updated the test project to xUnit v3.
* WebApi template: upgraded Akka.NET, Akka.Hosting, and Akka.Management to v1.5.70 and added the Akka.Aspire integration.
* Console and Streams templates (C# and F#): upgraded Akka.Hosting and Akka.Streams to v1.5.71 and standardized Microsoft.Extensions.Hosting on v8.0.1.

#### 1.3.1 January 26th 2026 ####

* Upgraded Akka.NET dependency to v1.5.59
* Upgraded Akka.Hosting dependency to v1.5.59

#### 1.3.0 November 3rd 2025 ####

* Upgraded Akka.NET dependency to v1.5.55

#### 1.2.0 February 25th 2025 ####

* Added F# template support for the [Akka.Streams template](https://github.com/akkadotnet/akkadotnet-templates/blob/dev/docs/AkkaStreamsTemplate.md) - see the docs for an example
