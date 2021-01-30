using DomainDrivenDesign.MongoDB.DomainClasses;
using DomainDrivenDesign.MongoDB.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public interface IUnitOfWork<TDbContext>
		where TDbContext: DbContext
	{
		Task<ValidationError[]> SaveChangesAsync();
	}

	public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
		where TDbContext: DbContext
	{
		private readonly TDbContext DbContext;
		private readonly IValidationService ValidationService;

		public UnitOfWork(TDbContext dbContext, IValidationService validationService)
		{
			DbContext = dbContext;
			ValidationService = validationService;
		}

		public async Task<ValidationError[]> SaveChangesAsync()
		{
			IEnumerable<AggregateRoot> entities = DbContext.GetEntries()
				.Where(x =>
					x.State == EntityState.Created
					|| x.State == EntityState.Modified)
				.Select(x => x.Entity);

			ValidationError[] validationErrors = await ValidationService
				.ValidateAsync(entities)
				.ConfigureAwait(false);

			if (validationErrors.Length > 0)
				return validationErrors;

			await DbContext.SaveChangesAsync().ConfigureAwait(false);
			return Array.Empty<ValidationError>();
		}
	}
}
