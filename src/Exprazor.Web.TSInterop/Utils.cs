using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
