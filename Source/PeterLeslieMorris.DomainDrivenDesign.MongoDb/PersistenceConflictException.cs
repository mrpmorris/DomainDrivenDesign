namespace PeterLeslieMorris.DomainDrivenDesign.MongoDb
{
	public class PersistenceConflictException : DomainException
	{
		public PersistenceConflictException() : base("One more more objects were altered") { }
	}
}
