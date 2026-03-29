namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;

// Main content of the file
public sealed record EndpointErrorResponse
(
	// ^ ----------------------------------------------------------------------------------------------------<

	[property: JsonPropertyName("error")]
	string Error

	// ------------------------------------------------------------------------------------------------------<
);