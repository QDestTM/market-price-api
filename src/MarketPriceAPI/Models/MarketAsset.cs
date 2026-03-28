namespace MarketPriceAPI.Models;

// Namespaces used by this file
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using System;

// Main content of the file
public sealed class MarketAsset
{
	// ^ ----------------------------------------------------------------------------------------------------<

	[BsonElement("_id")]
	[JsonPropertyName("id")]
	[BsonRepresentation(BsonType.String)]
	public Guid Id { get; set; } = Guid.Empty;

	[BsonElement("symbol")]
	[JsonPropertyName("symbol")]
	public string Symbol { get; set; } = string.Empty;

	[BsonElement("kind")]
	[JsonPropertyName("kind")]
	public string Kind { get; set; } = string.Empty;

	[BsonElement("tick_size")]
	[JsonPropertyName("tickSize")]
	public double TickSize { get; set; } = 0.0d;

	[BsonElement("description")]
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[BsonElement("currency")]
	[JsonPropertyName("currency")]
	public string Currency { get; set; } = string.Empty;

	[BsonElement("base_currency")]
	[JsonPropertyName("baseCurrency")]
	public string BaseCurrency { get; set; } = string.Empty;

	[BsonElement("contract_size")]
	[JsonPropertyName("contractSize")]
	public int ContractSize { get; set; } = 0;

	// ------------------------------------------------------------------------------------------------------<
}