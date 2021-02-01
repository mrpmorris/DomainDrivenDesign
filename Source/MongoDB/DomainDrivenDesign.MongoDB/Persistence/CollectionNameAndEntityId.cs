using System;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public class CollectionNameAndEntityId : IEquatable<CollectionNameAndEntityId>
	{
		public string CollectionName { get; }
		public Guid EntityId { get; }

		private int? CachedHashCode;

		public CollectionNameAndEntityId(string collectionName, Guid entityId)
		{
			CollectionName = collectionName;
			EntityId = entityId;
		}

		public static bool operator ==(CollectionNameAndEntityId a, CollectionNameAndEntityId b) => a.Equals(b);
		public static bool operator !=(CollectionNameAndEntityId a, CollectionNameAndEntityId b) => !a.Equals(b);

		public override int GetHashCode()
		{
			if (CachedHashCode is not null)
				return CachedHashCode.Value;

			unchecked
			{
				CachedHashCode = CollectionName.GetHashCode() * 23 + EntityId.GetHashCode();
			}
			return CachedHashCode.Value;
		}

		public override string ToString() =>
			$"{CollectionName}={EntityId}";

		public override bool Equals(object obj)
		{
			CollectionNameAndEntityId? other = obj as CollectionNameAndEntityId;
			if (other is null)
				return false;

			return Equals(other);
		}

		public bool Equals(CollectionNameAndEntityId other) =>
			other.EntityId == EntityId && other.CollectionName == CollectionName;
	}
}
