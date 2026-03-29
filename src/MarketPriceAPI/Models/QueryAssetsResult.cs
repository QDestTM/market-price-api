namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Collections.Generic;

// Main content of the file
public sealed class QueryAssetsResult
{
	// ^ ----------------------------------------------------------------------------------------------------<

	public IEnumerable<MarketAsset> Items { get; set; } = [];
	public PagingInfo Paging { get; set; } = new();

	public string Error { get; set; } = string.Empty;

	// ------------------------------------------------------------------------------------------------------<
};