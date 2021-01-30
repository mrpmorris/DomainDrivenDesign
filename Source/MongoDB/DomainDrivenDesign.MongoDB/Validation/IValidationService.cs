using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainDrivenDesign.MongoDB.Validation
{
	public interface IValidationService
	{
		Task<ValidationError[]> ValidateAsync(IEnumerable<object> entities);
	}

	public class NullValidationService : IValidationService
	{
		public Task<ValidationError[]> ValidateAsync(IEnumerable<object> entities)
		=>
			Task.FromResult(Array.Empty<ValidationError>());
	}
}