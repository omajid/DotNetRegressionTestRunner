# Overview

This framework provides a way to run regression tests. The goal is
that the user has a separate repository of regression tests and runs
this framework against it:

    $ dotnetreg /path/to/dotnet/to/test /path/to/tests

The tests look like standalone programs, and can be placed anywhere
under the `/path/to/tests` root.

Here is a simple test:

    using System;
    using System.IO;

    // <test/>
    // <requires runtime=">2.0"/>
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

A `<test>` marker indicates that this is a test for this framework.
Other files will not be executed.

`<requires>` places constraints on which runtime version this test
will be compiled and executed for. This framework will only run tests
that target the runtime provided with `/path/to/dotnet/to/test`.

Each test will be compiled and executed in a separate process. It is
okay for tests to do something that crashes the CLR. A test that exits
normally will count as a successful test. A test that exits with an
error code, throws an exception or crashes the CLR will count as a
failed test.
