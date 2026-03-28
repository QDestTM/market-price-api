namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Threading.Tasks;
using System.Threading;
using System;

// Main content of the file
public interface IAssetPriceStreamService
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task SubscribeToStreamAsync(Guid id, string provider, CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}