using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class TestHeader
    {
        public string Configuration { get; set; }
        public string TargetFramework { get; set; }
        public VersionRange TargetRuntimeVersion { get; set; }
    }

    public class TestParser
    {
        public static bool FileIsATest(FileInfo sourceFile)
        {
            var header = ParseTestHeader(sourceFile);
            return header != null;
        }

        public static bool FileTargetsAvailableRuntime(DirectoryInfo dotNetHome, FileInfo sourceFile)
        {
            var header = ParseTestHeader(sourceFile);
            var currentVersion = DotNet.GetAvailableRuntimeVersions(dotNetHome);
            return currentVersion
                .ToList()
                .Where(versionRange => header.TargetRuntimeVersion.IsInRange(versionRange))
                .Any();
        }

        public static bool FileTargetsAvailableFramework(DirectoryInfo dotNetHome, FileInfo sourceFile)
        {
            var header = ParseTestHeader(sourceFile);
            var availableFrameworks = DotNet.GetAvailableFrameworks(dotNetHome);
            return availableFrameworks
                .Where(framework => header.TargetFramework == framework)
                .Any();
        }

        public static TestHeader ParseTestHeader(FileInfo sourceFile)
        {
            var comments = GetFirstComment(sourceFile);
            var header = ParseExtractedTestHeader(ExtractTestHeader(comments));

            return header;
        }

        #region implementation details

        public static List<string> GetFirstComment(FileInfo sourceFile)
        {
            using (StreamReader reader = sourceFile.OpenText())
            {
                return GetFirstComment(reader);
            }
        }

        public static List<string> GetFirstComment(TextReader reader)
        {
            var comments = new List<string>();
            var sourceLine = "";
            while (sourceLine != null && !sourceLine.Trim().StartsWith("//"))
            {
                sourceLine = reader.ReadLine();
            }

            while (sourceLine != null && sourceLine.Trim().StartsWith("//"))
            {
                comments.Add(sourceLine.Trim());
                sourceLine = reader.ReadLine();
            }

            return comments;
        }

        public static string ExtractTestHeader(List<string> lines)
        {
            StringBuilder header = new StringBuilder();

            bool foundStart = false;

            foreach (var line in lines)
            {
                var processedLine = line.Substring("//".Length).Trim();

                if (foundStart)
                {
                    var endIndex = processedLine.IndexOf("</test>");
                    if (endIndex == -1)
                    {
                        header.Append(processedLine);
                    }
                    else
                    {
                        header.Append(processedLine.Substring(0, endIndex + "</test>".Length));
                        return header.ToString();
                    }
                }
                else
                {
                    if (processedLine.StartsWith("<test/>"))
                    {
                        return "<test/>";
                    }

                    if (processedLine.StartsWith("<test>"))
                    {
                        foundStart = true;
                    }
                    header.Append(processedLine);

                }
            }

            // malformed header
            return null;
        }

        public static TestHeader ParseExtractedTestHeader(string headerText)
        {
            if (headerText == null)
            {
                return null;
            }

            var runtimeVersionRange = new VersionRange();
            string configuration = "Debug";
            var targetFramework = "netcoreapp2.0";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(headerText);

            var requiresNodes = doc.SelectNodes("/test/requires");
            foreach (XmlNode requiresNode in requiresNodes)
            {
                foreach (XmlAttribute attribute in requiresNode.Attributes)
                {
                    if (attribute.Name == "runtime")
                    {
                        runtimeVersionRange = VersionRange.Parse(attribute.Value);
                    }
                }
            }

            var compileNodes = doc.SelectNodes("/test/compile");
            foreach (XmlNode compileNode in compileNodes)
            {
                foreach (XmlAttribute attribute in compileNode.Attributes)
                {
                    if (attribute.Name == "configuration")
                    {
                        configuration = attribute.Value;
                        switch (configuration.ToLowerInvariant())
                        {
                            case "debug":
                                configuration = "Debug";
                                break;
                            case "release":
                                configuration = "Release";
                                break;
                            default:
                                throw new Exception("Unable to parse compile configuration: " + configuration);
                        }
                    }

                    if (attribute.Name == "framework")
                    {
                        targetFramework = attribute.Value.ToLowerInvariant();
                    }
                }
            }

            return new TestHeader
            {
                Configuration = configuration,
                TargetFramework = targetFramework,
                TargetRuntimeVersion = runtimeVersionRange,
            };
        }

        #endregion
    }
}
