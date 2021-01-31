using System;
using System.Collections;
using System.Collections.Generic;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class AggregateRootEnumeratorInterceptor<T> : IEnumerator<T>
	{
		private T? CurrentValue;
		private readonly Func<object, object> InterceptValue;
		private readonly IEnumerator<T> Source;

		public AggregateRootEnumeratorInterceptor(IEnumerator<T> source, Func<object, object> interceptValue)
		{
			Source = source;
			InterceptValue = interceptValue;
		}

		public T Current => CurrentValue ?? throw new NullReferenceException();

		object IEnumerator.Current => Current ?? throw new NullReferenceException();

		public void Dispose()
		{
			Source.Dispose();
		}

		public bool MoveNext()
		{
			CurrentValue = default;
			bool result = Source.MoveNext();
			if (result)
				CurrentValue = (T)InterceptValue(Source.Current!);
			return result;
		}

		public void Reset()
		{
			CurrentValue = default;
			Source.Reset();
		}
	}
}
