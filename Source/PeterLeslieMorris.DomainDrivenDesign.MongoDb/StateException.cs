using PeterLeslieMorris.DomainDrivenDesign.MongoDb.Persistence;

namespace PeterLeslieMorris.DomainDrivenDesign.MongoDb
{
	public class StateException : DomainException
	{
		public EntityState State { get; }

		public StateException(string message, EntityState state): base(message)
		{
			State = state;
		}
	}
}
