namespace SharpLox;

internal class Environment
{
	public Environment? Enclosing { get; }
	private SortedDictionary<string, object?> Values { get; } = [];

	public Environment()
	{
		Enclosing = null;
	}

	public Environment(Environment enclosing)
	{
		Enclosing = enclosing;
	}

	private Environment Ancestor(int distance)
	{
		Environment environment = this;
		for (int i = 0; i < distance; i++)
		{
			environment = environment.Enclosing!;
		}

		return environment;
	}

	public object? Get(Token name)
	{
		string varName = name.Lexeme;
		if (Values.TryGetValue(varName, out object? val))
			return val;

		if (Enclosing != null)
			return Enclosing.Get(name);

		throw new RuntimeErrorException(name, $"Undefined variable '{varName}'.");
	}

	public object? GetAt(int distance, string name)
	{
		return Ancestor(distance).Values[name];
	}

	public void Define(string name, object? value)
	{
		Values[name] = value;
	}

	public void Assign(Token name, object? value)
	{
		string varName = name.Lexeme;
		if (Values.ContainsKey(varName))
		{
			Values[varName] = value;
			return;
		}

		if (Enclosing != null)
		{
			Enclosing.Assign(name, value);
			return;
		}

		throw new RuntimeErrorException(name, $"Undefined variable '{varName}'.");
	}
	public void AssignAt(int distance, Token name, object? value)
	{
		Ancestor(distance).Values[name.Lexeme] = value;
	}
}
