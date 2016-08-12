#!/bin/sh
dotnet restore --verbosity minimal
dotnet build src/JustEat.StatsD
#dotnet test src/JustEat.StatsD.Tests
dotnet pack src/JustEat.StatsD
