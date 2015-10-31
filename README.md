# JustEat.StatsD

## The point

### TL;DR

We use this within our components to publish [statsd](http://github.com/etsy/statsd) metrics.

### In detail

There's really not much else to say :-)

We've been using this in most of our things, mostly unchanged, since around 2013.

### Features

* statsd metrics formatter
* UDP client handling

####

`IStatsDPublisher` is the interface that you will use in most circumstances. Use an instance of this in order to send events, timers and gauges.

The concrete class that implements is `IStatsDPublisher` is `StatsDImmediatePublisher`. For the constructor parameters, you will need to statsd server host name. You can also append a prefix to all stats. Often one of both of these values vary between test and producion environments.

example of Ioc in NInject for statsd publisher with values from config:
```csharp
	string statsdHostName = GetConfigValue("statsd.hostname");
	int statsdPort = GetConfigValueInt("statsd.hostname");
	string statsdPrefix = GetConfigValue("statsd.hostname");
		
	Bind<IStatsDPublisher>().To<StatsDImmediatePublisher>()
        .WithConstructorArgument("cultureInfo", CultureInfo.InvariantCulture)
		.WithConstructorArgument("hostNameOrAddress",statsdHostName)
        .WithConstructorArgument("port", statsdPort)
        .WithConstructorArgument("prefix", statsdPrefix);

```
####
Timing with the interface. Given an existing instance of `IStatsDPublisher` you can do:

```csharp
		var stopWatch = Stopwatch.StartNew();

        var success = DoSomething();

		stopWatch.Stop();
		if (success)
        {
			stats.Timing("DoSomething.Success", stopWatch.Elapsed);
		}
```

####
Simple timers. 

This syntax is simpler; for cases where you always want to time the operation, and always with the same stat name. Given an existing instance of `IStatsDPublisher` you can do:

```csharp
    //  timing a block of code in a using statement:
   using (stats.StartTimer("someStat"))
   {
      DoSomething();
   }
 
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

