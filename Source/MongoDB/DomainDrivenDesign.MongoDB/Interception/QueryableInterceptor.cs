using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class QueryableInterceptor<T> : IQueryable<T>, IOrderedQueryable<T>
	{
		private readonly Func<object, object> InterceptValue;
		private readonly IQueryable<T> Source;
		private readonly Lazy<IQueryProvider> QueryProvider;

		public QueryableInterceptor(IQueryable<T?> source, Func<object?, object?> interceptValue)
		{
			Source = source;
			InterceptValue = interceptValue;
			QueryProvider = new Lazy<IQueryProvider>(() =>
				new QueryProviderInterceptor<T?>(source.Provider, interceptValue));
		}

		public Type ElementType => Source.ElementType;

		public Expression Expression => Source.Expression;

		public IQueryProvider Provider => QueryProvider.Value;

		public IEnumerator<T?> GetEnumerator() =>
			new EnumeratorInterceptor<T>(Source.GetEnumerator(), InterceptValue);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
