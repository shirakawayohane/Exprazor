
using Exprazor.TSParser.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exprazor.TSParser
{
	public static class Parser
	{
		public class ParseException : Exception
		{
			public ParseException(string message) : base(message) { }
		}
		public static SourceTree Parse(string source)
		{
			var tokens = Lexer.Tokenize(source);
			return ParseSourceFile(tokens);
		}

		static SourceTree ParseSourceFile(Queue<Token> tokens)
		{
			List<TopLevel> topLevels = new List<TopLevel>();
			while (tokens.Count > 0)
			{
				topLevels.Add(ParseTopLevel(tokens));
			}
			return new SourceTree(topLevels);
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

					TypeSyntax type = VoidType.Instance;
					if(tokens.Peek() is Colon)
                    {
						tokens.Dequeue();
						type = ParseTypeSyntax(tokens, false);
                    }
					if (tokens.Peek() is LBrace)
					{
						var block = ParseBlock(tokens);
						if(type is VoidType && block.HasReturnValue)
                        {
							type = UnspecifiedType.Instance;
                        }
						return new FunctionDecl(ident, parameters, type, block);
					}
					throw new ParseException("Function decl must be proceeded with type or block.");
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
			bool optional = false;
			if(tokens.Peek() is Question)
            {
				tokens.Dequeue();
				optional = true;
            }
			var next = tokens.Dequeue();
			if (next is Colon)
			{
				return new Parameter(ident, ParseTypeSyntax(tokens, optional));
			}
			else
			{
				return new Parameter(ident, AnyType.Instance);
			}
		}

		static IEnumerable<TypeSyntax> FlattenUnion(TypeSyntax t)
		{
			if (t is not UnionType uni)
			{
				yield return t;
			}
			else
			{
				foreach (var c in uni.Types)
					foreach (var cc in FlattenUnion(c))
						yield return cc;
			}
		}
		public static TypeSyntax ParseTypeSyntax(Queue<Token> tokens, bool optional)
		{
			TypeSyntax type = tokens.Dequeue() switch
			{
				NumberKeyword => NumberType.Instance,
				StringKeyword => StringType.Instance,
				AnyKeyword => AnyType.Instance,
				LiteralToken literal => literal switch
				{
					UndefinedLiteral uk => new LiteralType(uk),
					NullLiteral nk => new LiteralType(nk),
					StringLiteral stl => new LiteralType(stl),
					NumberLiteral nl => new LiteralType(nl),
					_ => throw new NotSupportedException("Literal type except null and undefined is currently not supported.")
				},
				Identifier ident => new TypeReference(new Identifier(ident.Name)),
				_ => throw new ParseException("Type syntax is invalid")
			};
			if (tokens.Peek() is LBracket)
			{
				tokens.Dequeue();
				if (tokens.Dequeue() is RBracket)
				{
					type = new ArrayType(type);
				}
				else
				{
					throw new ParseException("Array type is invalid");
				}
			}
			if(tokens.Peek() is Pipe)
            {
				if(optional)
                {
					type = new UnionType(new[] { type, new LiteralType(UndefinedLiteral.Instance), ParseTypeSyntax(tokens, false)});
                } else
				{
					type = new UnionType(new[] { type, ParseTypeSyntax(tokens, false) });
				}
				return new UnionType(FlattenUnion(type).ToArray());
			}

			return type;
		}

		public static Block ParseBlock(Queue<Token> tokens)
		{
			if (tokens.Peek() is not LBrace) throw new ParseException("Start of block '{' exptected.");
			tokens.Dequeue();
			bool hasReturnValue = false;
			while (true)
			{
				if (tokens.Count == 0) throw new ParseException("A block must be closed with '}'");
				var token = tokens.Dequeue();
				if (token is RBrace)
				{
					return new Block(hasReturnValue);
				}
				if(token is ReturnKeyword)
                {
					if(tokens.Peek() is not SemiColon and not RBrace)
                    {
						hasReturnValue = true;
                    }
					continue;
                }
			}
		}
	}
}
