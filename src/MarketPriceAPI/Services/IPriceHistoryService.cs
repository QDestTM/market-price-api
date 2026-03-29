namespace MarketPriceAPI.Services;

// Namespaces used by this file
using MarketPriceAPI.Models;
using System.Threading.Tasks;
using System.Threading;

// Main content of the file
public interface IPriceHistoryService
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task<PriceHistoryResponse> Fetch(PriceHistoryRequest request, CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}