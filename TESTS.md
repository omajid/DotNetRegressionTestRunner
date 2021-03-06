# Writing Tests

This framework provides a way to run regression tests. The goal is
that the user has a separate repository of regression tests and runs
this framework against it. The tests look like standalone programs,
and can be placed anywhere under the `/path/to/tests` root.

Each test will be compiled and executed in a separate process. It is
okay for tests to do something that crashes the CLR. A test that exits
normally will count as a successful test. A test that exits with an
error code, throws an exception or crashes the CLR will count as a
failed test.

Here is a simple test:

    // <test>
    // <requires runtime="[,2.0)"/>
    // <compile framework="netcoreapp2.0"/>
    // <compile configuration="Debug"/>
    // </test>

    using System;
    using System.IO;

    namespace Foo
    {
        class Bar
        {
            static int Main()
            {
                Console.WriteLine("Hello World!");
            }
        }
    }

A comment block must be the first set of lines in file. It must
contain a `<test>` element.

A `<test>` element indicates that this is a test for this framework.
Other files will not be executed.

`<requires>` places constraints on which runtime version this test
will be compiled and executed for. This framework will only run tests
that target a runtime available in the `dotnet` being tested. You can
specify a range here with the syntax `[2.0, 2.1]` to indicate that it
should only be run against runtimes with version 2.0 to 2.1,
inclusive. The range is specified in [interval
notation](https://en.wikipedia.org/wiki/Interval_(mathematics)#Notations_for_intervals),
with a missing value implying unbounded. For example, `[2.0,)` implies
that 2.0 or any later version is acceptable.

`<compile>` allows selecting how the program is compiled.
`configuration` can be `Debug` or `Release`. `framework` can be any
known framework to target, such as `netcoreapp2.0`.

There are many tests in the `Samples` directory that also serve as
examples.
