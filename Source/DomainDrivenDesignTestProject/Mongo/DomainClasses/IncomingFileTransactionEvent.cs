using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DomainDrivenDesignTestProject.Mongo.DomainClasses
{
	public class IncomingFileTransactionEvent
	{
		public IncomingFileTransactionEventType Type { get; set; }

		[Obsolete]
		public IncomingFileTransactionEvent() { }

		public IncomingFileTransactionEvent(IncomingFileTransactionEventType type)
		{
			Type = type;
		}
	}
}
