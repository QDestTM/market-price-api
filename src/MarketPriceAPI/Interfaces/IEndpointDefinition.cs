namespace MarketPriceAPI.Interfaces;

// Namespaces used by this file
using Microsoft.AspNetCore.Routing;

// Main content of the file
public interface IEndpointDefinition
{
	// ^ ----------------------------------------------------------------------------------------------------<

	void Map(IEndpointRouteBuilder builder);

	// ------------------------------------------------------------------------------------------------------<
}