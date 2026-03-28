namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;
using System;

// Main content of the file
public abstract class PriceTickBase
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("price")]
	public decimal Price { get; set; } = 0.0m;

	[JsonPropertyName("volume")]
	public long Volume { get; set; } = 0L;

	[JsonPropertyName("timestamp")]
	public DateTime Timestamp { get; set; } = DateTime.Now;

	// ------------------------------------------------------------------------------------------------------<
}