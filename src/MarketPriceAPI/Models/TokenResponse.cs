namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System.Text.Json.Serialization;

// Main content of the file
public sealed class TokenResponse
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = string.Empty;

	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = string.Empty;

	[JsonPropertyName("expires_in")]
	public int AccessExpiresIn { get; set; } = 0;

	[JsonPropertyName("refresh_expires_in")]
	public int RefreshExpiresIn { get; set; } = 0;

	// ------------------------------------------------------------------------------------------------------<
}