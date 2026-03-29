namespace MarketPriceAPI.Data;

// Namespaces used by this file
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System.Linq;

// Main content of the file
public interface IMarketDatabaseContext
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task UpsertAssetsAsync(IEnumerable<MarketAsset> assets, CancellationToken ct);

	Task<IQueryable<MarketAsset>> QueryAssetsAsync();

	// ------------------------------------------------------------------------------------------------------<

	Task<TokenEntry?> GetTokenEntryOrNullAsync(CancellationToken ct);

	Task SetTokenEntryAsync(TokenEntry entry, CancellationToken ct);

	Task<MetadataEntry?> GetMetadataEntryOrNullAsync(CancellationToken ct);

	Task SetMetadataEntryAsync(MetadataEntry entry, CancellationToken ct);

	Task<AssetRefreshMarker?> GetAssetsRefreshMarkerOrNullAsync(CancellationToken ct);

	Task SetAssetsRefreshMarkerAsync(AssetRefreshMarker entry, CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}