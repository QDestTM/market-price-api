namespace MarketPriceAPI.Services;

// Namespaces used by this file
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Net.Http.Json;
using MarketPriceAPI.Data;
using System.Threading;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System.Net;
using System;

// Main content of the file
public sealed class AssetsRefreshService : BackgroundService
{
	public readonly TimeSpan RefreshTriesInterval = TimeSpan.FromMinutes(3);
	public readonly TimeSpan RefreshItemsInterval = TimeSpan.FromDays(1.0);

	public const int AssetsBufferSize = 256;
	public const int AssetsPageSize = 128;
	public const int RefreshRetries = 5;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IMarketDatabaseContext marketDatabase;
	private readonly IFintachartsTokenService tokenService;
	private readonly ILogger<AssetsRefreshService> logger;
	private readonly HttpClient httpClient;

	private AssetsRefreshMarker? refreshMarker = null;

	// Public instance constructors
	public AssetsRefreshService(
		IMarketDatabaseContext marketDatabase,
		ILogger<AssetsRefreshService> logger,
		IFintachartsTokenService tokenService,
		HttpClient httpClient) : base()
	{
		this.marketDatabase = marketDatabase;
		this.tokenService = tokenService;
		this.httpClient = httpClient;
		this.logger = logger;
	}

	// # ----------------------------------------------------------------------------------------------------<

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while ( !stoppingToken.IsCancellationRequested )
		{
			try
			{
				logger.LogInformation("Refreshing assets...");
				await RefreshAssets(stoppingToken);
			}
			catch ( Exception exception )
			{
				logger.LogError($"Error refreshing assets: {exception.Message}");
			}

			await Task.Delay(RefreshTriesInterval, stoppingToken);
		}
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private async Task RefreshAssets(CancellationToken ct)
	{
		refreshMarker ??= await marketDatabase.GetAssetsRefreshMarkerOrNullAsync(ct);

		// Check if refresh is needed based on stored marker
		if ( refreshMarker is null || refreshMarker.RefreshAt < DateTime.UtcNow )
		{
			var providers = await GetProvidersAsync(ct);
			var kinds = await GetKindsAsync(ct);

			// Iterate through all providers and kinds to refresh assets
			foreach ( string provider in providers )
			{
				foreach ( string kind in kinds )
				{
					await RefreshAssets(provider, kind, ct);
					await Task.Delay(500, ct); // Soft throttling
				}

				await Task.Delay(500, ct); // Soft throttling
			}

			// Schedule next refresh by updating the marker in the database
			var refreshAt = DateTime.UtcNow + RefreshItemsInterval;

			refreshMarker = new AssetsRefreshMarker() { RefreshAt = refreshAt};
			await marketDatabase.SetAssetsRefreshMarkerAsync(refreshMarker, ct);

			logger.LogInformation("Assets succesfully refreshed.");
		}
		else
		{
			logger.LogInformation("Assets is up to date. No refresh needed.");
		}
	}


	private async Task RefreshAssets(string provider, string kind, CancellationToken ct)
	{
		List<MarketAsset> buffer = new(AssetsBufferSize);
		int retries = 0; int pages = 1; int page = 1;

		do
		{
			logger.LogInformation($"Refreshing {provider}-{kind} page {page}/{pages}...");

			// Create HTTP GET request with authorization token
			var request = await CreateGetRequest(
				$"https://platform.fintacharts.com/api/instruments/v1/instruments" +
				$"?provider={provider}&kind={kind}&page={page}&size={AssetsPageSize}", ct);

			// Send the request to the API and get the response
			var response = await httpClient.SendAsync(request, ct);

			if ( response.StatusCode == HttpStatusCode.Unauthorized )
			{
				logger.LogWarning("Unauthorized – refreshing token and retrying.");
				continue; // Recreating request will trigger token refreshing
			} else
			if ( response.StatusCode == HttpStatusCode.TooManyRequests )
			{
				if ( ++retries > RefreshRetries )
					break;

				int delay = 2000 * (int) Math.Pow(2, retries - 1); // Expo-delay
				logger.LogWarning($"TooManyRequests – retry {retries} in {delay}ms");

				await Task.Delay(delay, ct); continue;
			}

			response.EnsureSuccessStatusCode();

			// Deserialize JSON response into instruments object
			var instrumentsResponse = await response.Content
				.ReadFromJsonAsync<FintachartsInstrumentsResponse>(ct);

			// Add received instruments to buffer and update total pages
			if ( instrumentsResponse is not null )
			{
				buffer.AddRange(instrumentsResponse.Data);
				pages = instrumentsResponse.Paging.Pages;
			}

			if ( buffer.Count >= AssetsBufferSize )
			{
				await marketDatabase.UpsertAssetsAsync(buffer, ct);
				buffer.Clear();
			}

			// Apply soft throttling to avoid overwhelming the API
			await Task.Delay(200, ct); // Soft throttling

			retries = 0; // Reset retries counter
			page++; // Moving to next page

		} while ( page <= pages && !ct.IsCancellationRequested );

		// Insert remaining buffered assets into database if any
		if ( buffer.Count != 0 )
		{
			await marketDatabase.UpsertAssetsAsync(buffer, ct);
		}
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task<HttpRequestMessage> CreateGetRequest(string requestUri, CancellationToken ct)
	{
		var token = await tokenService.GetAccessTokenAsync(ct);
		var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		// Set Authorization header with bearer token for request
		var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
		request.Headers.Authorization = authenticationHeader;

		return request;
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task<string[]> GetProvidersAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri("https://platform.fintacharts.com/api/instruments/v1/providers", ct);
	}


	private async Task<string[]> GetKindsAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri("https://platform.fintacharts.com/api/instruments/v1/kinds", ct);
	}


	private async Task<string[]> ExtractDataFromUri(string requestUri, CancellationToken ct)
	{
		var request = await CreateGetRequest(requestUri, ct);
		var respond = await httpClient.SendAsync(request, ct);

		respond.EnsureSuccessStatusCode();

		// Read response stream and parse JSON document
		using var stream = await respond.Content.ReadAsStreamAsync(ct);
		using var json = await JsonDocument.ParseAsync(stream, default, ct);

		// Extract "data" array from JSON and return as string array
		return json.RootElement
			.GetProperty("data")
			.EnumerateArray()
			.Select(x => x.GetString())
			.Where(x => x is not null)
			.ToArray()!;
	}

	// ------------------------------------------------------------------------------------------------------<
}