﻿
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Exprazor.TSParser
{
	public interface Token { }
	public interface LiteralToken : Token { }
	public class ExportKeyword : Token
	{
		public static ExportKeyword Instance = new ExportKeyword();
	}
	public class ImportKeyword : Token
	{
		public static ImportKeyword Instance = new ImportKeyword();
	}
	public class FromKeyword : Token
	{
		public static FromKeyword Instance = new FromKeyword();
	}
	public class FunctionKeyword : Token
	{
		public static FunctionKeyword Instance = new FunctionKeyword();
	}
	public class ConstKeyword : Token
	{
		public static ConstKeyword Instance = new ConstKeyword();
	}
	public class LetKeyword : Token
	{
		public static LetKeyword Instance = new LetKeyword();
	}
	public class VarKeyword : Token
	{
		public static VarKeyword Instance = new VarKeyword();
	}
	public class Endline : Token
	{
		public static Endline Instance = new Endline();
	}
	public class LParen : Token
	{
		public static LParen Instance = new LParen();
	}
	public class RParen : Token
	{
		public static RParen Instance = new RParen();
	}
	public class LBrace : Token
	{
		public static LBrace Instance = new LBrace();
	}
	public class RBrace : Token
	{
		public static RBrace Instance = new RBrace();
	}
	public class LBracket : Token
	{
		public static LBracket Instance = new LBracket();
	}
	public class RBracket : Token
	{
		public static RBracket Instance = new RBracket();
	}
	public class LAngleBracket : Token
    {
		public static LAngleBracket Instance = new LAngleBracket();
    }
	public class RAngleBracket : Token
    {
		public static RAngleBracket Instance = new RAngleBracket();
    }
	public class Comma : Token
	{
		public static Comma Instance = new Comma();
	}
	public class Colon : Token
	{
		public static Colon Instance = new Colon();
	}
	public class SemiColon : Token
	{
		public static SemiColon Instance = new SemiColon();
	}
	public class SingleQuot : Token
	{
		public static SingleQuot Instance = new SingleQuot();
	}
	public class DoubleQuot : Token
	{
		public static DoubleQuot Instance = new DoubleQuot();
	}
	public class Dot : Token
	{
		public static Dot Instance = new Dot();
	}
	public class Plus : Token
	{
		public static Plus Instance = new Plus();
	}
	public class Minus : Token
	{
		public static Minus Instance = new Minus();
	}
	public class Astarisk : Token
	{
		public static Astarisk Instance = new Astarisk();
	}
	public class Slash : Token
	{
		public static Slash Instance = new Slash();
	}
	public class Pipe : Token
    {
		public static Pipe Instance = new Pipe();
    }
	public class Question : Token
    {
		public static Question Instance = new Question();
    }
	public class EQ : Token
	{
		public static EQ Instance = new EQ();
	}
	public class NumberLiteral : LiteralToken
	{
		public NumberLiteral(string numString)
		{
			NumString = numString;
		}
		public string NumString { get; }
	}
	public class StringLiteral : LiteralToken
	{
		public StringLiteral(string value)
		{
			Value = value;
		}
		public string Value { get; }
	}
	public class NullLiteral : LiteralToken
    {
		public static NullLiteral Instance = new NullLiteral();
    }
	public class UndefinedLiteral : LiteralToken
    {
		public static UndefinedLiteral Instance = new UndefinedLiteral();
	}
	public class NumberKeyword : Token
	{
		public static NumberKeyword Instance = new NumberKeyword();
	}
	public class StringKeyword : Token
    {
		public static StringKeyword Instance = new StringKeyword();
	}
	public class AnyKeyword : Token
	{
		public static AnyKeyword Instance = new AnyKeyword();
	}
	public class ReturnKeyword : Token
    {
		public static ReturnKeyword Instance = new ReturnKeyword();
    }
	public class Identifier : Token
	{
		public Identifier(string name)
		{
			Name = name;
		}
		public string Name { get; }
	}
	public static class CharSpanExtention
	{
		public static bool startWith(this ReadOnlySpan<char> span, string str)
		{
			if (span.Length < str.Length) return false;
			for (int i = 0; i < str.Length; i++)
			{
				if (span[i] != str[i]) return false;
			}
			return true;
		}
	}
	public static class Lexer
	{
		public static Queue<Token> Tokenize(string source)
		{
			var ret = new Queue<Token>();
			source = Regex.Replace(source, "/\\*[\\s\\S]*\\*/", string.Empty);
			source = Regex.Replace(source, "//.+", string.Empty);
			source = source.Replace("\r\n", " ");
			TokenizeInternal(source.AsSpan(), ret);

			return ret;
		}
		static void TokenizeInternal(ReadOnlySpan<char> source, Queue<Token> tokens)
		{
			if (source.Length < 1) return;

			if (source[0] == ' ')
			{
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '\t')
			{
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '\n')
			{
				tokens.Enqueue(Endline.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '=')
			{
				tokens.Enqueue(EQ.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '(')
			{
				tokens.Enqueue(LParen.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == ')')
			{
				tokens.Enqueue(RParen.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '{')
			{
				tokens.Enqueue(LBrace.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '}')
			{
				tokens.Enqueue(RBrace.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '[')
			{
				tokens.Enqueue(LBracket.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == ']')
			{
				tokens.Enqueue(RBracket.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '<')
            {
				tokens.Enqueue(LAngleBracket.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '>')
			{
				tokens.Enqueue(RAngleBracket.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == ':')
			{
				tokens.Enqueue(Colon.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == ';')
			{
				tokens.Enqueue(SemiColon.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == ',')
			{
				tokens.Enqueue(Comma.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '\'')
			{
				tokens.Enqueue(SingleQuot.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if(source[0] == '|')
            {
				tokens.Enqueue(Pipe.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '"')
			{
				tokens.Enqueue(DoubleQuot.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '+')
			{
				if (char.IsNumber(source[1]))
				{
					int wordLength = 1;
					while (source.Length > 0 && (char.IsLetterOrDigit(source[wordLength]))) wordLength++;
					tokens.Enqueue(new NumberLiteral(source.Slice(0, wordLength).ToString()));
					TokenizeInternal(source.Slice(wordLength), tokens);
					return;
				}
				else
				{
					tokens.Enqueue(Plus.Instance);
					TokenizeInternal(source.Slice(1), tokens);
					return;
				}
			}
			if (source[0] == '-')
			{
				if (char.IsNumber(source[1]))
				{
					int wordLength = 1;
					while (source.Length > 0 && (char.IsLetterOrDigit(source[wordLength]))) wordLength++;
					tokens.Enqueue(new NumberLiteral(source.Slice(0, wordLength).ToString()));
					TokenizeInternal(source.Slice(wordLength), tokens);
					return;
				}
				else
				{
					tokens.Enqueue(Minus.Instance);
					TokenizeInternal(source.Slice(1), tokens);
					return;
				}
			}
			if (source[0] == '*')
			{
				tokens.Enqueue(Astarisk.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if (source[0] == '/')
			{
				tokens.Enqueue(Slash.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
			}
			if(source[0] == '?')
            {
				tokens.Enqueue(Question.Instance);
				TokenizeInternal(source.Slice(1), tokens);
				return;
            }

			if (char.IsDigit(source[0]))
			{
				int wordLength = 1;
				while (source.Length > 0 && (char.IsLetterOrDigit(source[wordLength]))) wordLength++;
				tokens.Enqueue(new NumberLiteral(source.Slice(0, wordLength).ToString()));
				TokenizeInternal(source.Slice(wordLength), tokens);
				return;
			}

			if (char.IsLetter(source[0]) || source[0] == '_')
			{
				int wordLength = 1;
				while (source.Length > 0 && (char.IsLetterOrDigit(source[wordLength]) || source[wordLength] == '_')) wordLength++;

				string ident = source.Slice(0, wordLength).ToString();

				tokens.Enqueue(ident switch
				{
					"export" => ExportKeyword.Instance,
					"function" => FunctionKeyword.Instance,
					"import" => ImportKeyword.Instance,
					"from" => FromKeyword.Instance,
					"var" => VarKeyword.Instance,
					"let" => LetKeyword.Instance,
					"const" => ConstKeyword.Instance,
					"null" => NullLiteral.Instance,
					"undefined" => UndefinedLiteral.Instance,
					"number" => NumberKeyword.Instance,
					"string" => StringKeyword.Instance,
					"any" => AnyKeyword.Instance,
					"return" => ReturnKeyword.Instance,
					_ => new Identifier(ident)
				});
				TokenizeInternal(source.Slice(wordLength), tokens);
				return;
			}

		}
	}
}
