using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RedHat.DotNet.DotNetRegressionTestRunner.Tests
{
    public class TestTests
    {

        [Fact]
        public void FirstCommentIsExtractedCorrectly()
        {
            var file =
@"// foo
// bar

";

            var extracted = Test.GetFirstComment(new StringReader(file));
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

            var extracted = Test.GetFirstComment(new StringReader(file));
            Assert.Equal(new string[] {"// foo", "// bar" }, extracted);
        }
    
        [Fact]
        public void TestHeaderIsExtractedCorrectly()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test>");
            commentLines.Add("// </test>");

            var extracted = Test.ExtractTestHeader(commentLines);
            Assert.Equal("<test></test>", extracted);
        }

        [Fact]
        public void SelfClosedTestElementIsExtractedCorrectly()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test/>");

            var extracted = Test.ExtractTestHeader(commentLines);
            Assert.Equal("<test/>", extracted);
        }

        [Fact]
        public void MalformedTestHeaderIsNotParsed()
        {
            var commentLines = new List<string>();
            commentLines.Add("// <test>");

            var extracted = Test.ExtractTestHeader(commentLines);
            Assert.Equal(null, extracted);
        }
    }   
}
