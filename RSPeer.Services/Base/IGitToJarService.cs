using System.Threading.Tasks;

namespace RSPeer.Services.Base
{
	public interface IGitToJarService
	{
		Task<byte[]> BuildJarFromGit(string repository);
	}
}