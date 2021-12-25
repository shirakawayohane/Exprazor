using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Exprazor.AspNetCoreServer.TSInterop
{
    [Generator(LanguageNames.CSharp)]
    public sealed class Generator : IIncrementalGenerator
    {
        sealed class Eqcomp : IEqualityComparer<(AdditionalText, INamedTypeSymbol)>
        {
            public static Eqcomp Instance = new();

            public bool Equals((AdditionalText, INamedTypeSymbol) x, (AdditionalText, INamedTypeSymbol) y) => x.Item2.Name == y.Item2.Name;

            public int GetHashCode((AdditionalText, INamedTypeSymbol) obj) => obj.Item2.Name.GetHashCode();
        }

        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            initContext.RegisterPostInitializationOutput(GenerateInitialCode);

            var additionalFilesProvider = initContext.AdditionalTextsProvider.Where(x => x.Path.EndsWith(".ts"));

            var syntaxProvider = initContext.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        return node is ClassDeclarationSyntax;
                    },
                    transform: static (ctx, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var syntax = (ctx.Node as ClassDeclarationSyntax)!;
                        var symbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(ctx.SemanticModel, syntax, token);
                        return symbol;
                    }
                )
                .Where(x => x is not null).Collect();

            var provider = additionalFilesProvider
                .Combine(syntaxProvider)
                .Select((tuple, context) =>
                {
                    var (file, symbols) = tuple;
                    //var targetFile = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x.Path) == symbol!.Name);
                    var targetSymbol = symbols.FirstOrDefault(x => x!.Name == Path.GetFileNameWithoutExtension(file.Path));
                    return (file, targetSymbol);
                })
                .Where(tuple => tuple is (not null, not null))
                !.WithComparer(Eqcomp.Instance);

            initContext.RegisterSourceOutput(provider, (context, tuple) =>
            {
                var (tsFile, symbol) = tuple;
                var tsSource = tsFile?.GetText();
                if (symbol is null || tsSource is null) return;
                var sourceAST = TSParser.Parser.Parse(tsSource.ToString());
                int indent = 0;
                var builder = new StringBuilder();
                void Write(string text)
                {
                    for(int i = 0; i < indent; i++)
                    {
                        builder!.Append('\t');
                    }
                    builder!.AppendLine(text);
                }
                var nameSpace = symbol.ContainingNamespace.ToDisplayString();
                if (nameSpace is not null) {
                    Write($"namespace {nameSpace} {{");
                    ++indent;
                }
                Write($"{Utils.GetAccesibilityString(symbol)} partial class {symbol.Name}");
                Write("{");
                ++indent;
                builder.Append(Utils.GenerateTSBindingsFromAST(sourceAST, indent));
                --indent;
                Write("}");
                --indent;
                Write("}");
                context.AddSource(symbol.Name + ".generated", builder.ToString());
            });


            //            initContext.RegisterSourceOutput(classSymbols, (context, tuple) =>
            //            {
            //                var (symbol, tsFilePath) = tuple;
            //                if (!File.Exists(tsFilePath)) return;

            //                var tsSource = File.ReadAllText(tsFilePath);
            //                var sourceAST = TSParser.Parser.Parse(tsSource);

            //                // とりあえず適当なメソッドを生やす。
            //                string source = $@"namespace {symbol!.ContainingNamespace}
            //{{
            //    {symbol!.GetAccesibilityString()} partial class {symbol.Name} {{
            //{Utils.GenerateTSBindingsFromAST(sourceAST, 1)}
            //    }}
            //}}
            //";

            //                context.AddSource(hintName: $"{symbol!.Name}.g.cs", source);
            //            });
        }


        private static void GenerateInitialCode(IncrementalGeneratorPostInitializationContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            const string TSInteropAttributeCs = @"namespace Exprazor.Web.TSInterop
{
    [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
    public sealed class TSInteropAttribute : global::System.Attribute {}
}
";
            context.AddSource("TSInteropAttribute.cs", TSInteropAttributeCs);

        }
    }

    record struct ClassSymbolPath(INamedTypeSymbol classSymbol, string filePath);

}
