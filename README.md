# JustEat.StatsD

[![NuGet version](https://buildstats.info/nuget/JustEat.StatsD?includePreReleases=false)](https://www.nuget.org/packages/JustEat.StatsD)
[![Build status](https://github.com/justeattakeaway/JustEat.StatsD/workflows/build/badge.svg?branch=main&event=push)](https://github.com/justeattakeaway/JustEat.StatsD/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)

[![codecov](https://codecov.io/gh/justeattakeaway/JustEat.StatsD/branch/main/graph/badge.svg)](https://codecov.io/gh/justeattakeaway/JustEat.StatsD)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/justeattakeaway/JustEat.StatsD/badge)](https://securityscorecards.dev/viewer/?uri=github.com/justeattakeaway/JustEat.StatsD)

## Summary

We use this library within our components to publish [StatsD](https://github.com/etsy/statsd) metrics from .NET code to a server. We've been using this in most of our things, since around 2013.

### Supported platforms

`JustEat.StatsD` version 5.0.0 is built for these target frameworks:

* `net462`
* `netstandard2.0`
* `net6.0`

### Features

* Easy to use.
* Robust and proven.
* Tuned for high performance and low resource usage using [BenchmarkDotNet](https://benchmarkdotnet.org/). Typically zero allocation on sending a metric on target frameworks where `Span<T>` is available.
* Works well with modern .NET apps - `async ... await`, .NET Core, .NET Standard 2.0.
* Supports standard StatsD primitives: `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports tagging on `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports sample rate for cutting down of sends of high-volume metrics.
* Helpers to make it easy to time a delegate such as a `Func<T>` or `Action<T>`, or a code block inside a `using` statement.
* Send stats over UDP or IP.
* Send stats to a server by name or IP address.

#### Publishing statistics

`IStatsDPublisher` is the interface that you will use to send stats. The concrete class that implements `IStatsDPublisher` is `StatsDPublisher`. The `StatsDPublisher` constructor takes an instance of `StatsDConfiguration`.

For the configuration's values, you will always need the StatsD server host name or IP address. Optionally, you can also change the port from the default (`8125`). You can also prepend a prefix to all stats. These values often come from configuration as the host name and/or prefix may vary between test and production environments.

It is best practice to create a `StatsDPublisher` at application start and use it for the lifetime of the application, instead of creating a new one for each usage.

## Setting up StatsD

### Simple example of setting up a StatsDPublisher

An example of a very simple StatsD publisher configuration, using the default values for most things.

#### .NET Core

Using `IServiceCollection` and the built-in DI container:

```csharp
// Registration
services.AddStatsD("metrics_server.mycompany.com");
services.AddSingleton<MyService>();

// Consumption
public class MyService
{
    public MyService(IStatsDPublisher statsPublisher)
    {
        // Use the publisher
    }
}

```

#### Simplest example

No service registration or IoC. Works for both .NET Framework and .NET Core.

```csharp
var statsDConfig = new StatsDConfiguration { Host = "metrics_server.mycompany.com" };
IStatsDPublisher statsDPublisher = new StatsDPublisher(statsDConfig);
```

### IoC Examples

#### .NET (Core)

An example of registering StatsD dependencies using `IServiceCollection`:

```csharp
services.AddStatsD(
    (provider) =>
    {
        var options = provider.GetRequiredService<MyOptions>().StatsD;

        return new StatsDConfiguration()
        {
            Host = options.HostName,
            Port = options.Port,
            Prefix = options.Prefix,
            OnError = ex => LogError(ex)
        };
    });
```

#### .NET Core (AWS Lambda functions)

An example of registering StatsD dependencies using `IServiceCollection` when using the IP transport, e.g. for an AWS Lambda function:

```csharp

services.AddStatsD(
    (provider) =>
    {
        var options = provider.GetRequiredService<MyOptions>().StatsD;

        return new StatsDConfiguration
        {
            Prefix = options.Prefix,
            Host = options.HostName,
            SocketProtocol = SocketProtocol.IP
        };
    });
```

#### .NET Framework

An example of IoC in Ninject for StatsD publisher with values for all options, read from configuration:

```csharp
// read config values from app settings
string statsdHostName =  ConfigurationManager.AppSettings["statsd.hostname"];
int statsdPort = int.Parse(ConfigurationManager.AppSettings["statsd.port"]);
string statsdPrefix =  ConfigurationManager.AppSettings["statsd.prefix"];

// assemble a StatsDConfiguration object
// since the configuration doesn't change for the lifetime of the app,
// it can be a preconfigured singleton instance
var statsDConfig = new StatsDConfiguration
{
  Host = statsdHostName,
  Port = statsdPort,
  Prefix = statsdPrefix,
  OnError = ex => LogError(ex)
};

// register with NInject
Bind<StatsDConfiguration>().ToConstant(statsDConfig>);
Bind<IStatsDPublisher>().To<StatsDPublisher>().InSingletonScope();
```

### StatsDConfiguration fields

| Name              | Type                    | Default                        | Comments                                                                                                |
|-------------------|-------------------------|--------------------------------|---------------------------------------------------------------------------------------------------------|
| Host              | `string`                |                                | The host name or IP address of the StatsD server. There is no default, this must be set.                |
| Port              | `int`                   | `8125`                         | The StatsD port.                                                                                        |
| DnsLookupInterval | `TimeSpan?`             | `5 minutes`                    | Length of time to cache the host name to IP address lookup. Only used when "Host" contains a host name. |
| Prefix            | `string`                | `string.Empty`                 | Prepend a prefix to all stats.                                                                          |
| SocketProtocol    | `SocketProtocol`, one of `Udp`, `IP`| `Udp`              | Type of socket to use when sending stats to the server.                                                 |
| TagsFormatter     | `IStatsDTagsFormatter`  | `NoOpTagsFormatter`            | Format used for tags for the different providers. Out-of-the-box formatters can be accessed using the `TagsFormatter` class. |
| OnError           | `Func<Exception, bool>` | `null`                         | Function to receive notification of any exceptions.                                                     |

`OnError` is a function to receive notification of any errors that occur when trying to publish a metric. This function should return:

* `true` if the exception was handled and no further action is needed
* `false` if the exception should be thrown

The default behaviour is to ignore the error.

#### Tagging support

Tags or dimensions are not covered by the StatsD specification. Providers supporting tags have implemented their own flavours. Some of the major providers are supported out-of-the-box from 5.0.0+ are:

* [CloudWatch](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch-Agent-custom-metrics-statsd.html).
* [DataDog](https://docs.datadoghq.com/developers/dogstatsd/datagram_shell/?tab=metrics)
* [InfluxDB](https://www.influxdata.com/blog/getting-started-with-sending-statsd-metrics-to-telegraf-influxdb/#introducing-influx-statsd).
* [Librato](https://github.com/librato/statsd-librato-backend#tags).
* [SignalFX](https://docs.signalfx.com/en/latest/integrations/agent/monitors/collectd-statsd.html#adding-dimensions-to-statsd-metrics).
* [Splunk](https://docs.splunk.com/Documentation/Splunk/8.0.5/Metrics/GetMetricsInStatsd).

```csharp
var config = new StatsDConfiguration
{
    Prefix = "prefix",
    Host = "127.0.0.1",
    TagsFormatter = TagsFormatter.CloudWatch,
};
```

##### Extending tags formatter

As tags are not part of the StatsD specification, the `IStatsDTagsFormatter` used can be extended and specified by the `StatsDConfiguration`.

The template class `StatsDTagsFormatter` can be inherited from providing the `StatsDTagsFormatterConfiguration`:

* **Prefix**: the string that will appear before the tag(s).
* **Suffix**: the string that will appear after the tag(s).
* **AreTrailing**: a boolean indicating if the tag(s) are placed at the end of the StatsD message (like it is supported by AWS CloudWatch, DataDog and Splunk) or otherwise they are right after the bucket name (like it is supported by InfluxDB, Librato and SignalFX).
* **TagsSeparator**: the string that will be placed between tags.
* **KeyValueSeparator**: the string that will be placed between the tag key and its value.

### Example of using the interface

Given an existing instance of `IStatsDPublisher` called `stats` you can do for e.g.:

```csharp
stats.Increment("DoSomething.Attempt");
var stopWatch = Stopwatch.StartNew();
var success = DoSomething();

stopWatch.Stop();

var statName = "DoSomething." + success ? "Success" : "Failure";
stats.Timing(stopWatch.Elapsed, statName);
```

### Simple timers

This syntax for timers less typing in simple cases, where you always want to time the operation, and always with the same stat name. Given an existing instance of `IStatsDPublisher` you can do:

```csharp
//  timing a block of code in a using statement:
using (stats.StartTimer("someStat"))
{
    DoSomething();
}

// also works with async
using (stats.StartTimer("someStat"))
{
    await DoSomethingAsync();
}
```

The `StartTimer` returns an `IDisposableTimer` that wraps a stopwatch and implements `IDisposable`.
The stopwatch is automatically stopped and the metric sent when it falls out of scope and is disposed on the closing `}` of the `using` statement.

#### Changing the name of simple timers

Sometimes the decision of which stat to send should not be taken before the operation completes. e.g. When you are timing http operations and want different status codes to be logged under different stats.

The `IDisposableTimer` has a `Bucket` property to set or change the stat bucket - i.e. the name of the stat. To use this you need a reference to the timer, e.g. `using (var timer = stats.StartTimer("statName"))` instead of `using (stats.StartTimer("statName"))`

The stat name must be set to a non-empty string at the end of the `using` block.

```csharp
using (var timer = stats.StartTimer("SomeHttpOperation."))
{
    var response = DoSomeHttpOperation();
    timer.Bucket = timer.Bucket + (int)response.StatusCode;
    return response;
}
```

#### Functional style

```csharp
//  timing an action without a return value:
stats.Time("someStat", t => DoSomething());

//  timing a function with a return value:
var result = stats.Time("someStat", t => GetSomething());

// and it correctly times async code using the usual syntax:
await stats.Time("someStat", async t => await DoSomethingAsync());
var result = await stats.Time("someStat", async t => await GetSomethingAsync());
```

In all these cases the function or delegate is supplied with an `IDisposableTimer t` so that the stat name can be changed if need be.

#### Credits

The idea of "disposable timers" for using statements is an old one, see for example [this StatsD client](https://github.com/Pereingo/statsd-csharp-client) and [MiniProfiler](https://miniprofiler.com/dotnet/HowTo/ProfileCode).

### How to contribute

See [CONTRIBUTING.md](.github/CONTRIBUTING.md).

### How to release

See [CONTRIBUTING.md](.github/CONTRIBUTING.md#releases).
