namespace MarketPriceAPI.Features.GetAssets;

// Namespaces used by this file
using Microsoft.AspNetCore.Mvc;

// Main content of the file
public sealed record GetAssetsQuery
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: FromQuery(Name="kind")]
	string Kind = "",

	[property: FromQuery(Name="symbol")]
	string Symbol = "",

	[property: FromQuery(Name="size")]
	int Size = 16,

	[property: FromQuery(Name="page")]
	int Page = 1

	// ------------------------------------------------------------------------------------------------------<
);