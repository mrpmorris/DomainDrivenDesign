using DomainDrivenDesign.MongoDB.DomainClasses;
using System;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public class EntityEntry : IEquatable<EntityEntry>
	{
		public string CollectionName { get; }
		public AggregateRoot Entity { get; }
		public EntityState State { get; }

		private int? CachedHashCode;

		public EntityEntry(string collectionName, AggregateRoot entity, EntityState state)
		{
			Entity = entity;
			CollectionName = collectionName;
			State = state;
		}

		public static bool operator ==(EntityEntry a, EntityEntry b) => a.Equals(b);
		public static bool operator !=(EntityEntry a, EntityEntry b) => !a.Equals(b);

		public override string ToString() =>
			$"{Entity.GetType().FullName}:{Entity.Id}={State}";

		public override bool Equals(object obj)
		{
			EntityEntry? other = obj as EntityEntry;
			if (other is null)
				return false;

			return Equals(other);
		}

		public override int GetHashCode()
		{
			if (CachedHashCode is not null)
				return CachedHashCode.Value;

			CachedHashCode = Entity.Id.GetHashCode();
			return CachedHashCode.Value;
		}

		public bool Equals(EntityEntry other) => other.Entity.Id == Entity.Id;
	}
}
