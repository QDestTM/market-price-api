namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Collections.Generic;

// Main content of the file
public sealed class PriceHistoryResponse
{
	// ^ ----------------------------------------------------------------------------------------------------<

	public List<PriceHistoryRecord> Data { get; set; } = [];
	public string Error { get; set; } = string.Empty;

	// ------------------------------------------------------------------------------------------------------<
};