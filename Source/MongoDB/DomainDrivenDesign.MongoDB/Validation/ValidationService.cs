using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Validation
{
	public interface IValidationService<TDbContext, TValidationError>
		where TDbContext : DbContext
	{
		Task<TValidationError[]> ValidateAsync(object entity);
	}

	public class NullValidationService<TDbContext> : IValidationService<TDbContext, ValidationError>
		where TDbContext : DbContext
	{
		public Task<ValidationError[]> ValidateAsync(object entity)
		=>
			Task.FromResult(Array.Empty<ValidationError>());
	}
}