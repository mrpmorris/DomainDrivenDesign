namespace DomainDrivenDesign.MongoDB.Persistence
{
	public enum EntityState
	{
		Created,
		Modified,
		Unmodified,
		Deleted,
		//TODO: Remove Unknown, use NULL instead
		Unknown
	}
}
