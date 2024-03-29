﻿using Exprazor.TSParser.AST;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Exprazor.TSParser;

namespace Exprazor.AspNetCoreServer.TSInterop
{
    internal static class Utils
    {
        public static string GetAccesibilityString(this INamedTypeSymbol symbol) => symbol.DeclaredAccessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "internal protected",
            Accessibility.Public => "public",
            _ => ""
        };

        public static IEnumerable<AST> Traverse(this AST ast)
        {
            switch (ast)
            {
                case SourceTree st:
                    yield return st;
                    foreach (var topLevel in st.TopLevels)
                        foreach (var node in topLevel.Traverse())
                            yield return node;
                    break;
                case UnionType uni:
                    yield return uni;
                    foreach (var type in uni.Types)
                        foreach (var c in type.Traverse())
                            yield return c;
                    break;
                case ArrayType array:
                    yield return array;
                    foreach (var node in array.ArrayOf.Traverse())
                        yield return node;
                    break;
                case Parameter p:
                    yield return p;
                    foreach (var t in p.Type.Traverse())
                        yield return t;
                    break;
                case ExportStatement ex:
                    yield return ex;
                    foreach (var e in ex.What.Traverse())
                        yield return e;
                    break;
                case FunctionDecl func:
                    yield return func;
                    foreach (var param in func.Parameters)
                        foreach (var node in param.Traverse())
                            yield return node;
                    foreach (var node in func.ReturnType.Traverse())
                        yield return node;
                    foreach (var node in func.Block.Traverse())
                        yield return node;
                    break;
                default:
                    yield return ast;
                    break;
            }
        }

        public static string ToCsTypeString(TypeSyntax type)
        {
            if (type is NumberType) return "double";
            if (type is StringType) return "string";
            if (type is UnspecifiedType) return "object";
            if (type is TypeReference typeRef) return typeRef.Identifier.Name switch
            {
                "HTMLElement" => "global::Exprazor.ElementReference",
                _ => "object"
            };
            if (type is VoidType) return "void";
            if(type is ArrayType array)
            {
                return ToCsTypeString(array.ArrayOf) + "[]";
            }
            if(type is UnionType union)
            {
                var types = union.Types;
                if(types.Any(x => x is LiteralType lt && lt.Literal is NullLiteral or UndefinedLiteral)) {
                    try
                    {
                        var mainType = types.Single(x => x is LiteralType lt && (lt.Literal is not NullLiteral and not UndefinedLiteral));
                        return $"Nullable<{ToCsTypeString(mainType)}>";
                    } catch
                    {
                        return "object?";
                    }
                }
            }

            return "object";
        }

        public static string GenerateTSBindingsFromAST(AST ast, int numTabs)
        {
            string indent = new string(Enumerable.Repeat('\t', numTabs).ToArray());
            var builder = new StringBuilder();
            foreach (var func in ast.Traverse().Where(x => x is ExportStatement export && export.What is FunctionDecl).Select(x => (x as ExportStatement)!.What as FunctionDecl))
            {
                builder.Append(indent);
                builder.AppendFormat("protected {0} {1}(", ToCsTypeString(func!.ReturnType), func!.Identifier.Name);
                int index = 0;
                foreach(var parameter in func.Parameters)
                {
                    builder.Append(ToCsTypeString(parameter.Type));
                    builder.Append(' ');
                    builder.Append(parameter.Identifier.Name);
                    if (index < func.Parameters.Count - 1)
                    {
                        builder.Append(", ");
                    }
                    index++;
                }
                builder.Append(')');
                builder.AppendLine();
                builder.Append(indent);
                builder.Append('{');
                builder.AppendLine();

                // Content
                builder.Append(indent);
                builder.Append('\t');
                builder.Append("throw new NotImplementedException();");
                builder.AppendLine();

                builder.Append(indent);
                builder.Append('}');
                builder.AppendLine();
            }

            return builder.ToString();
        }

        //public static (string? nameSpace, string? accesibility, string? className) ReadCsInterestIdentifiers(string source)
        //{
        //    string? nameSpace = null;
        //    string? accesibility = null;
        //    string? className = null;
        //    var words = source.Replace("\r\n", "\n").Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        //    // don't care about last word.
        //    for(int i = 0; i < words.Length - 1; i++)
        //    {
        //        if (words[i] == "namespace")
        //        {
        //            var lbraceIndex = words[i + 1].IndexOf('{');
        //            if(lbraceIndex > 0)
        //            {
        //                nameSpace = words[i + 1].Substring(0, lbraceIndex - 1);
        //                continue;
        //            }
        //            nameSpace = words[i + 1];
        //            continue;
        //        }

        //        if(words[i] == "class")
        //        {
        //            // read class name
        //            var colonIndex = words[i + 1].IndexOf(':');
        //            if(colonIndex > 0)
        //            {
        //                className = words[words.Length - 1].Substring(0, colonIndex - 1);
        //                continue;
        //            }
        //            var lbraceIndex = words[i + 1].IndexOf('{');
        //            if (lbraceIndex > 0)
        //            {
        //                className = words[i + 1].Substring(0, lbraceIndex - 1);
        //                continue;
        //            }
        //            className = words[i + 1];
        //            continue;

        //            // read accesibility
        //            if(i > 0)
        //            {
        //                accesibility = words[i - 1] switch
        //                {
        //                    "public"
        //                }
        //            }
        //        }
        //    }
        //    return (nameSpace, accesibility, className);
        //}
    }
}