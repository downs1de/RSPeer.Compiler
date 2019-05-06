using System.IO;
using System.Reflection;

namespace RSPeer.Common
{
	public static class FileUtil
	{
		public static string GetAssemblyPath(string path)
		{
			var assembly = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			return Path.Combine(assembly, path);
		}
	}
}