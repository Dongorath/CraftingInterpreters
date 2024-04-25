namespace SharpLox;

internal class LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer) : ILoxCallable
{
	private Stmt.Function Declaration { get; } = declaration;
	private Environment Closure { get; } = closure;
	private bool IsInitializer { get; } = isInitializer;

	public int Arity => Declaration.Params.Count;

	public object? Call(Interpreter interpreter, List<object?> arguments)
	{
		Environment environment = new Environment(Closure);
		for (int i = 0; i < Declaration.Params.Count; i++)
		{
			environment.Define(Declaration.Params[i].Lexeme, arguments[i]);
		}

		try
		{
			interpreter.ExecuteBlock(Declaration.Body, environment);
		}
		catch (ReturnException retVal)
		{
			if (IsInitializer)
				return Closure.GetAt(0, "this");
			return retVal.Value;
		}

		if (IsInitializer)
			return Closure.GetAt(0, "this");
		return null;
	}

	public LoxFunction Bind(LoxInstance instance)
	{
		Environment environment = new Environment(Closure);
		environment.Define("this", instance);
		return new LoxFunction(Declaration, environment, IsInitializer);
	}

	public override string ToString()
	{
		return "<fn " + Declaration.Name.Lexeme + ">";
	}
}
