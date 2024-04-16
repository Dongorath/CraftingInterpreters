﻿using static SharpLox.TokenType;

namespace SharpLox;

internal class Interpreter : Expr.IVisitor<object?>
{
	public void Interpret(Expr expression)
	{
		try
		{
			object? value = Evaluate(expression);
			Console.WriteLine(Stringify(value));
		}
		catch (RuntimeErrorException error)
		{
			Lox.RuntimeError(error);
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
}