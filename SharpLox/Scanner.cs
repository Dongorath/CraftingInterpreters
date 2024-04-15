using System.Collections.Frozen;
using static SharpLox.TokenType;

namespace SharpLox;

internal class Scanner(string Source)
{
	private static readonly FrozenDictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
	{
		["and"] = AND,
		["class"] = CLASS,
		["else"] = ELSE,
		["false"] = FALSE,
		["for"] = FOR,
		["fun"] = FUN,
		["if"] = IF,
		["nil"] = NIL,
		["or"] = OR,
		["print"] = PRINT,
		["return"] = RETURN,
		["super"] = SUPER,
		["this"] = THIS,
		["true"] = TRUE,
		["var"] = VAR,
		["while"] = WHILE
	}.ToFrozenDictionary();
	private List<Token> Tokens { get; } = [];

	private int start = 0;
	private int current = 0;
	private int line = 1;

	public List<Token> ScanTokens()
	{
		while (!IsAtEnd())
		{
			// We are at the beginning of the next lexeme.
			start = current;
			ScanToken();
		}

		Tokens.Add(new Token(EOF, "", null, line));
		return Tokens;
	}

	private void ScanToken()
	{
		char c = Advance();
		switch (c)
		{
			// Single character lexeme
			case '(': AddToken(LEFT_PAREN); break;
			case ')': AddToken(RIGHT_PAREN); break;
			case '{': AddToken(LEFT_BRACE); break;
			case '}': AddToken(RIGHT_BRACE); break;
			case ',': AddToken(COMMA); break;
			case '.': AddToken(DOT); break;
			case '-': AddToken(MINUS); break;
			case '+': AddToken(PLUS); break;
			case ';': AddToken(SEMICOLON); break;
			case '*': AddToken(STAR); break;

			// Maybe single character, maybe two characters...
			case '!':
				AddToken(Match('=') ? BANG_EQUAL : BANG);
				break;
			case '=':
				AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
				break;
			case '<':
				AddToken(Match('=') ? LESS_EQUAL : LESS);
				break;
			case '>':
				AddToken(Match('=') ? GREATER_EQUAL : GREATER);
				break;

			// Division or comment ?
			case '/':
				if (Match('/'))
				{
					// A comment goes until the end of the line.
					while (Peek() != '\n' && !IsAtEnd())
						Advance();
				}
				else if (Match('*'))
				{
					while (!(Peek() == '*' && PeekNext() == '/') && !IsAtEnd())
						Advance();
					// Consume final "*/"
					if (!IsAtEnd())
						Advance();
					if (!IsAtEnd())
						Advance();
				}
				else
				{
					AddToken(SLASH);
				}
				break;

			case ' ':
			case '\r':
			case '\t':
				// Ignore whitespace.
				break;

			case '\n':
				line++;
				break;

			case '"': String(); break;

			default:
				if (IsDigit(c))
					Number();
				else if (IsAlpha(c))
					Identifier();
				else
					Lox.Error(line, $"Unexpected character : {c}");
				break;
		}
	}

	private void Identifier()
	{
		while (IsAlphaNumeric(Peek()))
			Advance();

		string text = Source[start..current];
		if (!keywords.TryGetValue(text, out TokenType type))
			type = IDENTIFIER;
		AddToken(type);
	}

	private void Number()
	{
		while (IsDigit(Peek()))
			Advance();

		// Look for a fractional part.
		if (Peek() == '.' && IsDigit(PeekNext()))
		{
			// Consume the "."
			Advance();

			while (IsDigit(Peek()))
				Advance();
		}

		AddToken(NUMBER, double.Parse(Source[start..current], System.Globalization.CultureInfo.InvariantCulture));
	}

	private void String()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n') line++;
			Advance();
		}

		if (IsAtEnd())
		{
			Lox.Error(line, "Unterminated string.");
			return;
		}

		// The closing ".
		Advance();

		// Trim the surrounding quotes.
		string value = Source[(start + 1)..(current - 1)];
		AddToken(STRING, value);
	}

	private bool Match(char expected)
	{
		if (IsAtEnd()) return false;
		if (Source[current] != expected) return false;

		current++;
		return true;
	}

	private char Peek()
	{
		if (IsAtEnd())
			return '\0';
		return Source[current];
	}

	private char PeekNext()
	{
		if (current + 1 >= Source.Length) return '\0';
		return Source[current + 1];
	}

	private static bool IsAlpha(char c)
	{
		return (c >= 'a' && c <= 'z') ||
			   (c >= 'A' && c <= 'Z') ||
				c == '_';
	}

	private static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}

	private static bool IsAlphaNumeric(char c)
	{
		return IsAlpha(c) || IsDigit(c);
	}

	private bool IsAtEnd()
	{
		return current >= Source.Length;
	}

	private char Advance()
	{
		return Source[current++];
	}

	private void AddToken(TokenType type)
	{
		AddToken(type, null);
	}

	private void AddToken(TokenType type, object? literal)
	{
		string text = Source[start..current];
		Tokens.Add(new Token(type, text, literal, line));
	}
}
