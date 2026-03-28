namespace MarketPriceAPI.Repositories;

// Namespaces used by this file
using MarketPriceAPI.Services;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System;

// Main content of the file
public sealed class AssetPriceRepository : IAssetPriceRepository
{
	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IAssetPriceStreamService assetPriceStream;
	private readonly IMetadataService metadataService;
	private readonly IAssetPriceCache priceCache;

	// Public instance constructors
	public AssetPriceRepository(
		IAssetPriceStreamService assetPriceStream,
		IMetadataService metadataService,
		IAssetPriceCache priceCache)
	{
		this.assetPriceStream = assetPriceStream;
		this.metadataService = metadataService;
		this.priceCache = priceCache;
	}

	// # ----------------------------------------------------------------------------------------------------<

	public async Task<AssetPrice?> GetAssetPriceOrNullAsync(
		Guid id, string provider, CancellationToken ct)
	{
		if ( !metadataService.Providers.Contains(provider) )
		{
			return null;
		}

		// Try to get the price from cache first
		var cachedPrice = priceCache.GetOrNull(id, provider);

		// Subscribe to stream if price is not cached yet
		if ( cachedPrice is null )
		{
			await assetPriceStream.SubscribeToStreamAsync(id, provider, ct);
			await Task.Delay(300, cancellationToken: ct);
		}

		// Return the latest cached price (may still be null if not received yet)
		return priceCache.GetOrNull(id, provider);
	}

	// ------------------------------------------------------------------------------------------------------<
}