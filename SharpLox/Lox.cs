﻿namespace SharpLox;

internal class Lox
{
	static bool HadError { get; set; } = false;

	public static void Main(string[] args)
	{
		if (args.Length > 1)
		{
			Console.WriteLine("Usage: SharpLox [script]");
			Environment.Exit(64);
		}
		else if (args.Length == 1)
		{
			RunFile(args[0]);
		}
		else
		{
			RunPrompt();
		}
	}

	private static void RunFile(string path)
	{
		string fileText = File.ReadAllText(path);
		Run(fileText);

		// Indicate an error in the exit code.
		if (HadError)
			Environment.Exit(65);
	}

	private static void RunPrompt()
	{
		while (true)
		{
			Console.Write("> ");
			string? line = Console.ReadLine();
			if (line == null)
				break;
			Run(line);
			HadError = false;
		}
	}

	private static void Run(string source)
	{
		Scanner scanner = new Scanner(source);
		List<Token> tokens = scanner.ScanTokens();

		Parser parser = new Parser(tokens);
		Expr? expression = parser.Parse();

		// Stop if there was a syntax error.
		if (HadError)
			return;

		Console.WriteLine(new AstPrinter().Print(expression!));
	}

	public static void Error(int line, string message)
	{
		Report(line, "", message);
	}

	private static void Report(int line, string where, string message)
	{
		Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
		HadError = true;
	}

	public static void Error(Token token, string message)
	{
		if (token.Type == TokenType.EOF)
		{
			Report(token.Line, " at end", message);
		}
		else
		{
			Report(token.Line, " at '" + token.Lexeme + "'", message);
		}
	}
}
