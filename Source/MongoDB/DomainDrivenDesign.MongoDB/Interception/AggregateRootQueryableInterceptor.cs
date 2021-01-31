using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class AggregateRootQueryableInterceptor<T> : IQueryable<T>, IOrderedQueryable<T>
	{
		private readonly IQueryable<T> Source;
		private readonly Func<object, object> InterceptValue;
		private readonly Lazy<IQueryProvider> QueryProvider;

		public AggregateRootQueryableInterceptor(
			IQueryable<T> source,
			DbContext dbContext,
			string collectionName,
			Func<object, object> interceptValue)
		{
			Source = source;
			InterceptValue = interceptValue;
			QueryProvider = new Lazy<IQueryProvider>(() =>
				new AggregateRootQueryProviderInterceptor<T>(
					source.Provider,
					dbContext,
					collectionName,
					interceptValue));
		}

		public Type ElementType => Source.ElementType;

		public Expression Expression => Source.Expression;

		public IQueryProvider Provider => QueryProvider.Value;

		public IEnumerator<T> GetEnumerator() =>
			new AggregateRootEnumeratorInterceptor<T>(Source.GetEnumerator(), InterceptValue);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
