
using System.Collections.Generic;

namespace Exprazor.TSParser.AST
{
	public interface AST {}
	public interface TopLevel : AST {}
	public class SourceTree : AST
	{
		public SourceTree(List<TopLevel> topLevels)
		{
			TopLevels = topLevels;
		}

		public List<TopLevel> TopLevels { get; }
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
	public class NotSpecified : TypeSyntax
    {
		public static NotSpecified Instance = new NotSpecified();
    }
	public class LiteralType : TypeSyntax
    {
        public LiteralType(LiteralToken literal)
        {
            Literal = literal;
        }

        public LiteralToken Literal { get; }
    }

	public class TypeReference : TypeSyntax
    {
        public TypeReference(Identifier identifier)
        {
            Identifier = identifier;
        }

        public Identifier Identifier { get; }
    }
	public class AnyType : TypeSyntax
	{
		public static AnyType Instance = new AnyType();
	}

	public class UnionType : TypeSyntax
    {
        public UnionType(TypeSyntax[] types)
        {
            Types = types;
        }

        public TypeSyntax[] Types { get; }
    }

	public class ArrayType : TypeSyntax
	{
		public ArrayType(TypeSyntax arrayOf)
		{
			ArrayOf = arrayOf;
		}

		public TypeSyntax ArrayOf { get; }
	}
	public class VoidType : TypeSyntax
    {
		public static VoidType Instance = new VoidType();
	}
	public class UnspecifiedType : TypeSyntax
	{
		public static UnspecifiedType Instance = new UnspecifiedType();
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
		public bool HasReturnValue { get; }

        public Block(bool hasReturnValue)
        {
            HasReturnValue = hasReturnValue;
        }
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
		public FunctionDecl(Identifier identifier, List<Parameter> parameters, TypeSyntax returnType, Block block)
		{
			Identifier = identifier;
			Parameters = parameters;
			this.ReturnType = returnType;
			Block = block;
		}

		public Identifier Identifier { get; }
		public List<Parameter> Parameters { get; }
		public TypeSyntax ReturnType { get; }
		public Block Block { get; }
	}
}