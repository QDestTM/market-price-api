namespace MarketPriceAPI.Repositories;

// Namespaces used by this file
using MarketPriceAPI.Services;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using MarketPriceAPI.Data;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Linq;
using System;

// Main content of the file
public sealed class MarketAssetRepository : IMarketAssetRepository
{
	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly IMarketDatabaseContext marketDatabase;
	private readonly IMetadataService metadataService;

	// Public instance constructors
	public MarketAssetRepository(
		IMarketDatabaseContext marketDatabase,
		IMetadataService metadataService)
	{
		this.metadataService = metadataService;
		this.marketDatabase = marketDatabase;
	}

	// # ----------------------------------------------------------------------------------------------------<

	public async Task<QueryAssetsResult> QueryAssetsAsync(QueryAssetsOptions options)
	{
		var assetsQuery = await marketDatabase.QueryAssetsAsync();
		var queryResult = new QueryAssetsResult();

		// Apply filters by asset kind/symbol if specified
		if ( !string.IsNullOrEmpty(options.Kind) )
		{
			if ( metadataService.Kinds.Contains(options.Kind) )
			{
				assetsQuery = assetsQuery.Where(a => a.Kind == options.Kind);
			}
			else // Kind not recognized, return empty result set
			{
				assetsQuery = Enumerable.Empty<MarketAsset>().AsQueryable();
			}
		}

		if ( !string.IsNullOrEmpty(options.Symbol) )
		{
			#pragma warning disable CA1862 // StringComparison.OrdinalIgnoreCase is unsuppored operation
			assetsQuery = assetsQuery.Where(a => a.Symbol.Contains(options.Symbol.ToUpper()));
			#pragma warning restore CA1862 // StringComparison.OrdinalIgnoreCase is unsuppored operation
		}

		// Order the query by Id descending to get consistent sequences
		assetsQuery = assetsQuery.OrderByDescending(a => a.Id);

		// Populate pagination information in the query result
		var pagination = queryResult.Paging;

		pagination.Items = await assetsQuery.CountAsync();
		pagination.Pages = (int) MathF.Ceiling((float) pagination.Items / options.Size);
		pagination.Page = options.Page;

		// Apply pagination based on page number and page size
		queryResult.Items = assetsQuery
			.Skip((options.Page - 1) * options.Size)
			.Take(options.Size)
			.AsEnumerable();

		return queryResult;
	}


	public async Task<MarketAsset?> GetAssetByIdOrNullAsync(Guid id, CancellationToken ct)
	{
		var assetsQuery = await marketDatabase.QueryAssetsAsync();

		// Filter the query by asset Id and return the first matching result or null
		return await assetsQuery.Where(x => x.Id == id).FirstOrDefaultAsync(ct);
	}


	public async Task<bool> IsAssetExistAsync(Guid id, CancellationToken ct)
	{
		var assetsQuery = await marketDatabase.QueryAssetsAsync();

		// Check if any asset with the specified ID exists
		return await assetsQuery.AnyAsync(x => x.Id == id, ct);
	}

	// ------------------------------------------------------------------------------------------------------<
}