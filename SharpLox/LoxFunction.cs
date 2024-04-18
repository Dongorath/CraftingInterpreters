namespace SharpLox;

internal class LoxFunction(Stmt.Function declaration, Environment closure) : ILoxCallable
{
	private Stmt.Function Declaration { get; } = declaration;
	private Environment Closure { get; } = closure;

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
			return retVal.Value;
		}
		return null;
	}

	public override string ToString()
	{
		return "<fn " + Declaration.Name.Lexeme + ">";
	}
}
