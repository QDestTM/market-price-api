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
using System.Net.Http;
using System.Net;
using System;

// Main content of the file
public sealed class AssetRefreshService : BackgroundService
{
	public const string UriBase = "https://platform.fintacharts.com/api/instruments/v1/instruments";

	public readonly TimeSpan RefreshTriesInterval = TimeSpan.FromMinutes(3);
	public readonly TimeSpan RefreshItemsInterval = TimeSpan.FromDays(1.0);

	public const int AssetsBufferSize = 256;
	public const int AssetsPageSize = 128;
	public const int RefreshRetries = 5;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IMarketDatabaseContext marketDatabase;
	private readonly ILogger<AssetRefreshService> logger;
	private readonly IMetadataService metadataService;
	private readonly ITokenService tokenService;
	private readonly HttpClient httpClient;

	private AssetRefreshMarker? refreshMarker = null;

	// Public instance constructors
	public AssetRefreshService(
		IMarketDatabaseContext marketDatabase,
		IMetadataService metadataService,
		ILogger<AssetRefreshService> logger,
		ITokenService tokenService,
		HttpClient httpClient) : base()
	{
		this.metadataService = metadataService;
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
				logger.LogError(exception, "Error occurred while refreshing assets.");
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
			// Iterate through all providers and kinds to refresh assets
			foreach ( string provider in metadataService.Providers )
			{
				foreach ( string kind in metadataService.Kinds )
				{
					await RefreshAssets(provider, kind, ct);
					await Task.Delay(500, ct); // Soft throttling
				}

				await Task.Delay(500, ct); // Soft throttling
			}

			// Schedule next refresh by updating the marker in the database
			var refreshAt = DateTime.UtcNow + RefreshItemsInterval;

			refreshMarker = new AssetRefreshMarker() { RefreshAt = refreshAt};
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
			var token = await tokenService.GetAccessTokenAsync(ct);

			var request = new HttpRequestMessage(HttpMethod.Get, UriBase
				+ $"?provider={provider}&kind={kind}&page={page}&size={AssetsPageSize}");

			// Set Authorization header with bearer token for request
			var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
			request.Headers.Authorization = authenticationHeader;

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
				.ReadFromJsonAsync<InstrumentsResponse>(ct);

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
}