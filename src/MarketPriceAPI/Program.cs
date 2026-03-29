namespace MarketPriceAPI;

// Namespaces used by this file
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MarketPriceAPI.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MarketPriceAPI.Repositories;
using MarketPriceAPI.Interfaces;
using DotNetEnv.Configuration;
using MarketPriceAPI.Services;
using MarketPriceAPI.Data;
using MongoDB.Driver;
using Asp.Versioning;
using System.Linq;
using System;

// Main content of the file
public static class Program
{
	// ^ ----------------------------------------------------------------------------------------------------<

	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Load environment variables from system and .env file
		builder.Configuration.AddEnvironmentVariables();
		builder.Configuration.AddDotNetEnv();

		// Configure options, hosted services, repositories, database, and other services
		builder.ConfigureOptions();
		builder.ConfigureHostedServices();
		builder.ConfigureRespositoryServices();
		builder.ConfigureDatabaseServices();
		builder.ConfigureServices();

		// Build the WebApplication and map all endpoints
		var app = builder.Build().WithEndpoints();

		// Enable HTTPS redirection for incoming requests
		app.UseHttpsRedirection();

		// Enable a development mode only conventions
		if ( app.Environment.IsDevelopment() )
		{
			app.MapOpenApi();
		}

		// Enable Swagger middleware for API documentation
		bool forceSwagger = app.Configuration.GetValue<bool>("FORCE_SWAGGER");

		if ( app.Environment.IsDevelopment() || forceSwagger )
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.Run(); // Run application
	}

	// @ ----------------------------------------------------------------------------------------------------<

	private static void ConfigureOptions(this WebApplicationBuilder builder)
	{
		builder.Services.AddOptions<AuthOptions>().Configure((options) =>
		{
			var configuration = builder.Configuration;

			options.FintachartsUsername = configuration.GetValue("FINTACHARTS_USERNAME", string.Empty);
			options.FintachartsPassword = configuration.GetValue("FINTACHARTS_PASSWORD", string.Empty);

			options.MongoDbUsername = configuration.GetValue("MONGODB_USERNAME", string.Empty);
			options.MongoDbPassword = configuration.GetValue("MONGODB_PASSWORD", string.Empty);
		})
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Bind MongoDB connection settings from configuration with fallback defaults
		builder.Services.AddOptions<MongoDbOptions>().Configure((options) =>
		{
			var configuration = builder.Configuration;

			options.Host = configuration.GetValue("MongoDB:Host", "localhost");
			options.Auth = configuration.GetValue("MongoDB:Auth", "admin");
			options.Port = configuration.GetValue("MongoDB:Port", 27017u);
		})
			.ValidateDataAnnotations()
			.ValidateOnStart();
	}


	private static void ConfigureHostedServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddHostedService<AssetRefreshService>();

		// Register hosted services as singletons for DI access
		builder.Services.AddSingleton<AssetPriceStreamService>();
		builder.Services.AddSingleton<MetadataRefreshService>();

		// Use the existing singleton instances as hosted services
		builder.Services.AddHostedService(
			sp => sp.GetRequiredService<AssetPriceStreamService>());
		builder.Services.AddHostedService(
			sp => sp.GetRequiredService<MetadataRefreshService>());
	}


	private static void ConfigureRespositoryServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddSingleton<IMarketAssetRepository, MarketAssetRepository>();
		builder.Services.AddSingleton<IAssetPriceRepository, AssetPriceRepository>();
	}


	private static void ConfigureDatabaseServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddSingleton<IMongoClient>((sp) =>
		{
			var mongoDbOptions = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
			var authOptions = sp.GetRequiredService<IOptions<AuthOptions>>().Value;

			var auth = $"{authOptions.MongoDbUsername}:{authOptions.MongoDbPassword}";
			var host = $"{mongoDbOptions.Host}:{mongoDbOptions.Port}";

			return new MongoClient($"mongodb://{auth}@{host}/?authSource={mongoDbOptions.Auth}");
		});

		builder.Services.AddSingleton<IMarketDatabaseContext>((sp) =>
		{
			var client = sp.GetRequiredService<IMongoClient>();
			return new MarketDatabaseContext(client, "market");
		});
	}


	private static void ConfigureServices(this WebApplicationBuilder builder)
	{
		builder.Services.AddSingleton<IPriceHistoryService, PriceHistoryService>();
		builder.Services.AddSingleton<IAssetPriceCache, AssetPriceCache>();
		builder.Services.AddSingleton<ITokenService, TokenService>();

		// Register hosted services for DI access
		builder.Services.AddSingleton<IAssetPriceStreamService>(
			sp => sp.GetRequiredService<AssetPriceStreamService>());
		builder.Services.AddSingleton<IMetadataService>(
			sp => sp.GetRequiredService<MetadataRefreshService>());

		// Add HTTP client and API documentation tools
		builder.Services.AddHttpClient();
		builder.Services.AddSwaggerGen();
		builder.Services.AddOpenApi();

		// Configure API versioning and API explorer options
		builder.Services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new ApiVersion(1.0);
			options.ReportApiVersions = true;
		})
		.AddApiExplorer(options =>
		{
			options.SubstituteApiVersionInUrl = true;
			options.GroupNameFormat = "'v'VVV";
		});
	}

	// ------------------------------------------------------------------------------------------------------<

	private static WebApplication WithEndpoints(this WebApplication application)
	{
		var assembly = typeof(Program).Assembly;

		// Create and configure an API version set for versioning support
		var apiVersionSet = application.NewApiVersionSet()
			.HasApiVersion( new ApiVersion(1.0) )
			.ReportApiVersions()
			.Build();

		var versionedBuilder = application
			.MapGroup("/api/v{version:apiVersion}")
			.WithApiVersionSet(apiVersionSet);

		// Discover all non-abstract, non-interface types implementing IEndpointDefinition
		var endpointTypes = assembly.GetTypes()
			.Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t))
			.Where(t => t is { IsAbstract: false, IsInterface: false });

		// Iterate through discovered endpoint types
		foreach ( var type in endpointTypes )
		{
			if ( type.GetConstructor(Type.EmptyTypes) is null )
			{
				application.Logger.LogError(
					"Endpoint class {EndpointType} requires " +
					"a parameterless constructor.", type.FullName
				);

				throw new InvalidOperationException(
					"Invalid endpoint configuration: " +
					$"{type.FullName} must have a parameterless constructor."
				);
			}

			// Instantiate the endpoint and invoke its Map method to register routes
			var obj = Activator.CreateInstance(type)!;
			((IEndpointDefinition) obj).Map(versionedBuilder);
		}

		return application; // Return application (no way)
	}

	// ------------------------------------------------------------------------------------------------------<
}