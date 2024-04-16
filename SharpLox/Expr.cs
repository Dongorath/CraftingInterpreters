namespace SharpLox;

internal abstract class Expr
{
	public interface IVisitor<T>
	{
		T VisitBinaryExpr(Binary expr);
		T VisitGroupingExpr(Grouping expr);
		T VisitLiteralExpr(Literal expr);
		T VisitUnaryExpr(Unary expr);
	}

	public abstract T Accept<T>(IVisitor<T> visitor);

	public class Binary(Expr left, Token @operator, Expr right) : Expr
	{
		public Expr Left { get; } = left;
		public Token Operator { get; } = @operator;
		public Expr Right { get; } = right;


		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBinaryExpr(this);
		}
	}

	public class Grouping(Expr expression) : Expr
	{
		public Expr Expression { get; } = expression;


		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitGroupingExpr(this);
		}
	}

	public class Literal(object? value) : Expr
	{
		public object? Value { get; } = value;


		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLiteralExpr(this);
		}
	}

	public class Unary(Token @operator, Expr right) : Expr
	{
		public Token Operator { get; } = @operator;
		public Expr Right { get; } = right;


		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitUnaryExpr(this);
		}
	}
}
