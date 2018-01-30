# DotNetRegressionTestRunner

## What

This is a framework that lets users write tests - as simple programs
with `Main` methods - that can exercise features in the desired .NET
Core sdk/runtimes.

This is inspired by
[jtreg](https://github.com/crummel/source-build-tarball-2.0.5).

## How

Usage:

    dotnet run <path to dotnet sdk> <path to dir containing tests>

For example:

    $ dotnet run --project RedHat.DotNet.DotNetRegressionTestRunner $(dirname $(readlink -f $(which dotnet))) temp/

## Why

This lets tests conditionally compile and run for selected SDK and
Runtime versions - it is okay to write tests that require API only
available in .NET Core 2.1 and run the tests against a .NET Core 2.0.
The framework will skip tests that are not applicable to the targetted
runtime.

Unlike *xUnit* and similar frameworks, the tests written here are
expected to be **dirty**. They may even crash the VM.
