#!/bin/sh
dotnet restore --verbosity minimal
dotnet build src/JustEat.StatsD --framework "netstandard1.3"
