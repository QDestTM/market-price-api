namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;
using System;

// Main content of the file
public sealed class PriceHistoryRecord
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("time")]
	public DateTime Time { get; set; }

	[JsonPropertyName("open")]
	public decimal Open { get; set; }

	[JsonPropertyName("high")]
	public decimal High { get; set; }

	[JsonPropertyName("low")]
	public decimal Low { get; set; }

	[JsonPropertyName("close")]
	public decimal Close { get; set; }

	[JsonPropertyName("volume")]
	public long Volume { get; set; }

	// ------------------------------------------------------------------------------------------------------<
}