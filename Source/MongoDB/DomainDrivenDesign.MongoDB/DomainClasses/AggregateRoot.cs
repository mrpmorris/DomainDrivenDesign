using MongoDB.Bson;
using System;

namespace DomainDrivenDesign.MongoDB.DomainClasses
{
	public class AggregateRoot
	{
		public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
		public int ConcurrencyVersion { get; set; }
		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
		public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;
	}
}
