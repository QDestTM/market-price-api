namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;

// Main content of the file
public sealed class PagesInfo
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("page")]
	public int Page { get; set; } = 0;

	[JsonPropertyName("pages")]
	public int Pages { get; set; } = 0;

	[JsonPropertyName("items")]
	[JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingDefault)]
	public int Items { get; set; } = 0;

	// ------------------------------------------------------------------------------------------------------<
}