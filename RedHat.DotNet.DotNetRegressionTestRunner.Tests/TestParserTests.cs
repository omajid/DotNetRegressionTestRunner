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
        public void TestHeaderIsParsedCorrectly()
        {
            var extractedHeader = "<test><requires runtime=\"[1.0,2.0)\" /></test>";

            var extracted = TestParser.ParseExtractedTestHeader(extractedHeader);

            var expected = new TestHeader
            {
                TargetRuntimeVersion = new VersionRange
                {
                    MinVersion = new Version("1.0"),
                    MinVersionInclusive = true,
                    MaxVersion = new Version("2.0"),
                    MaxVersionInclusive = false,
                },
            };

            Assert.Equal(expected.TargetRuntimeVersion.MinVersion, extracted.TargetRuntimeVersion.MinVersion);
            Assert.Equal(expected.TargetRuntimeVersion.MinVersionInclusive, extracted.TargetRuntimeVersion.MinVersionInclusive);
            Assert.Equal(expected.TargetRuntimeVersion.MaxVersion, extracted.TargetRuntimeVersion.MaxVersion);
            Assert.Equal(expected.TargetRuntimeVersion.MaxVersionInclusive, extracted.TargetRuntimeVersion.MaxVersionInclusive);
        }
    }   
}
