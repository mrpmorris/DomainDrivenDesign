namespace DomainDrivenDesignTestProject.Mongo.DomainClasses
{
	public enum IncomingFileTransactionEventType
	{
		Created = 0,
		Validated = 1,
		Sending = 2,
		Sent = 3,
		Acknowledged = 4
	}
}
