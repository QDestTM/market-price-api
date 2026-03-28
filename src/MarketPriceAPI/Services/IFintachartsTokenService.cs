namespace MarketPriceAPI.Services;

// Namespaces used by this file
using System.Threading.Tasks;
using System.Threading;

// Main content of the file
public interface IFintachartsTokenService
{
	// ^ ----------------------------------------------------------------------------------------------------<

	Task<string> GetAccessTokenAsync(CancellationToken ct);

	// ------------------------------------------------------------------------------------------------------<
}