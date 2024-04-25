namespace SharpLox;

internal class LoxInstance(LoxClass klass)
{
	private LoxClass Klass { get; } = klass;
	private Dictionary<string, object?> Fields { get; } = [];

	public object? Get(Token name)
	{
		if (Fields.TryGetValue(name.Lexeme, out object? val))
		{
			return val;
		}

		LoxFunction? method = Klass.FindMethod(name.Lexeme);
		if (method != null)
			return method.Bind(this);

		throw new RuntimeErrorException(name, $"Undefined property '{name.Lexeme}'.");
	}

	public void Set(Token name, object? value)
	{
		Fields[name.Lexeme] = value;
	}

	public override string ToString()
	{
		return $"{Klass.Name} instance";
	}
}
