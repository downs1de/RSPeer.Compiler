using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RSPeer.Compiler.Controllers.Base;
using RSPeer.Entities;
using RSPeer.Services.Base;

namespace RSPeer.Compiler.Controllers
{
	public class ObfuscateController : BaseController
	{
		private readonly IObfuscationService _service;

		public ObfuscateController(IObfuscationService service)
		{
			_service = service;
		}

		[HttpPost]
		public async Task<IActionResult> Execute(ObfuscateRequest request)
		{
			var bytes = await _service.Obfuscate(request);
			return File(bytes, "application/java-archive", "result.jar");
		}
	}
}