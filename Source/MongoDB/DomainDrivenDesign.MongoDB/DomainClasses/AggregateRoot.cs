using System;

namespace DomainDrivenDesign.MongoDB.DomainClasses
{
	public class AggregateRoot
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public int ConcurrencyVersion { get; set; }
	}
}
