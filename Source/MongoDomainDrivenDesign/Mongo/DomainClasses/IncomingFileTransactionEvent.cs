using MongoDB.Bson.Serialization.Attributes;
using System;

namespace MongoDomainDrivenDesign.Mongo.DomainClasses
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
