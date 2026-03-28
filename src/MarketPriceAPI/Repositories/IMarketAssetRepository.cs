namespace MarketPriceAPI.Repositories;

// Namespaces used by this file
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System;

// Main content of the file
public interface IMarketAssetRepository
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task<QueryAssetsResult> QueryAssetsAsync(QueryAssetsOptions queryOptions);

	Task<MarketAsset?> GetAssetByIdOrNullAsync(Guid id, CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}