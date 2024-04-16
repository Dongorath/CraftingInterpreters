using static SharpLox.TokenType;

namespace SharpLox;

internal class Parser(List<Token> tokens)
{
	private int current = 0;

	public Expr? Parse()
	{
		try
		{
			return Expression();
		}
		catch (ParseErrorException error)
		{
			return null;
		}
	}

	// expression     → equality ;
	private Expr Expression()
	{
		return Equality();
	}

	// equality       → comparison ( ( "!=" | "==" ) comparison )* ;
	private Expr Equality()
	{
		Expr expr = Comparison();

		while (Match(BANG_EQUAL, EQUAL_EQUAL))
		{
			Token @operator = Previous();
			Expr right = Comparison();
			expr = new Expr.Binary(expr, @operator, right);
		}

		return expr;
	}

	// comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
	private Expr Comparison()
	{
		Expr expr = Term();

		while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
		{
			Token @operator = Previous();
			Expr right = Term();
			expr = new Expr.Binary(expr, @operator, right);
		}

		return expr;
	}

	// term           → factor ( ( "-" | "+" ) factor )* ;
	private Expr Term()
	{
		Expr expr = Factor();

		while (Match(MINUS, PLUS))
		{
			Token @operator = Previous();
			Expr right = Factor();
			expr = new Expr.Binary(expr, @operator, right);
		}

		return expr;
	}

	// factor         → unary ( ( "/" | "*" ) unary )* ;
	private Expr Factor()
	{
		Expr expr = Unary();

		while (Match(SLASH, STAR))
		{
			Token @operator = Previous();
			Expr right = Unary();
			expr = new Expr.Binary(expr, @operator, right);
		}

		return expr;
	}

	// unary          → ( "!" | "-" ) unary
	//                | primary ;
	private Expr Unary()
	{
		if (Match(BANG, MINUS))
		{
			Token @operator = Previous();
			Expr right = Unary();
			return new Expr.Unary(@operator, right);
		}

		return Primary();
	}

	// primary        → NUMBER | STRING | "true" | "false" | "nil"
	//                | "(" expression ")" ;
	private Expr Primary()
	{
		if (Match(FALSE)) return new Expr.Literal(false);
		if (Match(TRUE)) return new Expr.Literal(true);
		if (Match(NIL)) return new Expr.Literal(null);

		if (Match(NUMBER, STRING))
		{
			return new Expr.Literal(Previous().Literal);
		}

		if (Match(LEFT_PAREN))
		{
			Expr expr = Expression();
			Consume(RIGHT_PAREN, "Expect ')' after expression.");
			return new Expr.Grouping(expr);
		}

		throw Error(Peek(), "Expect expression.");
	}

	private bool Match(params TokenType[] types)
	{
		foreach (TokenType type in types)
		{
			if (Check(type))
			{
				Advance();
				return true;
			}
		}

		return false;
	}

	private Token Consume(TokenType type, string message)
	{
		if (Check(type))
			return Advance();

		throw Error(Peek(), message);
	}

	private static ParseErrorException Error(Token token, string message)
	{
		Lox.Error(token, message);
		return new ParseErrorException();
	}

	private void Synchronize()
	{
		Advance();

		while (!IsAtEnd())
		{
			if (Previous().Type == SEMICOLON) return;

			switch (Peek().Type)
			{
				case CLASS:
				case FUN:
				case VAR:
				case FOR:
				case IF:
				case WHILE:
				case PRINT:
				case RETURN:
					return;
			}

			Advance();
		}
	}

	private bool Check(TokenType type)
	{
		if (IsAtEnd())
			return false;
		return Peek().Type == type;
	}

	private Token Advance()
	{
		if (!IsAtEnd())
			current++;
		return Previous();
	}

	private bool IsAtEnd()
	{
		return Peek().Type == EOF;
	}

	private Token Peek()
	{
		return tokens[current];
	}

	private Token Previous()
	{
		return tokens[current - 1];
	}

	private class ParseErrorException : Exception { }
}
