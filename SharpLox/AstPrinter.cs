﻿using System.Text;

namespace SharpLox;

internal class AstPrinter : Expr.IVisitor<string>
{
	public string Print(Expr expr)
	{
		return expr.Accept(this);
	}

	public string VisitBinaryExpr(Expr.Binary expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
	}

	public string VisitLogicalExpr(Expr.Logical expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
	}

	public string VisitGroupingExpr(Expr.Grouping expr)
	{
		return Parenthesize("group", expr.Expression);
	}

	public string VisitLiteralExpr(Expr.Literal expr)
	{
		return expr.Value?.ToString() ?? "nil";
	}

	public string VisitUnaryExpr(Expr.Unary expr)
	{
		return Parenthesize(expr.Operator.Lexeme, expr.Right);
	}

	public string VisitVariableExpr(Expr.Variable expr)
	{
		return expr.Name.Lexeme;
	}

	public string VisitAssignExpr(Expr.Assign expr)
	{
		return Parenthesize("= " + expr.Name.Lexeme, expr.Value);
	}

	private string Parenthesize(string name, params Expr[] exprs)
	{
		StringBuilder builder = new StringBuilder();

		builder.Append('(');
		builder.Append(name);
		foreach (Expr expr in exprs)
		{
			builder.Append(' ');
			builder.Append(expr.Accept(this));
		}
		builder.Append(')');

		return builder.ToString();
	}

	public string VisitCallExpr(Expr.Call expr)
	{
		return Parenthesize("call", [expr.Callee, .. expr.Arguments]);
	}
}
