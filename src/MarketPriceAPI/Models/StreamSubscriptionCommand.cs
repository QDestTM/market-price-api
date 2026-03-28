namespace MarketPriceAPI.Models;

// Namespaces used by this file
using System;

// Main content of the file
public sealed class StreamSubscriptionCommand
{
	// ^ ----------------------------------------------------------------------------------------------------<

	// Public instance readonly properties
	public string Provider => provider;
	public Guid Id => id;

	public bool Subscribe => subscribe;

	//! Private instance members
	private readonly string provider;
	private readonly Guid id;

	private readonly bool subscribe;

	// Public instance constructors
	public StreamSubscriptionCommand(Guid id, string provider, bool subscribe)
	{
		this.provider = provider;
		this.id = id;

		this.subscribe = subscribe;
	}


	public StreamSubscriptionCommand(AssetPrice assetPrice, bool subscribe)
		: this(assetPrice.InstrumentId, assetPrice.Provider, subscribe)
	{
	}

	// ------------------------------------------------------------------------------------------------------<
}