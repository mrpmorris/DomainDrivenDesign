using System;

namespace DomainDrivenDesign.MongoDB.DomainClasses
{
	public class AggregateRoot
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public int ConcurrencyVersion { get; set; }
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
		public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
	}
}
