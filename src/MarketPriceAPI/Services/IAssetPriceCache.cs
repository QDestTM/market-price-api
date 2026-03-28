namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Collections.Generic;
using MarketPriceAPI.Models;
using System;

// Main content of the file
public interface IAssetPriceCache
{
	// ^ ----------------------------------------------------------------------------------------------------<

	AssetPrice? GetOrNull(Guid id, string provider);

	AssetPrice? UpsertWithEviction(AssetPrice price);

	void ExpireCached(Guid id, string provider);

	IEnumerable<AssetPrice> GetAllCached();

	// ------------------------------------------------------------------------------------------------------<
}