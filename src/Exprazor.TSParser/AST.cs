
using System.Collections.Generic;

namespace Exprazor.TSParser
{
	public interface AST { }
	public interface TopLevel : AST
	{
	}
	public class SourceFile : AST
	{
		public SourceFile(List<TopLevel> topLevels)
		{
			TopLevels = topLevels;
		}

		List<TopLevel> TopLevels { get; }
	}
	public interface TypeSyntax : AST { }
	public class NumberType : TypeSyntax
	{
		public static NumberType Instance = new NumberType();
	}
	public class StringType : TypeSyntax
	{
		public static StringType Instance = new StringType();
	}
	public class AnyType : TypeSyntax
	{
		public static AnyType Instance = new AnyType();
	}
	public class HTMLElement : TypeSyntax
	{
		public static HTMLElement Instance = new HTMLElement();
	}
	public class ArrayType : TypeSyntax
	{
		public ArrayType(TypeSyntax arrayOf)
		{
			ArrayOf = arrayOf;
		}

		public TypeSyntax ArrayOf { get; }
	}
	public class Parameter : AST
	{
		public Parameter(Identifier identifier, TypeSyntax type)
		{
			Identifier = identifier;
			Type = type;
		}

		public Identifier Identifier { get; }
		public TypeSyntax Type { get; }
	}
	public class Block : AST
	{
		// Do nothing for now.
	}
	public class ExportStatement : TopLevel
	{
		public ExportStatement(TopLevel what)
		{
			What = what;
		}

		public TopLevel What { get; }
	}

	public class ImportStatement : TopLevel
	{
		public static ImportStatement Instance = new ImportStatement();
	}

	public interface TopLevelDecl : TopLevel { }
	public class FunctionDecl : TopLevelDecl
	{
		public FunctionDecl(Identifier identifier, List<Parameter> parameters, Block block)
		{
			Identifier = identifier;
			Parameters = parameters;
			Block = block;
		}

		public Identifier Identifier { get; }
		public List<Parameter> Parameters { get; }
		public Block Block { get; }
	}
}