using System.Text;

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
		throw new NotImplementedException();
	}

	public string VisitAssignExpr(Expr.Assign expr)
	{
		throw new NotImplementedException();
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
}
