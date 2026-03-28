namespace MarketPriceAPI.Features.GetAssetById;

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
public sealed class GetAssetByIdEndpoint : IEndpointDefinition
{
	public const string Endpoint = "/assets/{asset_id:guid}";

	// ^ ----------------------------------------------------------------------------------------------------<

	public void Map(IEndpointRouteBuilder builder)
	{
		builder.MapGet(Endpoint, GetAssetByIdAsync)
			.Produces<GetAssetByIdRespond>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound)
			.MapToApiVersion(1.0);
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private static async Task<IResult> GetAssetByIdAsync
	(
		[FromServices] IMarketAssetRepository assetRepository,
		[FromRoute(Name="asset_id")] Guid assetId,
		HttpContext httpContext, CancellationToken ct)
	{
		var asset = await assetRepository.GetAssetByIdOrNullAsync(assetId, ct);

		// Check if asset was not found and return HTTP 404 response
		if ( asset is null )
		{
			return Results.NotFound();
		}

		// Return HTTP 200 OK with the response payload
		var respond = new GetAssetByIdRespond(asset);
		return Results.Ok(respond);
	}

	// ------------------------------------------------------------------------------------------------------<
}