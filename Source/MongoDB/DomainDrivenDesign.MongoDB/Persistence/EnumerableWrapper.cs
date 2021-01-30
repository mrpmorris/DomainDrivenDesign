// This approach isn't working
//using DomainDrivenDesign.MongoDB.DomainClasses;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;

//namespace DomainDrivenDesign.MongoDB.Persistence
//{
//	internal class QueryableWrapper<T> : IQueryable<T>
//	{
//		private string CollectionName;
//		private readonly IQueryable<T> Source;
//		private readonly DbContext DbContext;
//		private readonly IQueryProvider QueryProvider;

//		public QueryableWrapper(
//			string collectionName,
//			DbContext dbContext,
//			IQueryable<T> source)
//		{
//			CollectionName = collectionName;
//			DbContext = dbContext;
//			Source = source;
//			QueryProvider = new QueryProviderWrapper<T>(collectionName, dbContext, source.Provider);
//		}

//		public Type ElementType => Source.ElementType;

//		public Expression Expression => Source.Expression;

//		public IQueryProvider Provider => QueryProvider;

//		public IEnumerator<T> GetEnumerator() =>
//			new EnumeratorWrapper<T>(CollectionName, DbContext, Source.GetEnumerator());

//		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//	}


//	internal class EnumeratorWrapper<T> : IEnumerator<T>
//	{
//		private T? CachedEntity;
//		private readonly string CollectionName;
//		private readonly DbContext DbContext;
//		private readonly IEnumerator<T> Source;

//		public EnumeratorWrapper(
//			string collectionName,
//			DbContext dbContext,
//			IEnumerator<T> source)
//		{
//			CollectionName = collectionName;
//			DbContext = dbContext;
//			Source = source;
//		}

//		public T Current =>
//			CachedEntity is not null
//				? CachedEntity
//				: Source.Current;

//		object? IEnumerator.Current => Current;

//		public void Dispose()
//		{
//			Source.Dispose();
//		}

//		public bool MoveNext()
//		{
//			CachedEntity = default;
//			bool result = Source.MoveNext();
//			if (result)
//			{
//				if (Source.Current is AggregateRoot entity)
//				{
//					EntityEntry entry = DbContext.GetEntry(CollectionName, entity);
//					switch (entry.State)
//					{
//						case EntityState.Created:
//						case EntityState.Deleted:
//						case EntityState.Modified:
//						case EntityState.Unmodified:
//							if (entry.Entity is T stronglyTypedEntity)
//								CachedEntity = stronglyTypedEntity;
//							else
//								throw new InvalidCastException(
//									$"Cached entity '{entry.Entity.GetType().FullName}' must be of type" +
//									$" '{typeof(T).FullName}'");
//							break;

//						case EntityState.Unknown:
//							DbContext.Attach(typeof(T), CollectionName, Source.Current);
//							break;

//						default:
//							throw new NotImplementedException(entry.State.ToString());
//					}
//				}
//			}
//			return result;
//		}

//		public void Reset()
//		{
//			CachedEntity = default;
//			Source.Reset();
//		}
//	}

//	internal class QueryProviderWrapper<T> : IQueryProvider
//	{
//		private string CollectionName;
//		private readonly IQueryProvider Source;
//		private readonly DbContext DbContext;

//		public QueryProviderWrapper(
//			string collectionName,
//			DbContext dbContext,
//			IQueryProvider source)
//		{
//			CollectionName = collectionName;
//			DbContext = dbContext;
//			Source = source;
//		}

//		public IQueryable CreateQuery(Expression expression) => CreateQuery<T>(expression);

//		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
//		{
//			IQueryable<TElement> result = Source.CreateQuery<TElement>(expression);
//			return new QueryableWrapper<TElement>(
//				collectionName: CollectionName,
//				dbContext: DbContext,
//				source: result);
//		}

//		public object? Execute(Expression expression) => Execute<T>(expression);

//		public TResult Execute<TResult>(Expression expression) => Source.Execute<TResult>(expression);
//	}
//}
