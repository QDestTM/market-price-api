namespace MarketPriceAPI.Features.GetAssets;

// Namespaces used by this file
using System.Text.Json.Serialization;
using System.Collections.Generic;
using MarketPriceAPI.Models;

// Main content of the file
public sealed record GetAssetsRespond
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: JsonPropertyName("items")]
	List<MarketAsset> Items,

	[property: JsonPropertyName("pages")]
	PagingInfo Paging

	// ------------------------------------------------------------------------------------------------------<
);