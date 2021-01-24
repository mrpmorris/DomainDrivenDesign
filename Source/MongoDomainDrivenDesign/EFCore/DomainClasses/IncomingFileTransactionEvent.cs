using System;

namespace MongoDomainDrivenDesign.EFCore.DomainClasses
{
	public class IncomingFileTransactionEvent
	{
		public Guid Id { get; set; }
		public Guid IncomingFileTransactionId { get; set; }
		public IncomingFileTransactionEventType Type { get; set; }

		[Obsolete]
		public IncomingFileTransactionEvent() { }

		public IncomingFileTransactionEvent(IncomingFileTransactionEventType type)
		{
			Type = type;
		}
	}
}
