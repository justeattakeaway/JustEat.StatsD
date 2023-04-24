# CHANGELOG

## 3.0.0

See <https://github.com/justeattakeaway/JustEat.StatsD/releases/tag/v3.0.0>.

## 2.1.0

See <https://github.com/justeattakeaway/JustEat.StatsD/releases/tag/v2.1.0>.

## 2.0.0

See <https://github.com/justeattakeaway/JustEat.StatsD/releases/tag/v2.0.0>.

## 1.1.0

Add support for .NET Core.

## 1.0.4

### Change

Easy to use timers. Time a block of code with a `using` statement or time a lambda, with or without a return value. `async ... await` is also supported.

Usage: given an existing instance of `IStatsDPublisher` called `stats` you can do:

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
var result = await stats.Time("someStat", async () => await GetSomethingAsync());
```

The idea of "disposable timers" comes from [this StatsD client](https://github.com/Pereingo/statsd-csharp-client).

## 1.0.3

### Changes

NuGet metadata

## 1.0.2

### Changes

* Move to Sys.Diag.Trace for logs to remove NLog dependency (so as to not force that on dependents)

## 1.0.0

### Add

* We've been in production with this for a while now.  It deserves the 1.0 version tag.
* Sort out references and dependencies such that nuget is relied upon.

## 0.0.2

### Add

* Ability to prefix each stat, so the prefix/namespace can be supplied one-time via configuration rather than built each time a stat is pushed.  Eg, Mailman would want every stat to be prefixed with `{country}.mailman_{instance position}`

## 0.0.1

* First release
