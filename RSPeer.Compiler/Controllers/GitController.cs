using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RSPeer.Compiler.Controllers.Base;
using RSPeer.Entities;
using RSPeer.Services.Base;

namespace RSPeer.Compiler.Controllers
{
	public class GitController : BaseController
	{
		private readonly IGitToJarService _service;
		private readonly IObfuscationService _obfuscation;

		public GitController(IGitToJarService service, IObfuscationService obfuscation)
		{
			_service = service;
			_obfuscation = obfuscation;
		}

		[HttpPost]
		public async Task<IActionResult> BuildJar([FromBody] CompileRequest request)
		{
			try
			{
				var bytes = await _service.BuildJarFromGit(request.GitPath);
				bytes = await _obfuscation.Obfuscate(new ObfuscateRequest
				{
					Bytes = bytes,
					Config = request.ObfuscateConfig
				});
				return File(bytes, "application/java-archive", "result.jar");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				var split = e.Message.Split("@@");
				return BadRequest(JsonConvert.SerializeObject(new
				{
					Logs = split[0],
					Errors = split.Length > 1 ? split[1] : string.Empty
				}));
			}
		}
	}
}