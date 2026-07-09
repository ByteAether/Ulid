using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Interceptors;

namespace ByteAether.Ulid.Linq2Db;

internal class UlidGuardInterceptor : IQueryExpressionInterceptor
{
	private class Visitor : ExpressionVisitor
	{
		protected override Expression VisitBinary(BinaryExpression node)
		{
			if (node.NodeType == ExpressionType.GreaterThanOrEqual ||
				node.NodeType == ExpressionType.LessThanOrEqual    ||
				node.NodeType == ExpressionType.GreaterThan        ||
				node.NodeType == ExpressionType.LessThan)
			{
				// Works perfectly for your target Ulid type!
				if (node.Left.Type == typeof(Ulid) || node.Right.Type == typeof(Ulid))
				{
					throw new LinqToDBException("Ulid mathematical comparisons (>, <, >=, <=) are prohibited in SQL.");
				}
			}
			return base.VisitBinary(node);
		}
	}

	/// <inheritdoc />
	public Expression ProcessExpression(Expression expression, QueryExpressionArgs args)
	{
		new Visitor().Visit(expression);
		return expression;
	}
}