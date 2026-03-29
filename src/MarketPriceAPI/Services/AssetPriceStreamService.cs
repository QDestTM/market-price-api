namespace MarketPriceAPI.Services;

// Namespaces used by this file
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.WebSockets;
using MarketPriceAPI.Models;
using System.Threading;
using System.Text.Json;
using System.Text;
using System;

// Main content of the file
public sealed class AssetPriceStreamService : BackgroundService, IAssetPriceStreamService
{
	public static readonly byte[] PingMessage = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");

	public static readonly TimeSpan KeepAliveInterval = TimeSpan.FromMinutes(1.9);
	public static readonly TimeSpan ReconnectInterval = TimeSpan.FromSeconds(5.0);

	public const int MessageBufferSize = 8192;

	// ^ ----------------------------------------------------------------------------------------------------<

	//! Private instance members
	private readonly ILogger<AssetPriceStreamService> logger;
	private readonly IAssetPriceCache priceCache;
	private readonly ITokenService tokenService;

	private readonly SemaphoreSlim sendLock = new(1, 1);

	private CancellationTokenSource subTasksToken = null!; // ExecuteAsync
	private ClientWebSocket socket = null!; // ExecuteAsync
	private Task keepAliveTask = null!; // ExecuteAsync

	// Public instance constructors
	public AssetPriceStreamService(
		ITokenService tokenService,
		ILogger<AssetPriceStreamService> logger,
		IAssetPriceCache priceCache) : base()
	{
		this.tokenService = tokenService;
		this.priceCache = priceCache;
		this.logger = logger;
	}

	// # ----------------------------------------------------------------------------------------------------<

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while ( !stoppingToken.IsCancellationRequested )
		{
			socket = new ClientWebSocket();

			// Establish WebSocket connection using fresh access token
			var uri = await GetSocketConnectionUri(stoppingToken);
			await socket.ConnectAsync(uri, stoppingToken);

			logger.LogInformation("WebSocket is successfully connected.");

			// Restore all active subscriptions after reconnect
			await RestoreSubscriptionsAsync(stoppingToken);

			// Start background tasks for keep-alive pings
			subTasksToken = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
			keepAliveTask = KeepAliveLoopAsync(subTasksToken.Token);

			// Start listening for incoming WebSocket messages
			try
			{
				await ListenerLoopAsync(stoppingToken);
			}
			catch ( Exception exception )
			{
				logger.LogError(exception, "An error occured in the listening loop.");
			}

			subTasksToken.Cancel();

			// Wait before attempting to reconnect after disconnection
			if ( !stoppingToken.IsCancellationRequested )
			{
				logger.LogWarning($"WebSocket disconnected. Reconnecting in 5s...");
				await Task.Delay(ReconnectInterval, stoppingToken);
			}

			// Await completion of background tasks before cleanup
			await keepAliveTask;

			subTasksToken.Dispose();
			socket.Dispose();
		}
	}

	// @ ----------------------------------------------------------------------------------------------------<

	public async Task SubscribeToStreamAsync(Guid id, string provider, CancellationToken ct)
	{
		var command = new StreamSubscriptionCommand(id, provider, true);
		await SendSubscriptionCommand(command, ct);
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task ListenerLoopAsync(CancellationToken ct)
	{
		var buffer = new byte[MessageBufferSize];

		// Continuously receive and process incoming WebSocket messages
		while ( !ct.IsCancellationRequested && socket.State == WebSocketState.Open )
		{
			var response = await socket.ReceiveAsync(buffer, ct);

			// Handle WebSocket close message and terminate loop
			if ( response.MessageType == WebSocketMessageType.Close )
			{
				var description = response.CloseStatusDescription;
				var status = response.CloseStatus;

				logger.LogWarning($"WebSocket close received: {status} - {description}");
				await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);

				break;
			}

			// Parse and handle incoming message payload
			try
			{
				string responseText = Encoding.UTF8.GetString(buffer, 0, response.Count);
				using JsonDocument json = JsonDocument.Parse(responseText);

				// Determine message type for further processing
				string? messageType = json.RootElement.GetProperty("type").GetString();
				if ( messageType is null ) continue;

				// Handle price update message and update cache
				if ( messageType == "l1-update" )
				{
					var assetPrice = json.RootElement.Deserialize<AssetPrice>();
					ArgumentNullException.ThrowIfNull(assetPrice, nameof(assetPrice));

					// Update cache and evict old entries if needed
					var removePrice = priceCache.UpsertWithEviction(assetPrice);

					if ( removePrice is not null )
					{
						var command = new StreamSubscriptionCommand(removePrice, false);
						await SendSubscriptionCommand(command, ct);
					}
				}
			}
			catch ( Exception exception )
			{
				logger.LogError(exception, "Failed to deserialize websocket message");
			}
		}
	}


	private async Task KeepAliveLoopAsync(CancellationToken ct)
	{
		while ( !ct.IsCancellationRequested && socket.State == WebSocketState.Open )
		{
			try
			{
				await SendSocketMessageAsync(PingMessage, ct);
				await Task.Delay(KeepAliveInterval, ct);
			}
			catch ( OperationCanceledException )
			{
				// Expected when the task is cancelled
			}
			catch ( Exception exception )
			{
				logger.LogError(exception, "Error occurred in keep-alive loop.");
			}
		}
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task RestoreSubscriptionsAsync(CancellationToken ct)
	{
		foreach ( var price in priceCache.GetAllCached() )
		{
			var command = new StreamSubscriptionCommand(price, true);
			await SendSubscriptionCommand(command, ct);
		}
	}


	private async Task SendSubscriptionCommand(StreamSubscriptionCommand command, CancellationToken ct)
	{
		var payload = new
		{
			type         = "l1-subscription",
			instrumentId = command.Id,
			provider     = command.Provider,
			subscribe    = command.Subscribe,
			kinds        = new[] { "ask", "bid", "last" }
		};

		var message = Encoding.UTF8.GetBytes( JsonSerializer.Serialize(payload) );
		await SendSocketMessageAsync(message, ct);
	}

	// ------------------------------------------------------------------------------------------------------<

	private async Task SendSocketMessageAsync(byte[] message, CancellationToken ct)
	{
		await sendLock.WaitAsync(ct);

		try
		{
			if ( socket.State == WebSocketState.Open )
			{
				await socket.SendAsync(message, WebSocketMessageType.Text, true, ct);
			}
		}
		finally
		{
			sendLock.Release();
		}
	}


	private async Task<Uri> GetSocketConnectionUri(CancellationToken ct)
	{
		var token = await tokenService.GetAccessTokenAsync(ct);
		return new Uri($"wss://platform.fintacharts.com/api/streaming/ws/v1/realtime?token={token}");
	}

	// ------------------------------------------------------------------------------------------------------<
}