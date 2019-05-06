using System.Threading.Tasks;
using RSPeer.Entities;

namespace RSPeer.Services.Base
{
	public interface IObfuscationService
	{
		Task<byte[]> Obfuscate(ObfuscateRequest request);
	}
}