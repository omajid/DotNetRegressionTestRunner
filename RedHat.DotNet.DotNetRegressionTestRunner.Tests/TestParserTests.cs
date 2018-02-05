using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RedHat.DotNet.DotNetRegressionTestRunner.Tests
{
    public class TestParserTests
    {

        [Fact]
        public void FirstCommentIsExtractedCorrectly()
        {
            var file =
@"// foo
// bar

";

            var extracted = TestParser.GetFirstComment(new StringReader(file));
            Assert.Equal(new string[] {"// foo", "// bar" }, extracted);
        }

        [Fact]
        public void OnlyFirstCommentIsExtracted()
        {
            var file =
@"// foo
// bar

// baz
";

            var extracted = TestParser.GetFirstComment(new StringReader(file));
            Assert.Equal(new string[] {"// foo", "// bar" }, extracted);
        }

        [Fact]
        public void OnlyCommentIsExtracted()
        {
            var file =
@"// foo
// bar

using System;
";

            var extracted = TestParser.GetFirstComment(new StringReader(file));
            Assert.Equal(new string[] {"// foo", "// bar" }, extracted);
        }

        [Fact]
        public void TestHeaderIsExtractedCorrectly()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test>");
            commentLines.Add("// </test>");

            var extracted = TestParser.ExtractTestHeader(commentLines);
            Assert.Equal("<test></test>", extracted);
        }

        [Fact]
        public void SelfClosedTestElementIsExtractedCorrectly()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test/>");

            var extracted = TestParser.ExtractTestHeader(commentLines);
            Assert.Equal("<test/>", extracted);
        }

        [Fact]
        public void MalformedTestHeaderIsNotExtracted()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test>");

            var extracted = TestParser.ExtractTestHeader(commentLines);
            Assert.Equal(null, extracted);
        }

        [Fact]
        public void TestHeaderWithRequiresIsExtractedCorrectly()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test>");
            commentLines.Add("// <requires runtime=\"<2.0\" />");
            commentLines.Add("// </test>");

            var extracted = TestParser.ExtractTestHeader(commentLines);
            Assert.Equal("<test><requires runtime=\"<2.0\" /></test>", extracted);
        }

        [Fact]
        public void DefaultTargetRuntimeVersionIsAll()
        {
            var header = "<test/>";
            var extracted = TestParser.ParseExtractedTestHeader(header);
            var targetVersion = extracted.TargetRuntimeVersion;

            Assert.Equal(new SemVersion(0,0), targetVersion.MinVersion);
            Assert.Equal(new SemVersion(int.MaxValue,int.MaxValue), targetVersion.MaxVersion);
        }

        [Fact]
        public void RuntimeInTestHeaderIsParsedCorrectly()
        {
            var header = "<test><requires runtime=\"[1.0,2.0)\" /></test>";

            var extracted = TestParser.ParseExtractedTestHeader(header);

            var minVersion = new SemVersion(1, 0);
            var minVersionInclusive = true;
            var maxVersion = new SemVersion(2, 0);
            var maxVersionInclusive = false;

            Assert.Equal(minVersion, extracted.TargetRuntimeVersion.MinVersion);
            Assert.Equal(minVersionInclusive, extracted.TargetRuntimeVersion.MinVersionInclusive);
            Assert.Equal(maxVersion, extracted.TargetRuntimeVersion.MaxVersion);
            Assert.Equal(maxVersionInclusive, extracted.TargetRuntimeVersion.MaxVersionInclusive);
        }

        [Fact]
        public void DefaultConfigurationIsDebug()
        {
            var header = "<test/>";
            var extracted = TestParser.ParseExtractedTestHeader(header);
            Assert.Equal("Debug", extracted.Configuration);
        }

        [Fact]
        public void ConfigurationInTestHeaderIsParsedCorrectly()
        {
            var header = "<test><compile configuration=\"release\"/></test>";
            var extracted = TestParser.ParseExtractedTestHeader(header);
            Assert.Equal("Release", extracted.Configuration);
        }

        [Fact]
        public void UnknownConfigurationInTestHeaderCausesException()
        {
            var header = "<test><compile configuration=\"foobar\"/></test>";
            Assert.Throws(typeof(Exception), () => TestParser.ParseExtractedTestHeader(header));
        }

        [Fact]
        public void DefaultTargetFrameworkIsNetCoreApp()
        {
            var header = "<test/>";
            var extracted = TestParser.ParseExtractedTestHeader(header);
            Assert.Equal("netcoreapp2.0", extracted.TargetFramework);
        }

        [Fact]
        public void TargetFrameworkIsParsedCorrectly()
        {
            var header = "<test><compile framework=\"foobar\" /></test>";
            var extracted = TestParser.ParseExtractedTestHeader(header);
            Assert.Equal("foobar", extracted.TargetFramework);
        }
    }   
}
