namespace MarketPriceAPI.Services;

// Namespaces used by this file
using MarketPriceAPI.Models;
using System;

// Main content of the file
public interface IAssetPriceCache
{
	// ^ ----------------------------------------------------------------------------------------------------<

	AssetPrice? GetOrNull(Guid id, string provider);

	AssetPrice? UpsertWithEviction(AssetPrice price);

	// ------------------------------------------------------------------------------------------------------<
}