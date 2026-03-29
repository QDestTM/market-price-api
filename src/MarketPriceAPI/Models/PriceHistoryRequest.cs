namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System;

// Main content of the file
public sealed class PriceHistoryRequest
{
	// ^ ----------------------------------------------------------------------------------------------------<

	public Guid InstrumentId { get; set; } = Guid.Empty;
	public string Provider { get; set; } = string.Empty;

	public string Periodicity { get; set; } = "hour";
	public int Interval { get; set; } = 1;

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	// ------------------------------------------------------------------------------------------------------<
};