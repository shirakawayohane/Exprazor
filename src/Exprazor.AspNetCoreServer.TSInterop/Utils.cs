using Exprazor.TSParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exprazor.AspNetCoreServer.TSInterop
{
    internal static class Utils
    {
        public static string CreateCsBindings(string tsSource)
        {
            var builder = new StringBuilder();
            SourceTree sourceTree = TSParser.Parser.Parse(tsSource)!;
            foreach(var topLevel in sourceTree.TopLevels)
            {
                if(topLevel is ExportStatement exp && exp.What is FunctionDecl functionDecl)
                {
                    builder.Append("protected void ");
                    builder.Append(functionDecl.Identifier);
                    builder.Append('(');
                    foreach(var param in functionDecl.Parameters)
                    {
                        switch (param.Type)
                        {
                            case NumberType:
                                builder.Append("double ");
                                break;
                            case StringType:
                                builder.Append("string ");
                                break;
                            //case HTMLElementType:
                            //    builder.Append("Exprazor.ElementReference")
                        }
                    }
                    builder.Append(')');
                }
            }

            return builder.ToString();
        }
    }
}
