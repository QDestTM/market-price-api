namespace MarketPriceAPI.Models;

// Namespaces used by this file
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

// Main content of the file
public sealed class AssetRefreshMarker
{
	public const string DefaultId = "asset_refresh_marker";

	// ^ ----------------------------------------------------------------------------------------------------<

	[BsonElement("_id")]
	public string Id { get; set; } = DefaultId;

	[BsonElement("refresh_at")]
	[BsonDateTimeOptions(Kind=DateTimeKind.Utc)]
	public DateTime RefreshAt { get; set; } = DateTime.UtcNow;

	// ------------------------------------------------------------------------------------------------------<
}