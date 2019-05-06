using System.Threading.Tasks;

namespace RSPeer.Services.Base
{
	public interface IRspeerApiService
	{
		Task<byte[]> GetCurrentJar();
		Task<decimal> GetBotVersion();
	}
}