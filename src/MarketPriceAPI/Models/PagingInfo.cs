namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;

// Main content of the file
public sealed class PagingInfo
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("page")]
	public int Page { get; set; } = 1;

	[JsonPropertyName("pages")]
	public int Pages { get; set; } = 0;

	[JsonPropertyName("items")]
	public int Items { get; set; } = 0;

	// ------------------------------------------------------------------------------------------------------<
}