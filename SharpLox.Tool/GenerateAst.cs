﻿using System.Security.Cryptography;

namespace SharpLox.Tool;

internal class GenerateAst
{
	private static readonly StringSplitOptions _noEmptyTrimmed = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

	public static void Main(string[] args)
	{
		if (args.Length != 1)
		{
			Console.Error.WriteLine("Usage: GenerateAst <output directory>");
			Environment.Exit(64);
		}
		string outputDir = args[0];

		List<string> typeDefs = [
			"Assign   : Token name, Expr value",
			"Binary   : Expr left, Token @operator, Expr right",
			"Call     : Expr callee, Token paren, List<Expr> arguments",
			"Get      : Expr @object, Token name",
			"Grouping : Expr expression",
			"Literal  : object? value",
			"Logical  : Expr left, Token @operator, Expr right",
			"Set      : Expr @object, Token name, Expr value",
			"Super    : Token keyword, Token method",
			"This     : Token keyword",
			"Unary    : Token @operator, Expr right",
			"Variable : Token name"
		];
		Dictionary<string, List<TypeDef>> parsedFields = ParseTypeDefs(typeDefs);

		DefineAst(outputDir, "Expr", parsedFields);

		typeDefs = [
			"Block      : List<Stmt> statements",
			"Class      : Token name, Expr.Variable? superclass, List<Stmt.Function> methods",
			"Expression : Expr expres",
			"Function   : Token name, List<Token> @params, List<Stmt> body",
			"If         : Expr condition, Stmt thenBranch, Stmt? elseBranch",
			"Print      : Expr expres",
			"Return     : Token keyword, Expr? value",
			"Var        : Token name, Expr? initializer",
			"While      : Expr condition, Stmt body"
		];
		parsedFields = ParseTypeDefs(typeDefs);
		DefineAst(outputDir, "Stmt", parsedFields);
	}

	private static Dictionary<string, List<TypeDef>> ParseTypeDefs(List<string> typeDefs)
	{
		return typeDefs.Select(f =>
		{
			string[] t = f.Split(':', _noEmptyTrimmed);
			string typeName = t[0];

			List<TypeDef> fields = t[1].Split(',', _noEmptyTrimmed).Select(fi =>
			{
				string[] ft = fi.Split(' ', _noEmptyTrimmed);
				int i = ft[1][0] == '@' ? 1 : 0;
				string fc = char.ToUpperInvariant(ft[1][i]) + ft[1][(i + 1)..];
				return new TypeDef(Type: ft[0], Param: ft[1], Field: fc);
			}).ToList();

			return new KeyValuePair<string, List<TypeDef>>(typeName, fields);
		}).ToDictionary();
	}

	private static void DefineAst(string outputDir, string baseName, Dictionary<string, List<TypeDef>> types)
	{
		string path = Path.Combine(outputDir, baseName + ".cs");
		using StreamWriter writer = File.CreateText(path);

		writer.WriteLine("namespace SharpLox;");
		writer.WriteLine();
		writer.WriteLine("internal abstract class " + baseName);
		writer.WriteLine("{");

		DefineVisitor(writer, baseName, types);

		// The base Accept() method.
		writer.WriteLine();
		writer.WriteLine("	public abstract T Accept<T>(IVisitor<T> visitor);");

		// The AST classes.
		foreach ((string typeName, List<TypeDef> fields) in types)
		{
			DefineType(writer, baseName, typeName, fields);
		}

		writer.WriteLine("}");
		writer.Close();
	}

	private static void DefineVisitor(StreamWriter writer, string baseName, Dictionary<string, List<TypeDef>> types)
	{
		writer.WriteLine("	public interface IVisitor<T>");
		writer.WriteLine("	{");

		string paramName = baseName.ToLowerInvariant();
		foreach ((string typeName, List<TypeDef> _) in types)
			writer.WriteLine($"		T Visit{typeName}{baseName}({typeName} {paramName});");

		writer.WriteLine("	}");
	}

	private static void DefineType(StreamWriter writer, string baseName, string className, List<TypeDef> fieldList)
	{
		writer.WriteLine();
		writer.WriteLine($"	public class {className}({string.Join(", ", fieldList.Select(td => td.Type + " " + td.Param))}) : {baseName}");
		writer.WriteLine("	{");
		
		// public fields
		foreach (TypeDef field in fieldList)
		{
			writer.WriteLine($"		public {field.Type} {field.Field} {{ get; }} = {field.Param};");
		}

		// Visitor pattern.
		writer.WriteLine();
		writer.WriteLine("		public override T Accept<T>(IVisitor<T> visitor)");
		writer.WriteLine("		{");
		writer.WriteLine($"			return visitor.Visit{className}{baseName}(this);");
		writer.WriteLine("		}");

		writer.WriteLine("	}");
	}

	private record TypeDef(string Type, string Param, string Field);
}
