# JustEat.StatsD

[![NuGet version](https://buildstats.info/nuget/JustEat.StatsD?includePreReleases=false)](https://www.nuget.org/packages/JustEat.StatsD)
[![Build status](https://github.com/justeattakeaway/JustEat.StatsD/workflows/build/badge.svg?branch=main&event=push)](https://github.com/justeattakeaway/JustEat.StatsD/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)
[![codecov](https://codecov.io/gh/justeattakeaway/JustEat.StatsD/branch/main/graph/badge.svg)](https://codecov.io/gh/justeattakeaway/JustEat.StatsD)

## Summary

A library to publish [StatsD](https://github.com/etsy/statsd) metrics from .NET applications to a server.

## Features

* Easy to use.
* Robust and proven.
* Tuned for high performance and low resource usage.
* Supports standard StatsD primitives: `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports tagging on `Increment`, `Decrement`, `Timing` and `Gauge`.
* Supports sampling for cutting down of sends of high-volume metrics.
* Helpers to make it easy to time a delegate such as a `Func<T>` or `Action<T>`, or a code block inside a `using` statement.
* Send stats over UDP or IP.
* Send stats to a server by name or IP address.

## Feedback

Any feedback or issues for this library can be added to the issues in [GitHub](https://github.com/justeattakeaway/JustEat.StatsD/issues "This package's issues on GitHub.com").

## License

This package is licensed under the [Apache 2.0](https://www.apache.org/licenses/LICENSE-2.0.html "The Apache 2.0 license") license.
