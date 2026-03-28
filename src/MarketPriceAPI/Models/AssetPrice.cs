namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;
using System;

// Main content of the file
public sealed class AssetPrice
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("instrumentId")]
	public Guid InstrumentId { get; set; } = Guid.Empty;

	[JsonPropertyName("provider")]
	public string Provider { get; set; } = string.Empty;

	[JsonPropertyName("ask")]
	public PriceAskTick? Ask { get; set; } = null;

	[JsonPropertyName("bid")]
	public PriceBidTick? Bid { get; set; } = null;

	[JsonPropertyName("last")]
	public PriceLastTick? Last { get; set; } = null;

	[JsonPropertyName("last_updated")]
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

	// ------------------------------------------------------------------------------------------------------<
}