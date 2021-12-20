using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;

namespace Exprazor.AspNetCoreServer.TSInterop
{
    [Generator(LanguageNames.CSharp)]
    public sealed class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            initContext.RegisterPostInitializationOutput(GenerateInitialCode);

            var classSymbols = initContext.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        return s is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
                    },
                    transform: static (ctx, token) =>
                    {
                        token.ThrowIfCancellationRequested();
                        var syntax = (ctx.Node as ClassDeclarationSyntax)!;
                        var symbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(ctx.SemanticModel, syntax, token);

                        if (symbol == null) return null;

                        if (symbol.GetAttributes().Any(x => "Exprazor.AspNetCoreServer.TSInterop.TSInteropAttribute" == $"{x.AttributeClass!.ContainingNamespace}.{x.AttributeClass!.Name}"))
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
                context.AddSource(hintName: $"{symbol!.Name}.g.cs", $@"namespace {symbol.ContainingNamespace} {{
    public partial class {symbol.Name} {{
        private static void DoSomethingNew() {{
        }}
    }}
}}
");
            });
        }


        private static void GenerateInitialCode(IncrementalGeneratorPostInitializationContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            const string TSInteropAttributeCs = @"namespace Exprazor.AspNetCoreServer.TSInterop
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
