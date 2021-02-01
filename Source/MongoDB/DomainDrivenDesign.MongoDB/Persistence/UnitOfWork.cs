using DomainDrivenDesign.MongoDB.DomainClasses;
using DomainDrivenDesign.MongoDB.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Persistence
{
	public interface IUnitOfWork<TDbContext, TValidationError>
		where TDbContext: DbContext
	{
		Task<TValidationError[]> CommitAsync();
	}

	public class UnitOfWork<TDbContext, TValidationError> : IUnitOfWork<TDbContext, TValidationError>
		where TDbContext: DbContext
	{
		private readonly TDbContext DbContext;
		private readonly IValidationService<TDbContext, TValidationError> ValidationService;

		public UnitOfWork(TDbContext dbContext, IValidationService<TDbContext, TValidationError> validationService)
		{
			DbContext = dbContext;
			ValidationService = validationService;
		}

		public async Task<TValidationError[]> CommitAsync()
		{
			IEnumerable<AggregateRoot> entities = DbContext.GetEntries()
				.Where(x =>
					x.State == EntityState.Created
					|| x.State == EntityState.Modified)
				.Select(x => x.Entity);

			TValidationError[] validationErrors = await ValidationService
				.ValidateAsync(entities)
				.ConfigureAwait(false);

			if (validationErrors.Length > 0)
				return validationErrors;

			await DbContext.SaveChangesAsync().ConfigureAwait(false);
			return Array.Empty<TValidationError>();
		}
	}
}
