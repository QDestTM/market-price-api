namespace MarketPriceAPI.Configurations;

// Namespaces used by this file
using System.ComponentModel.DataAnnotations;

// Main content of the file
public sealed class AuthOptions
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[Required, MinLength(16)]
	public string FintachartsUsername { get; set; } = string.Empty;

	[Required, MinLength(16)]
	public string FintachartsPassword { get; set; } = string.Empty;

	[Required, MinLength(8)]
	public string MongoDbUsername { get; set; } = string.Empty;

	[Required, MinLength(8)]
	public string MongoDbPassword { get; set; } = string.Empty;

	// ------------------------------------------------------------------------------------------------------<
}