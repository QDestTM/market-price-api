namespace MarketPriceAPI.Data;

// Namespaces used by this file
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketPriceAPI.Models;
using System.Threading;
using MongoDB.Driver;
using System.Linq;

// Main content of the file
public sealed class MarketDatabaseContext : IMarketDatabaseContext
{
	// ^ ----------------------------------------------------------------------------------------------------<

	// Public instance properties
	public IMongoCollection<MarketAsset> Assets => database.GetCollection<MarketAsset>("assets");

	//! Private instance members
	private readonly IMongoDatabase database;

	// Public instance constructors
	public MarketDatabaseContext(IMongoClient client, string databaseName)
	{
		database = client.GetDatabase(databaseName);
	}

	// # ----------------------------------------------------------------------------------------------------<

	public async Task UpsertAssetsAsync(IEnumerable<MarketAsset> assets, CancellationToken ct)
	{
		var requests = new List<WriteModel<MarketAsset>>();

		// Generate upsert request from each asset
		foreach ( var asset in assets )
		{
			var filter = Builders<MarketAsset>.Filter.Eq(d => d.Id, asset.Id);

			// Create an replace model for each asset
			var replaceModel = new ReplaceOneModel<MarketAsset>(filter, asset)
			{
				IsUpsert = true
			};

			requests.Add(replaceModel);
		}

		// Perform the bulk write operation
		if (requests.Count > 0)
		{
			var options = new BulkWriteOptions { IsOrdered = false };
			await Assets.BulkWriteAsync(requests, options, ct);
		}
	}


	public async Task<IQueryable<MarketAsset>> QueryAssetsAsync()
	{
		return Assets.AsQueryable();
	}

	// ------------------------------------------------------------------------------------------------------<

	public async Task<TokenEntry?> GetTokenEntryOrNullAsync(CancellationToken ct)
	{
		var filter = Builders<TokenEntry>.Filter.Eq(x => x.Id, TokenEntry.DefaultId);
		return await GetSystemDocument(filter, ct);
	}


	public async Task SetTokenEntryAsync(TokenEntry entry, CancellationToken ct)
	{
		var filter = Builders<TokenEntry>.Filter.Eq(x => x.Id, TokenEntry.DefaultId);
		await SetSystemDocument(filter, entry, ct);
	}


	public async Task<AssetsRefreshMarker?> GetAssetsRefreshMarkerOrNullAsync(CancellationToken ct)
	{
		var filter = Builders<AssetsRefreshMarker>.Filter.Eq(x => x.Id, AssetsRefreshMarker.DefaultId);
		return await GetSystemDocument(filter, ct);
	}


	public async Task SetAssetsRefreshMarkerAsync(AssetsRefreshMarker entry, CancellationToken ct)
	{
		var filter = Builders<AssetsRefreshMarker>.Filter.Eq(x => x.Id, AssetsRefreshMarker.DefaultId);
		await SetSystemDocument(filter, entry, ct);
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task SetSystemDocument<T>(FilterDefinition<T> filter, T entry, CancellationToken ct)
	{
		var options = new ReplaceOptions() { IsUpsert = true };
		var system = GetSystemCollection<T>();

		await system.ReplaceOneAsync(filter, entry, options, ct);
	}


	private async Task<T?> GetSystemDocument<T>(FilterDefinition<T> filter, CancellationToken ct)
	{
		var system = GetSystemCollection<T>();

		return await system.Find(filter)
			.FirstOrDefaultAsync(ct);
	}


	private IMongoCollection<T> GetSystemCollection<T>()
	{
		return database.GetCollection<T>("system");
	}

	// ------------------------------------------------------------------------------------------------------<
}