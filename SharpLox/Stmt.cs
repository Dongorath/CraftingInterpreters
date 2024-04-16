namespace SharpLox;

internal abstract class Stmt
{
	public interface IVisitor<T>
	{
		T VisitExpressionStmt(Expression stmt);
		T VisitPrintStmt(Print stmt);
		T VisitVarStmt(Var stmt);
	}

	public abstract T Accept<T>(IVisitor<T> visitor);

	public class Expression(Expr expres) : Stmt
	{
		public Expr Expres { get; } = expres;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitExpressionStmt(this);
		}
	}

	public class Print(Expr expres) : Stmt
	{
		public Expr Expres { get; } = expres;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitPrintStmt(this);
		}
	}

	public class Var(Token name, Expr? initializer) : Stmt
	{
		public Token Name { get; } = name;
		public Expr? Initializer { get; } = initializer;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitVarStmt(this);
		}
	}
}