# JustEat.StatsD

## The point

### TL;DR

We use this within our components to publish [statsd](http://github.com/etsy/statsd) metrics from .Net code.

We've been using this in most of our things, since around 2013.

### Features

* statsd metrics formatter
* UDP client handling

#### Publishing statistics

`IStatsDPublisher` is the interface that you will use in most circumstances. With this you can `Increment` or `Decrement` an event, and send values for a `Gauge` or `Timing`.

The concrete class that implements `IStatsDPublisher` is `StatsDImmediatePublisher`. For the constructor parameters, you will need the statsd server host name. You can change the standard port (8125). You can also append a prefix to all stats. These values often come from configuration as the host name and/or prefix may vary between test and production environments.

#### Example of setting up a StatsDPublisher

An example of Ioc in NInject for statsd publisher with values from configuration:
```csharp
	string statsdHostName =  ConfigurationManager.AppSettings["statsd.hostname"];
	int statsdPort = int.Parse(ConfigurationManager.AppSettings["statsd.port"]);
	string statsdPrefix =  ConfigurationManager.AppSettings["statsd.prefix"];
		
	Bind<IStatsDPublisher>().To<StatsDImmediatePublisher>()
        .WithConstructorArgument("cultureInfo", CultureInfo.InvariantCulture)
		.WithConstructorArgument("hostNameOrAddress",statsdHostName)
        .WithConstructorArgument("port", statsdPort)
        .WithConstructorArgument("prefix", statsdPrefix);

```

#### Example of using the interface

Given an existing instance of `IStatsDPublisher` called `stats` you can do for e.g.:

```csharp
		stats.Increment("DoSomething-Attempt");
		var stopWatch = Stopwatch.StartNew();
        var success = DoSomething();

		stopWatch.Stop();
		if (success)
        {
			stats.Timing("DoSomething-Success", stopWatch.Elapsed);
		}
```

#### Simple timers

This syntax for timers less typing in simple cases, where you always want to time the operation, and always with the same stat name. Given an existing instance of `IStatsDPublisher` you can do:

```csharp
    //  timing a block of code in a using statement:
   using (stats.StartTimer("someStat"))
   {
      DoSomething();
   }
```
 
The `StartTimer` returns an `IDisposable` that wraps a stopwatch. The stopwatch is automatically stopped and the metric sent when it falls out of scope on the closing `}` of the using statement.
 
```csharp
   //  timing a lambda without a return value:
   stats.Time("someStat", () => DoSomething());

    //  timing a lambda with a return value:
    var result = stats.Time("someStat", () => GetSomething());

    // works with async
    using (stats.StartTimer("someStat"))
    {
        await DoSomethingAsync();
    }

    // and correctly times async lambdas using the usual syntax:
    await stats.Time("someStat", async () => await DoSomethingAsync());
    var result = await stats.Time("someStat", async () => await GetSomethingAsync());
    
```

The idea of "disposable timers" for using statements comes from [this StatsD client](https://github.com/Pereingo/statsd-csharp-client).


### How to contribute

See [CONTRIBUTING.md](CONTRIBUTING.md).

### How to release
See [CONTRIBUTING.md](CONTRIBUTING.md).

