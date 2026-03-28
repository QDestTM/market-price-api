namespace MarketPriceAPI.Services;

// Namespaces used by this file
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System;

// Main content of the file
public sealed class MetadataRefreshService : BackgroundService, IMetadataService
{
	public const string UriBase = "https://platform.fintacharts.com/api/instruments/v1";
	public readonly TimeSpan RefreshInterval = TimeSpan.FromDays(1.5);

	// ^ ----------------------------------------------------------------------------------------------------<

	// Public instance readonly properties
	public IReadOnlySet<string> Providers => providers;
	public IReadOnlySet<string> Kinds => kinds;

	//! Private instance members
	private readonly ILogger<MetadataRefreshService> logger;
	private readonly ITokenService tokenService;
	private readonly HttpClient httpClient;

	private HashSet<string> providers = [];
	private HashSet<string> kinds = [];

	// Public instance constructors
	public MetadataRefreshService(
		ILogger<MetadataRefreshService> logger,
		ITokenService tokenService,
		HttpClient httpClient) : base()
	{
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
				logger.LogInformation("Starting refresh of asset providers...");
				var providersEnum = await GetProvidersAsync(stoppingToken);
				providers = providersEnum.ToHashSet()!;

				logger.LogInformation("Starting refresh of asset kinds...");
				var kindsEnum = await GetKindsAsync(stoppingToken);
				kinds = kindsEnum.ToHashSet()!;

				logger.LogInformation("Asset providers and kinds succesfully refreshed.");
			}
			catch ( Exception exception )
			{
				logger.LogError(exception, "Error occurred while refreshing assets metadata.");
			}

			await Task.Delay(RefreshInterval, stoppingToken);
		}
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private async Task<IEnumerable<string>> GetProvidersAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri(UriBase + "/providers", ct);
	}


	private async Task<IEnumerable<string>> GetKindsAsync(CancellationToken ct)
	{
		return await ExtractDataFromUri(UriBase + "/kinds", ct);
	}


	private async Task<IEnumerable<string>> ExtractDataFromUri(string requestUri, CancellationToken ct)
	{
		var token = await tokenService.GetAccessTokenAsync(ct);
		var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		// Set Authorization header with bearer token for request
		var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
		request.Headers.Authorization = authenticationHeader;

		// Send request via http client and wait for the response
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
			.Where(x => x is not null)!;
	}

	// ------------------------------------------------------------------------------------------------------<
}