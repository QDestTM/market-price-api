namespace MarketPriceAPI.Services;

// Namespaces used by this file
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using MarketPriceAPI.Data;
using System.Threading;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System;

// Main content of the file
public sealed class MetadataRefreshService : BackgroundService, IMetadataService
{
	public const string UriBase = "https://platform.fintacharts.com/api/instruments/v1";

	public readonly TimeSpan RefreshTriesInterval = TimeSpan.FromMinutes(3);
	public readonly TimeSpan MetadataExpiresAt = TimeSpan.FromDays(1.5);

	// ^ ----------------------------------------------------------------------------------------------------<

	// Public instance readonly properties
	public IReadOnlySet<string> Providers => metadataEntry!.Providers;
	public IReadOnlySet<string> Kinds => metadataEntry!.Kinds;

	//! Private instance members
	private readonly ILogger<MetadataRefreshService> logger;
	private readonly IMarketDatabaseContext marketDatabase;
	private readonly ITokenService tokenService;
	private readonly HttpClient httpClient;

	private MetadataEntry? metadataEntry = null;

	// Public instance constructors
	public MetadataRefreshService(
		ILogger<MetadataRefreshService> logger,
		IMarketDatabaseContext marketDatabase,
		ITokenService tokenService,
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
				logger.LogInformation("Refreshing assets metadata...");
				await RefreshMetadata(stoppingToken);
			}
			catch ( Exception exception )
			{
				logger.LogError(exception, "Error occurred while refreshing assets metadata.");
			}

			await Task.Delay(RefreshTriesInterval, stoppingToken);
		}
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private async Task RefreshMetadata(CancellationToken ct)
	{
		metadataEntry ??= await marketDatabase.GetMetadataEntryOrNullAsync(ct);

		// Determine whether metadata needs to be refreshed (missing or expired)
		if ( metadataEntry is null || metadataEntry.RefreshAt < DateTime.UtcNow )
		{
			var providers = await GetProvidersAsync(ct);
			var kinds     = await GetKindsAsync(ct);

			// Create and persist a new metadata entry with updated values and expiration
			metadataEntry = new MetadataEntry()
			{
				Providers = providers,
				Kinds = kinds,

				RefreshAt = DateTime.UtcNow + MetadataExpiresAt
			};

			await marketDatabase.SetMetadataEntryAsync(metadataEntry, ct);
			logger.LogInformation("Assets metadata succesfully refreshed.");
		}
		else
		{
			logger.LogInformation("Assets metadata is up to date. No refresh needed.");
		}
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task<HashSet<string>> GetProvidersAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri(UriBase + "/providers", ct);
	}


	private async Task<HashSet<string>> GetKindsAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri(UriBase + "/kinds", ct);
	}


	private async Task<HashSet<string>> ExtractDataFromUri(string requestUri, CancellationToken ct)
	{
		var token = await tokenService.GetAccessTokenAsync(ct);
		using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		// Set Authorization header with bearer token for request
		var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
		request.Headers.Authorization = authenticationHeader;

		// Send request via http client and wait for the response
		using var respond = await httpClient.SendAsync(request, ct);
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
			.ToHashSet()!;
	}

	// ------------------------------------------------------------------------------------------------------<
}