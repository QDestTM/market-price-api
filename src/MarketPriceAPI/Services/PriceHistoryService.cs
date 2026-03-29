namespace MarketPriceAPI.Services;

// Namespaces used by this file
using MarketPriceAPI.Repositories;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System.Text.Json;
using System.Net.Http;
using System.Linq;
using System;

// Main content of the file
public sealed class PriceHistoryService : IPriceHistoryService
{
	public const string UriBase = "https://platform.fintacharts.com/api/bars/v1/bars/date-range";
	public const int RespondMaxRecords = 128;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IMarketAssetRepository assetRepository;
	private readonly IMetadataService metadataService;
	private readonly ITokenService tokenService;
	private readonly HttpClient httpClient;

	// Public instance constructors
	public PriceHistoryService(
		IMarketAssetRepository assetRepository,
		IMetadataService metadataService,
		ITokenService tokenService,
		HttpClient httpClient)
	{
		this.metadataService = metadataService;
		this.assetRepository = assetRepository;
		this.tokenService = tokenService;
		this.httpClient = httpClient;
	}


	// # ----------------------------------------------------------------------------------------------------<

	public async Task<PriceHistoryResponse> Fetch(PriceHistoryRequest request, CancellationToken ct)
	{
		var respond = new PriceHistoryResponse()
		{
			Error = await ValidateRequestAsync(request, ct)
		};

		// Return early if request validation failed
		if ( !string.IsNullOrEmpty(respond.Error) )
		{
			return respond;
		}

		// Retrieve access token for API authentication and prerape GET request
		var token = await tokenService.GetAccessTokenAsync(ct);

		using var requestMessage = new HttpRequestMessage(HttpMethod.Get, UriBase +
			$"?instrumentId={request.InstrumentId}" +
			$"&provider={request.Provider}" +
			$"&interval={request.Interval}" +
			$"&periodicity={request.Periodicity}" +
			$"&startDate={request.StartDate:yyyy-MM-dd}" +
			$"&endDate={request.EndDate:yyyy-MM-dd}");

		// Set Authorization header with bearer token
		var authenticationHeader = new AuthenticationHeaderValue("Bearer", token);
		requestMessage.Headers.Authorization = authenticationHeader;

		// Send HTTP request and get response from API
		using var respondMessage = await httpClient.SendAsync(requestMessage, ct);
		respondMessage.EnsureSuccessStatusCode();

		// Read response content as stream and parse JSON
		using var respondStream = await respondMessage.Content.ReadAsStreamAsync(ct);
		using var json = await JsonDocument.ParseAsync(respondStream, default, ct);

		// Extract price history records from JSON data
		var records = json.RootElement
			.GetProperty("data")
			.EnumerateArray()
			.Select((r) =>
			{
				return new PriceHistoryRecord()
				{
					Time   = r.GetProperty("t").GetDateTime(),
					Open   = r.GetProperty("o").GetDecimal(),
					High   = r.GetProperty("h").GetDecimal(),
					Low    = r.GetProperty("l").GetDecimal(),
					Close  = r.GetProperty("c").GetDecimal(),
					Volume = r.GetProperty("v").GetInt64()
				};
			})
			.ToList();

		// Assign extracted records to response
		respond.Data = records;
		return respond;
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task<string> ValidateRequestAsync(PriceHistoryRequest request, CancellationToken ct)
	{
		// Check that start date is before end date
		if ( request.EndDate < request.StartDate )
		{
			return "Invalid start and end date range.";
		}

		// Verify that the requested provider exists
		if ( !metadataService.Providers.Contains(request.Provider) )
		{
			return $"Provider '{request.Provider}' does not exist.";
		}

		// Validate that interval is greater than zero
		if ( request.Interval < 1 )
		{
			return $"Interval must be > 0";
		}

		// Calculate total time span between start and end dates
		TimeSpan timeSpan = request.EndDate - request.StartDate;
		double? totalUnits = request.Periodicity.ToLower() switch
		{
			"minute" => timeSpan.TotalMinutes,
			"hour"   => timeSpan.TotalHours,
			"day"    => timeSpan.TotalDays,
			_        => null
		};

		// Ensure periodicity is supported
		if ( totalUnits is null )
		{
			return $"Periodicity '{request.Periodicity}' is not supported.";
		}

		// Estimate number of records to be returned and enforce maximum record limit
		var estimatedRecords = (int) Math.Ceiling(totalUnits.Value / request.Interval);

		if ( estimatedRecords > RespondMaxRecords )
		{
			return "Too many records requested.";
		}

		// Check if the requested instrument exists in repository
		if ( !await assetRepository.IsAssetExistAsync(request.InstrumentId, ct) )
		{
			return $"Instrument not found: {request.InstrumentId}";
		}

		// Return empty string if validation passed
		return string.Empty;
	}

	// ------------------------------------------------------------------------------------------------------<
}