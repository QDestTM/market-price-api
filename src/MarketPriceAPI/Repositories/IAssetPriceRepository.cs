namespace MarketPriceAPI.Repositories;

// Namespaces used by this file
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System;

// Main content of the file
public interface IAssetPriceRepository
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task<AssetPrice?> GetAssetPriceOrNullAsync(Guid id, string provider, CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}