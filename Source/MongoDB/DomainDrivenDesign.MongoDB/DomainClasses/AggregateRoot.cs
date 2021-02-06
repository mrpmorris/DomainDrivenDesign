using System;

namespace DomainDrivenDesign.MongoDB.DomainClasses
{
	public abstract class AggregateRoot
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public int ConcurrencyVersion { get; set; }
	}
}
