namespace MarketPriceAPI.Features.GetPriceHistory;

// Namespaces used by this file
using Microsoft.AspNetCore.Mvc;
using System;

// Main content of the file
public sealed record GetPriceHistoryQuery
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: FromQuery(Name="instrument_id")]
	Guid InstrumentId,

	[property: FromQuery(Name="provider")]
	string Provider,

	[property: FromQuery(Name="interval")]
	int Interval = 1,

	[property: FromQuery(Name="periodicity")]
	string Periodicity = "hour",

	[property: FromQuery(Name="start_date")]
	DateTime? StartDate = null,

	[property: FromQuery(Name="end_data")]
	DateTime? EndDate = null

	// ------------------------------------------------------------------------------------------------------<
);