namespace MarketPriceAPI.Features.GetPrice;

// Namespaces used by this file
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MarketPriceAPI.Repositories;
using Microsoft.AspNetCore.Http;
using MarketPriceAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using System;

// Main content of the file
public sealed class GetPriceEndpoint : IEndpointDefinition
{
	public const string Endpoint = "/prices/{provider}/{asset_id:guid}";

	// ^ ----------------------------------------------------------------------------------------------------<

	public void Map(IEndpointRouteBuilder builder)
	{
		builder.MapGet(Endpoint, GetPriceAsync)
			.WithDisplayName("Get Price")
			.WithDescription("Retrieves the real-time price of a specific asset from " +
							"the given provider by its unique identifier (GUID). " +
							"If no price is available (404 Not Found), it may be worth retrying, " +
							"as the data might not have arrived yet.")
			.Produces<GetPriceResponse>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound)
			.MapToApiVersion(1.0);
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private static async Task<IResult> GetPriceAsync
	(
		[FromServices] IAssetPriceRepository priceRepository,
		[FromRoute(Name="provider")] string provider,
		[FromRoute(Name="asset_id")] Guid assetId,
		HttpContext httpContext, CancellationToken ct)
	{
		var assetPrice = await priceRepository
			.GetAssetPriceOrNullAsync(assetId, provider, ct);

		// Check if asset price was not found and return HTTP 404 response
		if ( assetPrice is null )
		{
			return Results.NotFound();
		}

		// Return HTTP 200 OK with the response payload
		var response = new GetPriceResponse(assetPrice);
		return Results.Ok(response);
	}

	// ------------------------------------------------------------------------------------------------------<
}