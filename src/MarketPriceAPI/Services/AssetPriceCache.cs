namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Collections.Generic;
using MarketPriceAPI.Models;
using System.Threading;
using System.Linq;
using System;

// Main content of the file
public sealed class AssetPriceCache : IAssetPriceCache
{
	public const int CacheMaxSize = 16;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly Dictionary<string, AssetPrice> cache = [];
	private readonly Lock sync = new();

	// # ----------------------------------------------------------------------------------------------------<

	public AssetPrice? GetOrNull(Guid id, string provider)
	{
		lock (sync)
		{
			return cache.GetValueOrDefault( ToKey(id, provider) );
		}
	}


	public AssetPrice? UpsertWithEviction(AssetPrice price)
	{
		lock (sync)
		{
			var key = ToKey(price.InstrumentId, price.Provider);
			DateTime utcNow = DateTime.UtcNow;

			// Check if the price already exists in the cache
			if ( !cache.TryGetValue(key, out var cachedPrice) )
			{
				price.LastUpdated = utcNow;
				cache[key] = price;
			}
			else // Update existing cached price fields if new values are provided
			{
				cachedPrice.Ask = price.Ask ?? cachedPrice.Ask;
				cachedPrice.Bid = price.Bid ?? cachedPrice.Bid;
				cachedPrice.Last = price.Last ?? cachedPrice.Last;

				cachedPrice.LastUpdated = utcNow;
			}

			// Evict the oldest entry if the cache exceeds its maximum size
			AssetPrice? removePrice = null;

			if ( cache.Count > CacheMaxSize )
			{
				removePrice = cache.Values.MinBy(x => x.LastUpdated);

				if ( removePrice is not null )
				{
					var removeKey = ToKey(removePrice);
					cache.Remove(removeKey);
				}
			}

			return removePrice;
		}
	}

	// ------------------------------------------------------------------------------------------------------<

	private static string ToKey(AssetPrice price)
	{
		return ToKey(price.InstrumentId, price.Provider);
	}


	private static string ToKey(Guid id, string provider)
	{
		return $"{id}:{provider}";
	}

	// ------------------------------------------------------------------------------------------------------<
}