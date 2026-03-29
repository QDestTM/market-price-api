namespace MarketPriceAPI.Features.GetPrice;

// Namespaces used by this file
using System.Text.Json.Serialization;
using MarketPriceAPI.Models;

// Main content of the file
public sealed record GetPriceResponse
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: JsonPropertyName("data")]
	AssetPrice Data

	// ------------------------------------------------------------------------------------------------------<
);