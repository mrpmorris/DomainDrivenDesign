using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class AggregateRootQueryProviderInterceptor<T> : IQueryProvider
	{
		private readonly IQueryProvider Source;
		private readonly DbContext DbContext;
		private readonly string CollectionName;
		private readonly Func<object, object> InterceptValue;

		public AggregateRootQueryProviderInterceptor(
			IQueryProvider source,
			DbContext dbContext,
			string collectionName,
			Func<object, object> interceptValue)
		{
			CollectionName = collectionName;
			DbContext = dbContext;
			Source = source;
			InterceptValue = interceptValue;
		}

		public IQueryable CreateQuery(Expression expression) => CreateQuery<T>(expression);

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			IQueryable<TElement> result = Source.CreateQuery<TElement>(expression);

			// Don't intercept if not the source type
			if (!typeof(T).IsAssignableFrom(typeof(TElement)))
				return result;

			return new AggregateRootQueryableInterceptor<TElement>(
				result,
				DbContext,
				CollectionName,
				InterceptValue);
		}

		public object Execute(Expression expression) => Execute<T>(expression)!;

		public TResult Execute<TResult>(Expression expression)
		{
			TResult result = Source.Execute<TResult>(expression);
			if (result is T)
				DbContext.Attach(typeof(T), CollectionName, result);
			return result;
		}
			
	}
}
