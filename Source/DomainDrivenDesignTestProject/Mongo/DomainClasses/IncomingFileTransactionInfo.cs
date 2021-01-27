using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DomainDrivenDesignTestProject.Mongo.DomainClasses
{
	public class IncomingFileTransactionInfo
	{
		public string Mpxn { get; set; }

		[Obsolete]
		public IncomingFileTransactionInfo() { }

		public IncomingFileTransactionInfo(string mpxn)
		{
			Mpxn = mpxn ?? throw new ArgumentNullException(nameof(mpxn));
		}
	}
}
