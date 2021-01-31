using System;
using System.Linq;
using System.Linq.Expressions;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class QueryProviderInterceptor<T> : IQueryProvider
	{
		private readonly Func<object?, object?> InterceptValue;
		private readonly IQueryProvider Source;

		public QueryProviderInterceptor(IQueryProvider source, Func<object?, object?> interceptValue)
		{
			Source = source;
			InterceptValue = interceptValue;
		}

		public IQueryable CreateQuery(Expression expression) => CreateQuery<T?>(expression);

		public IQueryable<TElement?> CreateQuery<TElement>(Expression expression)
		{
			IQueryable<TElement?> result = Source.CreateQuery<TElement?>(expression);

			// Don't intercept if not the source type
			if (!typeof(T).IsAssignableFrom(typeof(TElement)))
				return result;

			return new QueryableInterceptor<TElement?>(result, InterceptValue);
		}

		public object? Execute(Expression expression) => Execute<T?>(expression);

		public TResult Execute<TResult>(Expression expression) => Source.Execute<TResult>(expression);
	}
}
