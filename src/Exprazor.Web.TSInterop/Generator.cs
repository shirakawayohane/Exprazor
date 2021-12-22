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

                        if (symbol == null) return default;

                        if (symbol.GetAttributes().Any(x => "Exprazor.Web.TSInterop.TSInteropAttribute" == $"{x.AttributeClass!.ContainingNamespace}.{x.AttributeClass!.Name}"))
                        {
                            var filePath = ctx.Node.SyntaxTree.FilePath;
                            var tsFilePath = Path.ChangeExtension(filePath, "ts");
                            if (File.Exists(tsFilePath))
                                return (symbol, tsFilePath);
                        }

                        return default;
                    }
                )
                .Where(static x => x is not (null, _) and not (_, null));


            initContext.RegisterSourceOutput(classSymbols, (context, tuple) =>
            {
                var (symbol, tsFilePath) = tuple;
                if (!File.Exists(tsFilePath)) return;

                var tsSource = File.ReadAllText(tsFilePath);
                var sourceAST = TSParser.Parser.Parse(tsSource);

                // とりあえず適当なメソッドを生やす。
                string source = $@"namespace {symbol!.ContainingNamespace}
{{
    {symbol!.GetAccesibilityString()} partial class {symbol.Name} {{
{Utils.GenerateTSBindingsFromAST(sourceAST, 1)}
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
