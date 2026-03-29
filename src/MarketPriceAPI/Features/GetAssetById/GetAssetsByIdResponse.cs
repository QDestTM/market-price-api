namespace MarketPriceAPI.Features.GetAssetById;

// Namespaces used by this file
using System.Text.Json.Serialization;
using MarketPriceAPI.Models;

// Main content of the file
public sealed record GetAssetByIdResponse
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: JsonPropertyName("data")]
	MarketAsset Data

	// ------------------------------------------------------------------------------------------------------<
);