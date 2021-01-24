using System.Collections.Generic;

namespace MongoDomainDrivenDesign.EFCore.DomainClasses
{
	public class IncomingFileTransaction : AggregateRoot
	{
		public IReadOnlyList<IncomingFileTransactionInfo> Infos { get; set; }
		public IReadOnlyList<IncomingFileTransactionEvent> Events { get; set; }

		public IncomingFileTransaction()
		{
			Infos = new List<IncomingFileTransactionInfo>()
			{
				new IncomingFileTransactionInfo(mpxn: "1"),
				new IncomingFileTransactionInfo(mpxn: "2"),
				new IncomingFileTransactionInfo(mpxn: "3"),
				new IncomingFileTransactionInfo(mpxn: "4"),
				new IncomingFileTransactionInfo(mpxn: "5"),
			}.AsReadOnly();

			Events = new List<IncomingFileTransactionEvent>()
			{
				new IncomingFileTransactionEvent(IncomingFileTransactionEventType.Created),
				new IncomingFileTransactionEvent(IncomingFileTransactionEventType.Validated),
				new IncomingFileTransactionEvent(IncomingFileTransactionEventType.Sending),
				new IncomingFileTransactionEvent(IncomingFileTransactionEventType.Sent),
				new IncomingFileTransactionEvent(IncomingFileTransactionEventType.Acknowledged),
			}.AsReadOnly();
		}
	}
}
