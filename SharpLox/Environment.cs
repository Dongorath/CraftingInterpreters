namespace SharpLox;

internal class Environment
{
	private SortedDictionary<string, object?> Values { get; } = [];

	public object? Get(Token name)
	{
		string varName = name.Lexeme;
		if (Values.TryGetValue(varName, out object? val))
			return val;

		throw new RuntimeErrorException(name, $"Undefined variable '{varName}'.");
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

		throw new RuntimeErrorException(name, $"Undefined variable '{varName}'.");
	}
}
