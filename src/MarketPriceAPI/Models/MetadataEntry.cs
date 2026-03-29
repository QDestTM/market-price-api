namespace MarketPriceAPI.Models;

// Namespaces used by this file
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using MongoDB.Bson;
using System;

// Main content of the file
public sealed class MetadataEntry
{
	public const string DefaultId = "metadata_entry";

	// ^ ----------------------------------------------------------------------------------------------------<

	[BsonElement("_id")]
	public string Id { get; set; } = DefaultId;

	[BsonElement("providers")]
	public HashSet<string> Providers { get; set; } = [];

	[BsonElement("kinds")]
	public HashSet<string> Kinds { get; set; } = [];

	[BsonElement("refresh_at")]
	[BsonDateTimeOptions(Kind=DateTimeKind.Utc)]
	public DateTime RefreshAt { get; set; } = DateTime.MinValue;

	// ------------------------------------------------------------------------------------------------------<
}