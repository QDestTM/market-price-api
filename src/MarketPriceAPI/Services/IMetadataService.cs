namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Collections.Generic;
using System.Threading.Tasks;

// Main content of the file
public interface IMetadataService
{
	// ^ ----------------------------------------------------------------------------------------------------<

	IReadOnlySet<string> Providers { get; }
	IReadOnlySet<string> Kinds { get; }

	public Task InitializationTask { get; }

	// ------------------------------------------------------------------------------------------------------<
}