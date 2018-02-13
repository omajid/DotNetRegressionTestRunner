using Xunit;

namespace RedHat.DotNet.DotNetRegressionTestRunner.Tests
{
    public class ArgumentsAndOptionsTests
    {
        [Fact]
        public void FailsToParseWhenThereAreNoArguments()
        {
            string[] args = {};
            var parsed = ArgumentsAndOptions.Parse(args);
            Assert.Null(parsed);
        }

        [Fact]
        public void OnlyRequiredArgumentIsTestRoot()
        {
            string[] args = { "/foo/bar" };
            var parsed = ArgumentsAndOptions.Parse(args);
            Assert.Equal(args[0], parsed.TestRoot);
        }

        [Fact]
        public void VerboseOptionIsParsedCorrectly()
        {
            string[] args = { "--verbose", "root" };
            var parsed = ArgumentsAndOptions.Parse(args);
            Assert.Equal(true, parsed.Verbose);
        }

        [Fact]
        public void OptionalDotNetPathIsParsedCorrectly()
        {
            string[] args = { "root", "/bin/" };
            var parsed = ArgumentsAndOptions.Parse(args);
            Assert.Equal(args[1], parsed.DotNetHome);
        }
    }
}
