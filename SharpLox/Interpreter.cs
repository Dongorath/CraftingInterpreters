using static SharpLox.TokenType;

namespace SharpLox;

internal class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
	public Environment Globals { get; } = new Environment();
	private Dictionary<Expr, int> Locals { get; } = new Dictionary<Expr, int>();
	private Environment Environment { get; set; }

	public Interpreter()
	{
		Environment = Globals;
		Globals.Define("clock", new NativeFunctions.Clock());
	}

	public void Interpret(List<Stmt> statements)
	{
		try
		{
			foreach (Stmt statement in statements)
			{
				Execute(statement);
			}
		}
		catch (RuntimeErrorException error)
		{
			Lox.RuntimeError(error);
		}
	}

	private void Execute(Stmt stmt)
	{
		stmt.Accept(this);
	}

	public void Resolve(Expr expr, int depth)
	{
		Locals[expr] = depth;
	}

	public void ExecuteBlock(List<Stmt> statements,Environment environment)
	{
		Environment previous = Environment;
		try
		{
			Environment = environment;

			foreach (Stmt statement in statements)
			{
				Execute(statement);
			}
		}
		finally
		{
			Environment = previous;
		}
	}

	private object? Evaluate(Expr expr)
	{
		return expr.Accept(this);
	}

	public object? VisitLiteralExpr(Expr.Literal expr)
	{
		return expr.Value;
	}

	public object? VisitLogicalExpr(Expr.Logical expr)
	{
		object? left = Evaluate(expr.Left);

		if (expr.Operator.Type == TokenType.OR)
		{
			if (IsTruthy(left))
				return left;
		}
		else // AND
		{
			if (!IsTruthy(left))
				return left;
		}

		return Evaluate(expr.Right);
	}

	public object? VisitSetExpr(Expr.Set expr)
	{
		object? @object = Evaluate(expr.Object);

		if (@object is not LoxInstance inst)
		{
			throw new RuntimeErrorException(expr.Name, "Only instances have fields.");
		}

		object? value = Evaluate(expr.Value);
		inst.Set(expr.Name, value);
		return value;
	}

	public object? VisitThisExpr(Expr.This expr)
	{
		return LookUpVariable(expr.Keyword, expr);
	}

	public object? VisitGroupingExpr(Expr.Grouping expr)
	{
		return Evaluate(expr.Expression);
	}

	public object? VisitUnaryExpr(Expr.Unary expr)
	{
		object? right = Evaluate(expr.Right);

		switch (expr.Operator.Type)
		{
			case BANG:
				return !IsTruthy(right);
			case MINUS:
				CheckNumberOperand(expr.Operator, right);
#pragma warning disable CS8605 // Unboxing a possibly null value. => CheckNumberOperand would throw if right is not a double
				return -(double)right;
#pragma warning restore CS8605 // Unboxing a possibly null value.
		}

		// Unreachable.
		return null;
	}

	public object? VisitVariableExpr(Expr.Variable expr)
	{
		return LookUpVariable(expr.Name, expr);
	}

	private object? LookUpVariable(Token name, Expr expr)
	{
		if (Locals.TryGetValue(expr, out int distance))
		{
			return Environment.GetAt(distance, name.Lexeme);
		}
		else
		{
			return Globals.Get(name);
		}
	}

	public object? VisitAssignExpr(Expr.Assign expr)
	{
		object? value = Evaluate(expr.Value);
		if (Locals.TryGetValue(expr, out int distance))
		{
			Environment.AssignAt(distance, expr.Name, value);
		}
		else
		{
			Globals.Assign(expr.Name, value);
		}
		return value;
	}

	public object? VisitBinaryExpr(Expr.Binary expr)
	{
		object? left = Evaluate(expr.Left);
		object? right = Evaluate(expr.Right);

		switch (expr.Operator.Type)
		{
			case GREATER:
#pragma warning disable CS8605 // Unboxing a possibly null value. => All types are checked
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left > (double)right;
			case GREATER_EQUAL:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left >= (double)right;
			case LESS:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left < (double)right;
			case LESS_EQUAL:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left <= (double)right;
			case MINUS:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left - (double)right;
			case PLUS:
				if (left is double dl && right is double dr)
					return dl + dr;
				if (left is string sl && right is string sr)
					return sl + sr;
				throw new RuntimeErrorException(expr.Operator, "Operands must be two numbers or two strings.");
			case SLASH:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left / (double)right;
			case STAR:
				CheckNumberOperands(expr.Operator, left, right);
				return (double)left * (double)right;
#pragma warning restore CS8605 // Unboxing a possibly null value.
			case BANG_EQUAL: return !IsEqual(left, right);
			case EQUAL_EQUAL: return IsEqual(left, right);
		}

		// Unreachable.
		return null;
	}

	public object? VisitCallExpr(Expr.Call expr)
	{
		object? callee = Evaluate(expr.Callee);

		List<object?> arguments = new List<object?>();
		foreach (Expr argument in expr.Arguments)
		{
			arguments.Add(Evaluate(argument));
		}

		if (callee is not ILoxCallable function)
		{
			throw new RuntimeErrorException(expr.Paren, "Can only call functions and classes.");
		}
		if (arguments.Count != function.Arity)
		{
			throw new RuntimeErrorException(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
		}

		return function.Call(this, arguments);
	}

	public object? VisitGetExpr(Expr.Get expr)
	{
		object? @object = Evaluate(expr.Object);
		if (@object is LoxInstance inst)
		{
			return inst.Get(expr.Name);
		}

		throw new RuntimeErrorException(expr.Name, "Only instances have properties.");
	}

	private static bool IsTruthy(object? @object)
	{
		if (@object is null) return false;
		if (@object is bool b) return b;
		return true;
	}

	private static bool IsEqual(object? a, object? b)
	{
		if (a == null && b == null) return true;
		if (a == null) return false;

		return a.Equals(b);
	}

	private static string Stringify(object? @object)
	{
		return @object?.ToString() ?? "nil";
	}

	private static void CheckNumberOperand(Token @operator, object? operand)
	{
		if (operand is double) return;
		throw new RuntimeErrorException(@operator, "Operand must be a number.");
	}

	private static void CheckNumberOperands(Token @operator, object? left, object? right)
	{
		if (left is double && right is double) return;

		throw new RuntimeErrorException(@operator, "Operands must be numbers.");
	}

	public object? VisitExpressionStmt(Stmt.Expression stmt)
	{
		Evaluate(stmt.Expres);
		return null;
	}

	public object? VisitFunctionStmt(Stmt.Function stmt)
	{
		LoxFunction function = new LoxFunction(stmt, Environment, false);
		Environment.Define(stmt.Name.Lexeme, function);
		return null;
	}

	public object? VisitIfStmt(Stmt.If stmt)
	{
		if (IsTruthy(Evaluate(stmt.Condition)))
		{
			Execute(stmt.ThenBranch);
		}
		else if (stmt.ElseBranch != null)
		{
			Execute(stmt.ElseBranch);
		}
		return null;
	}

	public object? VisitPrintStmt(Stmt.Print stmt)
	{
		object? value = Evaluate(stmt.Expres);
		Console.WriteLine(Stringify(value));
		return null;
	}

	public object? VisitReturnStmt(Stmt.Return stmt)
	{
		object? value = null;
		if (stmt.Value != null)
			value = Evaluate(stmt.Value);

		throw new ReturnException(value);
	}

	public object? VisitWhileStmt(Stmt.While stmt)
	{
		while (IsTruthy(Evaluate(stmt.Condition)))
		{
			Execute(stmt.Body);
		}
		return null;
	}

	public object? VisitBlockStmt(Stmt.Block stmt)
	{
		ExecuteBlock(stmt.Statements, new Environment(Environment));
		return null;
	}

	public object? VisitClassStmt(Stmt.Class stmt)
	{
		Environment.Define(stmt.Name.Lexeme, null);

		Dictionary<string, LoxFunction> methods = [];
		foreach (Stmt.Function method in stmt.Methods)
		{
			LoxFunction function = new LoxFunction(method, Environment, method.Name.Lexeme == "init");
			methods[method.Name.Lexeme] = function;
		}

		LoxClass klass = new LoxClass(stmt.Name.Lexeme, methods);
		Environment.Assign(stmt.Name, klass);
		return null;
	}

	public object? VisitVarStmt(Stmt.Var stmt)
	{
		object? value = null;
		if (stmt.Initializer != null)
		{
			value = Evaluate(stmt.Initializer);
		}

		Environment.Define(stmt.Name.Lexeme, value);
		return null;
	}
}
