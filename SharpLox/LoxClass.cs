namespace SharpLox;

internal class LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
	public LoxClass? Superclass { get; } = superclass;
	public string Name { get; } = name;
	private Dictionary<string, LoxFunction> Methods { get; } = methods;

	public int Arity => FindMethod("init")?.Arity ?? 0;

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		LoxInstance instance = new LoxInstance(this);
		FindMethod("init")?.Bind(instance).Call(interpreter, arguments);
		return instance;
	}

	public LoxFunction? FindMethod(string name)
	{
		if (Methods.TryGetValue(name, out LoxFunction? method))
		{
			return method;
		}

		if (Superclass != null)
		{
			return Superclass.FindMethod(name);
		}

		return null;
	}

	public override string ToString()
	{
		return Name;
	}
}
