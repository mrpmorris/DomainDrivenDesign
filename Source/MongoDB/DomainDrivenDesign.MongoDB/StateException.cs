using DomainDrivenDesign.MongoDB.Persistence;

namespace DomainDrivenDesign.MongoDB
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
