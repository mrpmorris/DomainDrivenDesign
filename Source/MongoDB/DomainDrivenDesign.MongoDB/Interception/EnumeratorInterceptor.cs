using System;
using System.Collections;
using System.Collections.Generic;

namespace DomainDrivenDesign.MongoDB.Interception
{
	internal class EnumeratorInterceptor<T> : IEnumerator<T?>
	{
		private T? CurrentValue;
		private readonly Func<object?, object?> InterceptValue;
		private readonly IEnumerator<T?> Source;

		public EnumeratorInterceptor(IEnumerator<T?> source, Func<object?, object?> interceptValue)
		{
			Source = source;
			InterceptValue = interceptValue;
		}

		public T? Current => CurrentValue;

		object? IEnumerator.Current => Current;

		public void Dispose()
		{
			Source.Dispose();
		}

		public bool MoveNext()
		{
			CurrentValue = default;
			bool result = Source.MoveNext();
			if (result)
				CurrentValue = (T?)InterceptValue(Source.Current);
			return result;
		}

		public void Reset()
		{
			CurrentValue = default;
			Source.Reset();
		}
	}
}
