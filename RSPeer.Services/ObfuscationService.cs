using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using RSPeer.Common;
using RSPeer.Entities;
using RSPeer.Services.Base;

namespace RSPeer.Services
{
	public class ObfuscationService : IObfuscationService
	{
		public async Task<byte[]> Obfuscate(ObfuscateRequest request)
		{
			CreateDefaultDirectories();
			var obfuscationFolder = FileUtil.GetAssemblyPath("ObfuscationTemp");
			var guid = Guid.NewGuid();
			var outPath = Path.Combine(obfuscationFolder, $"{guid}-obbed.jar");
			var jarPath = Path.Combine(obfuscationFolder, $"{guid}.jar");
			File.WriteAllBytes(jarPath, request.Bytes);
			var path = await ObfuscateJar(jarPath, outPath, BuildObfuscateConfig(jarPath, outPath, request.Config));
			var bytes = File.ReadAllBytes(path);
			File.Delete(path);
			return bytes;
		}

		private async Task<string> ObfuscateJar(string jarPath, string outPath, string config)
		{
			CreateDefaultDirectories();
			var allatori = FileUtil.GetAssemblyPath("Assets/allatori.jar");
			var assets = FileUtil.GetAssemblyPath("Assets");
			if (!File.Exists(allatori)) throw new Exception("Allatori .jar does not exist, unable to obfuscate.");

			var obfuscationFolder = FileUtil.GetAssemblyPath("ObfuscationTemp");

			var configFilePath = Path.Combine(obfuscationFolder, $"{Guid.NewGuid()}.xml");

			await File.WriteAllTextAsync(configFilePath, config);

			var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

			var process = Process.Start(new ProcessStartInfo("java")
			{
				ArgumentList =
				{
					"-jar",
					allatori,
					configFilePath
				},
				WorkingDirectory = assets,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true
			});
			while (!process.StandardOutput.EndOfStream)
			{
				Console.WriteLine(await process.StandardOutput.ReadLineAsync());
			}
			while (!process.StandardError.EndOfStream)
			{
				Console.WriteLine(await process.StandardError.ReadLineAsync());
			}

			while (!process.HasExited)
			{
				Console.WriteLine("Waiting...");
				Thread.Sleep(100);
			}
			process?.WaitForExit();
			Console.WriteLine("Done.");
			File.Delete(configFilePath);
			File.Delete(jarPath);
			return outPath;
		}

		private string BuildObfuscateConfig(string jarPath, string outPath, string config)
		{
			return config.Replace("<jar placeholder=\"placeholder\"/>", $"<jar in=\"{jarPath}\" out=\"{outPath}\"/>");
		}

		private void CreateDefaultDirectories()
		{
			var obfuscationFolder = FileUtil.GetAssemblyPath("ObfuscationTemp");
			if (!Directory.Exists(obfuscationFolder)) Directory.CreateDirectory(obfuscationFolder);
		}
	}
}