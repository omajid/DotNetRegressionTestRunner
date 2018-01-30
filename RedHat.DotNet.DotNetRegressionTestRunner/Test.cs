using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class Test
    {
        public static bool FileIsATest(FileInfo filePath)
        {
            using (StreamReader reader = filePath.OpenText())
            {
                // TODO is this really a good idea? We are using our
                // version of roslyn to parse the source. If there are
                // bugs in our version of roslyn, or the test contains
                // newer language features, we might run into issues.
                // It might be better to resort to bad regular
                // expressions for this.
            
                var sourceCode = reader.ReadToEnd();
                var text = SourceText.From(sourceCode);
                var syntaxTree = CSharpSyntaxTree.ParseText(text);
                syntaxTree.TryGetRoot(out var root);

                return root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(IsMain)
                    .Any();
            }
        }

        private static bool IsMain(MethodDeclarationSyntax syntax)
        {
            var isMain = syntax.Identifier.Value.Equals("Main");
            var isVoid = syntax.ReturnType is PredefinedTypeSyntax &&
                ((PredefinedTypeSyntax)syntax.ReturnType).Keyword.Value.Equals("void");

            var hasOneStringArrayArgument = false;
            
            var parameters = syntax.ParameterList.Parameters.ToList();
            if (parameters.Count() == 1)
            {
                var id = parameters[0].Identifier;
                var isArray = parameters[0].Type.IsKind(SyntaxKind.ArrayType);
                if (isArray)
                {
                    var elementType = ((ArrayTypeSyntax)parameters[0].Type).ElementType;
                    if (elementType is PredefinedTypeSyntax)
                    {
                        var isStringArray = ((PredefinedTypeSyntax)elementType)
                            .Keyword.IsKind(SyntaxKind.StringKeyword);
                        hasOneStringArrayArgument = true;
                    }
                }
            }

            var hasTypeParameters = syntax.TypeParameterList != null;

            return isVoid && isMain && !hasTypeParameters && hasOneStringArrayArgument;
        }

        public static bool FileTargetsCurrentRuntime(FileInfo sourceFile)
        {
            // TODO
            return true;
        }
    }
}
