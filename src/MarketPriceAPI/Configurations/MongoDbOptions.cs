namespace MarketPriceAPI.Configurations;

// Namespaces used by this file
using System.ComponentModel.DataAnnotations;

// Main content of the file
public sealed class MongoDbOptions
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[Required, MinLength(8)]
	public string Host { get; set; } = string.Empty;

	[Required, MinLength(4)]
	public string Auth { get; set; } = string.Empty;

	[Required]
	public uint Port { get; set; } = 0;

	// ------------------------------------------------------------------------------------------------------<
}