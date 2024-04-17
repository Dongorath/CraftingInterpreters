namespace SharpLox;

internal abstract class Stmt
{
	public interface IVisitor<T>
	{
		T VisitBlockStmt(Block stmt);
		T VisitExpressionStmt(Expression stmt);
		T VisitIfStmt(If stmt);
		T VisitPrintStmt(Print stmt);
		T VisitVarStmt(Var stmt);
		T VisitWhileStmt(While stmt);
	}

	public abstract T Accept<T>(IVisitor<T> visitor);

	public class Block(List<Stmt> statements) : Stmt
	{
		public List<Stmt> Statements { get; } = statements;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBlockStmt(this);
		}
	}

	public class Expression(Expr expres) : Stmt
	{
		public Expr Expres { get; } = expres;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitExpressionStmt(this);
		}
	}

	public class If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
	{
		public Expr Condition { get; } = condition;
		public Stmt ThenBranch { get; } = thenBranch;
		public Stmt? ElseBranch { get; } = elseBranch;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitIfStmt(this);
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

	public class While(Expr condition, Stmt body) : Stmt
	{
		public Expr Condition { get; } = condition;
		public Stmt Body { get; } = body;

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitWhileStmt(this);
		}
	}
}
