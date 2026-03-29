namespace MarketPriceAPI.Features.GetAssets;

// Namespaces used by this file
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MarketPriceAPI.Repositories;
using Microsoft.AspNetCore.Http;
using MarketPriceAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using System;

// Main content of the file
public sealed class GetAssetsEndpoint : IEndpointDefinition
{
	public const string Endpoint = "/assets";

	public const int MaxPageSize = 128;
	public const int MinPageSize = 1;

	// ^ ----------------------------------------------------------------------------------------------------<

	public void Map(IEndpointRouteBuilder builder)
	{
		builder.MapGet(Endpoint, GetAssetsAsync)
			.Produces<EndpointErrorResponse>(StatusCodes.Status400BadRequest)
			.Produces<GetAssetsRespond>(StatusCodes.Status200OK)
			.MapToApiVersion(1.0);
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private static async Task<IResult> GetAssetsAsync
	(
		[FromServices] IMarketAssetRepository assetRepository,
		[AsParameters] GetAssetsQuery query,
		HttpContext httpContext, CancellationToken ct)
	{
		var options = QueryOptionsFromEndpointQuery(query);

		// Execute assets query with applied filters and pagination
		var queryResult = await assetRepository.QueryAssetsAsync(options);

		if ( !string.IsNullOrEmpty(queryResult.Error) )
		{
			var errorResponse = new EndpointErrorResponse(queryResult.Error);
			return Results.BadRequest(errorResponse);
		}

		var respond = new GetAssetsRespond([..queryResult.Items], queryResult.Paging);
		return Results.Ok(respond); // Return HTTP 200 OK with the response payload
	}

	// ------------------------------------------------------------------------------------------------------<

	private static QueryAssetsOptions QueryOptionsFromEndpointQuery(GetAssetsQuery query)
	{
		return new QueryAssetsOptions()
		{
			Symbol = string.IsNullOrEmpty(query.Symbol) ? null : query.Symbol,
			Kind = string.IsNullOrEmpty(query.Kind) ? null : query.Kind,

			Page = Math.Max(query.Page, MinPageSize),
			Size = Math.Min(Math.Max(query.Size, MinPageSize), MaxPageSize),
		};
	}

	// ------------------------------------------------------------------------------------------------------<
}