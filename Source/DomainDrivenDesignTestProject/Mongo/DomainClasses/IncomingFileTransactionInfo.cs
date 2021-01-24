using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DomainDriveDesignTestProject.Mongo.DomainClasses
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
