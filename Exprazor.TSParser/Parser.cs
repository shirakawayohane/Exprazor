
using System;
using System.Collections.Generic;

namespace Exprazor.TSParser
{
	public static class Parser
	{
		public class ParseException : Exception
		{
			public ParseException(string message) : base(message) { }
		}
		public static AST Parse(string source)
		{
			var tokens = Lexer.Tokenize(source);
			return ParseSourceFile(tokens);
		}

		static SourceFile ParseSourceFile(Queue<Token> tokens)
		{
			List<TopLevel> topLevels = new List<TopLevel>();
			while (tokens.Count > 0)
			{
				topLevels.Add(ParseTopLevel(tokens));
			}
			return new SourceFile(topLevels);
		}

		static TopLevel ParseTopLevel(Queue<Token> tokens)
		{
			while (tokens.Peek() is Endline)
			{
				tokens.Dequeue();
			}
			if (tokens.Peek() is ExportKeyword)
			{
				return ParseExportStatement(tokens);
			}
			else if (tokens.Peek() is ImportStatement)
			{
				return ParseImportStatement(tokens);
			}
			else
			{
				return ParseTopLevelDecl(tokens);
			}
		}

		public static ImportStatement ParseImportStatement(Queue<Token> tokens)
		{
			tokens.Dequeue();
			while (true)
			{
				var token = tokens.Dequeue();
				if (token is FromKeyword)
				{
					if (tokens.Peek() is SemiColon)
					{
						tokens.Dequeue();
					}
				}
				return ImportStatement.Instance;
			}
		}

		public static ExportStatement ParseExportStatement(Queue<Token> tokens)
		{
			tokens.Dequeue();
			var what = ParseTopLevelDecl(tokens);
			return new ExportStatement(what);
		}

		public static void SkipTopLevelVariableDecls(Queue<Token> tokens)
		{
			//while(tokens.Peek() is ExportKeyword or FunctionDecl == false) {
			//	tokens.Dequeue();
			//}
			while (tokens.Count > 0)
			{
				var token = tokens.Peek();
				if (token is ExportKeyword || token is FunctionKeyword)
				{
					break;
				}
				tokens.Dequeue();
			}
		}

		public static TopLevelDecl ParseTopLevelDecl(Queue<Token> tokens)
		{
			SkipTopLevelVariableDecls(tokens);
			var token = tokens.Dequeue();
			if (token is FunctionKeyword)
			{
				return ParseFunctionDecl(tokens);
			}
			else
			{
				throw new ParseException("TopLevel declaration other than function declarations are not supported for now.");
			}
		}

		public static FunctionDecl ParseFunctionDecl(Queue<Token> tokens)
		{
			var token = tokens.Dequeue();
			if (token is Identifier ident == false) throw new ParseException("An identifier is expected after function keyword");
			var nextToken = tokens.Dequeue();
			if (nextToken is LParen == false) throw new ParseException("The character '(' is exptected after function identifier");

			var parameters = new List<Parameter>();
			while (true)
			{
				var _token = tokens.Peek();
				if (_token is RParen)
				{
					tokens.Dequeue();
					if (tokens.Peek() is LBracket)
					{
						return new FunctionDecl(ident, parameters, ParseBlock(tokens));
					}
					throw new ParseException("Function decl must be proceeded with block.");
				}
				parameters.Add(ParseParameter(tokens));
				if (tokens.Peek() is Comma)
				{
					tokens.Dequeue();
					continue;
				}
			}
		}

		public static Parameter ParseParameter(Queue<Token> tokens)
		{
			var token = tokens.Dequeue();
			if (token is Identifier ident == false) throw new ParseException("A parameter name is required.");

			var next = tokens.Dequeue();
			if (next is Colon)
			{
				return new Parameter(ident, ParseTypeSyntax(tokens));
			}
			else
			{
				return new Parameter(ident, AnyType.Instance);
			}
		}

		public static TypeSyntax ParseTypeSyntax(Queue<Token> tokens)
		{
			var token = tokens.Dequeue();
			if (token is Identifier ident == false) throw new ParseException("Type other than number or string or HTMLElement or their array type is not supported for now.");
			TypeSyntax type = ident.Name switch
			{
				"number" => NumberType.Instance,
				"string" => StringType.Instance,
				"HTMLElement" => HTMLElement.Instance,
				_ => throw new ParseException("Type other than number or string or HTMLElement or their array type is not supported for now.")
			};
			if (tokens.Peek() is LSQBracket)
			{
				tokens.Dequeue();
				if (tokens.Dequeue() is RSQBracket)
				{
					return new ArrayType(type);
				}
				throw new ParseException("Array type is invalid");
			}

			return type;
		}

		public static Block ParseBlock(Queue<Token> tokens)
		{
			tokens.Dequeue();
			while (true)
			{
				if (tokens.Count == 0) throw new ParseException("A block must be closed with '}'");
				if (tokens.Dequeue() is RBracket)
				{
					return new Block();
				}
			}
		}
	}
}
