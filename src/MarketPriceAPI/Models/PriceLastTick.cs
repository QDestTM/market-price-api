namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;

// Main content of the file
public sealed class PriceLastTick : PriceTickBase
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("changePct")]
	public decimal ChangePct { get; set; } = 0.0m;

	[JsonPropertyName("change")]
	public decimal Change { get; set; } = 0.0m;

	// ------------------------------------------------------------------------------------------------------<
}