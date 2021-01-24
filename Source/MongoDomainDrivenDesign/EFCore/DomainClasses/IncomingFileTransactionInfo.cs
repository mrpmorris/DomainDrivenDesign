using System;

namespace MongoDomainDrivenDesign.EFCore.DomainClasses
{
	public class IncomingFileTransactionInfo
	{
		public Guid Id { get; set; }
		public Guid IncomingFileTransactionId { get; set; }
		public string Mpxn { get; set; }

		[Obsolete]
		public IncomingFileTransactionInfo() { }

		public IncomingFileTransactionInfo(string mpxn)
		{
			Mpxn = mpxn ?? throw new ArgumentNullException(nameof(mpxn));
		}
	}
}
