namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Diagnostics.CodeAnalysis;
using MarketPriceAPI.Configurations;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using MarketPriceAPI.Data;
using System.Text.Json;
using System.Threading;
using System.Net.Http;
using System;

// Main content of the file
public sealed class TokenService : ITokenService
{
	private const int TokenExpiryBufferSeconds = 16;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IMarketDatabaseContext marketDatabase;
	private readonly IOptions<AuthOptions> authOptions;
	private readonly HttpClient httpClient;

	private TokenEntry? tokenEntry = null;

	// Public instance constructors
	public TokenService(
		IMarketDatabaseContext marketDatabase,
		IOptions<AuthOptions> authOptions,
		HttpClient httpClient)
	{
		this.marketDatabase = marketDatabase;
		this.authOptions = authOptions;
		this.httpClient = httpClient;
	}

	// # ----------------------------------------------------------------------------------------------------<

	public async Task<string> GetAccessTokenAsync(CancellationToken ct)
	{
		tokenEntry ??= await marketDatabase.GetTokenEntryOrNullAsync(ct);

		// Check if there is no token or refresh token has expired, request a new access token
		if ( tokenEntry is null || tokenEntry.RefreshExpiresAt < DateTime.UtcNow )
		{
			var respond = await RequestAccessTokenAsync(ct);
			await UpdateTokenEntryFromResponseAsync(respond, ct);
		}

		// Check if access token has expired, refresh it using the refresh token
		if ( tokenEntry.AccessExpiresAt < DateTime.UtcNow )
		{
			var respond = await RefreshAccessTokenAsync(ct);
			await UpdateTokenEntryFromResponseAsync(respond, ct);
		}

		return tokenEntry.AccessToken;
	}

	// ------------------------------------------------------------------------------------------------------<

	[MemberNotNull(nameof(tokenEntry))]
	private async Task UpdateTokenEntryFromResponseAsync(
		TokenResponse response, CancellationToken ct)
	{
		var utcNow = DateTime.UtcNow;

		// Calculate the absolute expiration times for access and refresh tokens
		var accessExpiresAt = utcNow.AddSeconds(response.AccessExpiresIn - TokenExpiryBufferSeconds);
		var refreshExpiresAt = utcNow.AddSeconds(response.RefreshExpiresIn - TokenExpiryBufferSeconds);

		tokenEntry = new TokenEntry()
		{
			AccessToken = response.AccessToken,
			RefreshToken = response.RefreshToken,

			AccessExpiresAt = accessExpiresAt,
			RefreshExpiresAt = refreshExpiresAt
		};

		// Save or update token entry in the database
		await marketDatabase.SetTokenEntryAsync(tokenEntry, ct);
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task<TokenResponse> RequestAccessTokenAsync(CancellationToken ct)
	{
		var body = new Dictionary<string, string>
		{
			["grant_type"] = "password",
			["client_id"]  = "app-cli",
			["username"]   = authOptions.Value.FintachartsUsername,
			["password"]   = authOptions.Value.FintachartsPassword
		};

		// Send a POST request to the token endpoint using password grant
		return await PostTokenRequestAsync(body, ct);
	}


	private async Task<TokenResponse> RefreshAccessTokenAsync(CancellationToken ct)
	{
		ArgumentNullException.ThrowIfNull(tokenEntry, nameof(tokenEntry));

		var body = new Dictionary<string, string>
		{
			["grant_type"]    = "refresh_token",
			["client_id"]     = "app-cli",
			["refresh_token"] = tokenEntry.RefreshToken
		};

		// Send a POST request to refresh the access token using the refresh token
		return await PostTokenRequestAsync(body, ct);
	}


	private async Task<TokenResponse> PostTokenRequestAsync(
		Dictionary<string, string> body, CancellationToken ct)
	{
		const string requestUri = "https://platform.fintacharts.com/" +
			"identity/realms/fintatech/protocol/openid-connect/token";

		// Create an HTTP POST request with form URL-encoded body
		var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
		{
			Content = new FormUrlEncodedContent(body)
		};

		// Send the HTTP request and ensure a successful response
		var response = await httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();

		// Read response content and deserialize it into TokenRespond
		var json = await response.Content.ReadAsStringAsync(ct);
		return JsonSerializer.Deserialize<TokenResponse>(json)!;
	}

	// ------------------------------------------------------------------------------------------------------<
}