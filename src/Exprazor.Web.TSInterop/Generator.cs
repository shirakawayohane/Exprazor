using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Exprazor.AspNetCoreServer.TSInterop
{
    [Generator(LanguageNames.CSharp)]
    public sealed class Generator : IIncrementalGenerator
    {
        static bool Predicate(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
        }

        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            initContext.RegisterPostInitializationOutput(GenerateInitialCode);

            var classSymbols = initContext.SyntaxProvider.CreateSyntaxProvider(
                    predicate: Predicate,
                    transform: static (ctx, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var syntax = (ctx.Node as ClassDeclarationSyntax)!;
                        var symbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(ctx.SemanticModel, syntax, token);

                        if (symbol == null) return null;

                        if (symbol.GetAttributes().Any(x => "Exprazor.Web.TSInterop.TSInteropAttribute" == $"{x.AttributeClass!.ContainingNamespace}.{x.AttributeClass!.Name}"))
                        {
                            //var filePath = ctx.Node.SyntaxTree.FilePath;
                            //var tsFilePath = Path.ChangeExtension("cs", "ts");
                            return symbol;
                        }

                        return null;
                    }
                )
                .Where(static x => x is not null);


            initContext.RegisterSourceOutput(classSymbols, (context, symbol) =>
            {
                //var (symbol, tsFilePath) = tuple;
                //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                //    id: "HOGE001",
                //    title: "アナライザー動いてますよ",
                //    messageFormat: "Analyzer is working!",
                //    category: "HOGEHOGE",
                //    DiagnosticSeverity.Info,
                //    true
                //    ), Location.None));

                //var source = File.ReadAllText(tsFilePath);
                //var sourceAST = TSParser.Parser.Parse(source);

                //{ (File.Exists(tsFilePath) ? "private static void FileFound() {}" : "private static void FileNotFound() {}")}
                // とりあえず適当なメソッドを生やす。

                string source = $@"namespace {symbol!.ContainingNamespace}
{{
    {symbol!.GetAccesibilityString()} partial class {symbol.Name} {{
        private static void DoSomethingNew() {{
        }}
    }}
}}
";

                context.AddSource(hintName: $"{symbol!.Name}.g.cs", source);
            });
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
