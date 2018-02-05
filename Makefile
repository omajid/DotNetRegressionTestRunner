
all: build

run-samples:
	dotnet run --project RedHat.DotNet.DotNetRegressionTestRunner Samples $$(dirname $$(readlink -f $$(which dotnet)))

build:
	# dotnet build -c Release RedHat.DotNet.DotNetRegressionTestRunner
	dotnet build -c Release RedHat.DotNet.DotNetRegressionTestRunner.Tests

check:
	dotnet test RedHat.DotNet.DotNetRegressionTestRunner.Tests

clean:
	rm -rf obj
	rm -rf bin
	rm -rf RedHat.DotNet.DotNetRegressionTestRunner/obj
	rm -rf RedHat.DotNet.DotNetRegressionTestRunner/bin
	rm -rf RedHat.DotNet.DotNetRegressionTestRunner.Tests/obj
	rm -rf RedHat.DotNet.DotNetRegressionTestRunner.Tests/bin
	rm -rf dotnetreg.*
