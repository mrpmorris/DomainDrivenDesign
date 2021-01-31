using DomainDrivenDesign.MongoDB.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Validation
{
	public interface ValidationService<TDbContext>
		where TDbContext : DbContext
	{
		Task<ValidationError[]> ValidateAsync(IEnumerable<object> entities);
	}

	public class NullValidationService<TDbContext> : ValidationService<TDbContext>
		where TDbContext : DbContext
	{
		public Task<ValidationError[]> ValidateAsync(IEnumerable<object> entities)
		=>
			Task.FromResult(Array.Empty<ValidationError>());
	}
}