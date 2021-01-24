using System;
using System.ComponentModel.DataAnnotations;

namespace MongoDomainDrivenDesign.EFCore.DomainClasses
{
	public class AggregateRoot
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		[Timestamp]
		public byte[] ConcurrencyVersion { get; set; }
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
		public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
	}
}
