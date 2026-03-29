namespace MarketPriceAPI.Features.GetPriceHistory;

using System.Collections.Generic;

// Namespaces used by this file
using System.Text.Json.Serialization;
using MarketPriceAPI.Models;

// Main content of the file
public sealed record GetPriceHistoryResponse
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: JsonPropertyName("data")]
	List<PriceHistoryRecord> Data

	// ------------------------------------------------------------------------------------------------------<
);