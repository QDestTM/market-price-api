namespace MarketPriceAPI.Features.GetPriceHistory;

// Namespaces used by this file
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using MarketPriceAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MarketPriceAPI.Services;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System;

// Main content of the file
public sealed class GetPriceHistoryEndpoint : IEndpointDefinition
{
	public const string Endpoint = "/prices/history";

	// ^ ----------------------------------------------------------------------------------------------------<

	public void Map(IEndpointRouteBuilder builder)
	{
		builder.MapGet(Endpoint, GetPriceHistoryAsync)
			.Produces<GetPriceHistoryResponse>(StatusCodes.Status200OK)
			.Produces<EndpointErrorResponse>(StatusCodes.Status400BadRequest)
			.MapToApiVersion(1.0);
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private static async Task<IResult> GetPriceHistoryAsync
	(
		[FromServices] IPriceHistoryService historyService,
		[AsParameters] GetPriceHistoryQuery query,
		HttpContext httpContext, CancellationToken ct)
	{
		var request = RequestFromQuery(query);

		// Call service to fetch price history data
		var serviceResponse = await historyService.Fetch(request, ct);

		if ( !string.IsNullOrEmpty(serviceResponse.Error) )
		{
			var errorResponse = new EndpointErrorResponse(serviceResponse.Error);
			return Results.BadRequest(errorResponse);
		}

		// Map service data to API response and return 200 OK
		var resultResponse = new GetPriceHistoryResponse(serviceResponse.Data);
		return Results.Ok(resultResponse);
	}

	// ------------------------------------------------------------------------------------------------------<

	private static PriceHistoryRequest RequestFromQuery(GetPriceHistoryQuery query)
	{
		var endDate = query.EndDate ?? DateTime.UtcNow;
		var startDate = query.StartDate ?? (endDate - TimeSpan.FromDays(1.0));

		return new PriceHistoryRequest()
		{
			InstrumentId = query.InstrumentId,
			Provider = query.Provider,

			Periodicity = query.Periodicity,
			Interval = query.Interval,

			StartDate = startDate,
			EndDate = endDate
		};
	}

	// ------------------------------------------------------------------------------------------------------<
}