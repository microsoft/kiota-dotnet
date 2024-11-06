﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace KiotaGenerated;

[Generator]
public class KiotaVersionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
        {
            var projectDirectory = Path.GetDirectoryName(compilation.SyntaxTrees.First().FilePath);

            try
            {
                XmlDocument csproj = new XmlDocument();
                projectDirectory = Path.Combine(projectDirectory, "..", "..", "..", "Directory.Build.props");
                csproj.Load(projectDirectory);
                var version = csproj.GetElementsByTagName("VersionPrefix")[0].InnerText;
                string source = $@"// <auto-generated/>
namespace Microsoft.Kiota.Http.Generated
{{
    /// <summary>
    /// The version class
    /// </summary>
    public static class Version
    {{
        /// <summary>
        /// The current version string
        /// </summary>
        public static string Current()
        {{
            return ""{version}"";
        }}
    }}
}}
";

                // Add the source code to the compilation
                spc.AddSource($"KiotaVersion.g.cs", SourceText.From(source, Encoding.UTF8));
            }
            catch(Exception e)
            {
                throw new FileNotFoundException($"KiotaVersionGenerator expanded in an invalid project, missing 'Directory.Build.props' file in the following directory {projectDirectory}", e);
            }
        });
    }

}
