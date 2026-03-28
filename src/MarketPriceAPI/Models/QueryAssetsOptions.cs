namespace MarketPriceAPI.Models;

// Main content of the file
public sealed class QueryAssetsOptions
{
	// ^ ----------------------------------------------------------------------------------------------------<

	public string? Kind { get; set; } = null;
	public string? Symbol { get; set; } = null;

	public int Size { get; set; } = 16;
	public int Page { get; set; } = 1;

	// ------------------------------------------------------------------------------------------------------<
};