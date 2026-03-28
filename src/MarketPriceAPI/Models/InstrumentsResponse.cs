namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;
using System.Collections.Generic;

// Main content of the file
public sealed class InstrumentsResponse
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("paging")]
	public PagingInfo Paging { get; set; } = new();

	[JsonPropertyName("data")]
	public List<MarketAsset> Data { get; set; } = [];

	// ------------------------------------------------------------------------------------------------------<
}