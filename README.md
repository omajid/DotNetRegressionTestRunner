# DotNetRegressionTestRunner

## Overview

This is a framework that lets users write tests - as simple programs
with `Main` methods - that can exercise features in the desired .NET
Core SDK and Runtimes.

This lets tests conditionally compile and run for selected SDK and
Runtime versions - it is okay to write tests that require API only
available in .NET Core 2.1 and run the tests against a .NET Core 2.0.
The framework will skip tests that are not applicable to the targetted
runtime.

Unlike *xUnit* and similar frameworks, the tests written here are
expected to be **dirty**. They may even crash the VM. See `TESTS.md`
for more details on how to write tests. A number of sample tests are
included in the `Samples` directory.

This is inspired by
[jtreg](http://openjdk.java.net/jtreg/).

## Building

You will need a .NET Core 2.0 SDK to build this. After installing
that, run:

    make

The built product will be placed in `bin/` directory.

## Running

Usage:

    bin/dntr <path to dir containing tests> [<path to dotnet sdk>]

For example:

    bin/dntr Samples

# License

Licensed under the MIT license. Please see the LICENSE file for details.
