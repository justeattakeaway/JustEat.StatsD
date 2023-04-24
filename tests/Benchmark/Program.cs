#pragma warning disable CA1812

using BenchmarkDotNet.Running;

BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args);
