using static System.Formats.Asn1.AsnWriter;

namespace SharpLox;

internal class Resolver(Interpreter interpreter) : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
	private enum FunctionType
	{
		NONE,
		FUNCTION,
		INITIALIZER,
		METHOD
	}

	private enum ClassType
	{
		NONE,
		CLASS
	}

	private Interpreter Interpreter { get; } = interpreter;
	private Stack<Dictionary<string, bool>> Scopes { get; } = [];
	private FunctionType CurrentFunction { get; set; } = FunctionType.NONE;
	private ClassType CurrentClass { get; set; } = ClassType.NONE;

	public void Resolve(List<Stmt> statements)
	{
		foreach (Stmt statement in statements)
		{
			Resolve(statement);
		}
	}

	private void Resolve(Stmt stmt)
	{
		stmt.Accept(this);
	}

	private void Resolve(Expr expr)
	{
		expr.Accept(this);
	}

	private void BeginScope()
	{
		Scopes.Push([]);
	}

	private void EndScope()
	{
		Scopes.Pop();
	}

	private void Declare(Token name)
	{
		if (Scopes.Count == 0)
			return;

		Dictionary<string, bool> scope = Scopes.Peek();
		if (scope.ContainsKey(name.Lexeme))
		{
			Lox.Error(name, "Already a variable with this name in this scope.");
		}
		scope[name.Lexeme] = false;
	}

	private void Define(Token name)
	{
		if (Scopes.Count == 0)
			return;
		Scopes.Peek()[name.Lexeme] =  true;
	}

	private void ResolveLocal(Expr expr, Token name)
	{
#pragma warning disable IDE0305 // Simplify collection initialization => ToArray() seems more performant...
		Dictionary<string, bool>[] stack = Scopes.ToArray();
#pragma warning restore IDE0305 // Simplify collection initialization
		for (int i = 0; i < stack.Length; i++)
		{
			if (stack[i].ContainsKey(name.Lexeme))
			{
				Interpreter.Resolve(expr, i);
				return;
			}
		}
	}

	private void ResolveFunction(Stmt.Function function, FunctionType type)
	{
		FunctionType enclosingFunction = CurrentFunction;
		CurrentFunction = type;

		BeginScope();
		foreach (Token param in function.Params)
		{
			Declare(param);
			Define(param);
		}
		Resolve(function.Body);
		EndScope();

		CurrentFunction = enclosingFunction;
	}

	public object? VisitBlockStmt(Stmt.Block stmt)
	{
		BeginScope();
		Resolve(stmt.Statements);
		EndScope();
		return null;
	}

	public object? VisitClassStmt(Stmt.Class stmt)
	{
		ClassType enclosingClass = CurrentClass;
		CurrentClass = ClassType.CLASS;

		Declare(stmt.Name);
		Define(stmt.Name);

		BeginScope();
		Scopes.Peek()["this"] = true;

		foreach (Stmt.Function method in stmt.Methods)
		{
			FunctionType declaration = FunctionType.METHOD;
			if (method.Name.Lexeme == "init")
				declaration = FunctionType.INITIALIZER;

			ResolveFunction(method, declaration);
		}

		EndScope();

		CurrentClass = enclosingClass;

		return null;
	}

	public object? VisitExpressionStmt(Stmt.Expression stmt)
	{
		Resolve(stmt.Expres);
		return null;
	}

	public object? VisitFunctionStmt(Stmt.Function stmt)
	{
		Declare(stmt.Name);
		Define(stmt.Name);

		ResolveFunction(stmt, FunctionType.FUNCTION);
		return null;
	}

	public object? VisitIfStmt(Stmt.If stmt)
	{
		Resolve(stmt.Condition);
		Resolve(stmt.ThenBranch);
		if (stmt.ElseBranch != null)
			Resolve(stmt.ElseBranch);
		return null;
	}

	public object? VisitPrintStmt(Stmt.Print stmt)
	{
		Resolve(stmt.Expres);
		return null;
	}

	public object? VisitReturnStmt(Stmt.Return stmt)
	{
		if (CurrentFunction == FunctionType.NONE)
		{
			Lox.Error(stmt.Keyword, "Can't return from top-level code.");
		}

		if (stmt.Value != null)
		{
			if (CurrentFunction == FunctionType.INITIALIZER)
			{
				Lox.Error(stmt.Keyword, "Can't return a value from an initializer.");
			}
			Resolve(stmt.Value);
		}

		return null;
	}

	public object? VisitVarStmt(Stmt.Var stmt)
	{
		Declare(stmt.Name);
		if (stmt.Initializer != null)
		{
			Resolve(stmt.Initializer);
		}
		Define(stmt.Name);
		return null;
	}

	public object? VisitWhileStmt(Stmt.While stmt)
	{
		Resolve(stmt.Condition);
		Resolve(stmt.Body);
		return null;
	}

	public object? VisitAssignExpr(Expr.Assign expr)
	{
		Resolve(expr.Value);
		ResolveLocal(expr, expr.Name);
		return null;
	}

	public object? VisitBinaryExpr(Expr.Binary expr)
	{
		Resolve(expr.Left);
		Resolve(expr.Right);
		return null;
	}

	public object? VisitCallExpr(Expr.Call expr)
	{
		Resolve(expr.Callee);

		foreach (Expr argument in expr.Arguments)
		{
			Resolve(argument);
		}

		return null;
	}

	public object? VisitGetExpr(Expr.Get expr)
	{
		Resolve(expr.Object);
		return null;
	}

	public object? VisitGroupingExpr(Expr.Grouping expr)
	{
		Resolve(expr.Expression);
		return null;
	}

	public object? VisitLiteralExpr(Expr.Literal expr)
	{
		return null;
	}

	public object? VisitLogicalExpr(Expr.Logical expr)
	{
		Resolve(expr.Left);
		Resolve(expr.Right);
		return null;
	}

	public object? VisitSetExpr(Expr.Set expr)
	{
		Resolve(expr.Value);
		Resolve(expr.Object);
		return null;
	}

	public object? VisitThisExpr(Expr.This expr)
	{
		if (CurrentClass == ClassType.NONE)
		{
			Lox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
			return null;
		}

		ResolveLocal(expr, expr.Keyword);
		return null;
	}


	public object? VisitUnaryExpr(Expr.Unary expr)
	{
		Resolve(expr.Right);
		return null;
	}

	public object? VisitVariableExpr(Expr.Variable expr)
	{
		if (Scopes.Count > 0 &&
			Scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool b) && b == false)
		{
			Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
		}

		ResolveLocal(expr, expr.Name);
		return null;
	}
}
