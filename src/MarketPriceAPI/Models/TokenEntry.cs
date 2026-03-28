namespace MarketPriceAPI.Models;

// Namespaces used by this file
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

// Main content of the file
public sealed class TokenEntry
{
	public const string DefaultId = "token_entry";

	// ^ ----------------------------------------------------------------------------------------------------<

	[BsonElement("_id")]
	public string Id { get; set; } = DefaultId;

	[BsonElement("access_token")]
	public string AccessToken { get; set; } = string.Empty;

	[BsonElement("refresh_token")]
	public string RefreshToken { get; set; } = string.Empty;

	[BsonElement("access_expires_at")]
	[BsonDateTimeOptions(Kind=DateTimeKind.Utc)]
	public DateTime AccessExpiresAt { get; set; } = DateTime.UtcNow;

	[BsonElement("refresh_expires_at")]
	[BsonDateTimeOptions(Kind=DateTimeKind.Utc)]
	public DateTime RefreshExpiresAt { get; set; } = DateTime.UtcNow;

	// ------------------------------------------------------------------------------------------------------<
}