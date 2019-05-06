using Microsoft.AspNetCore.Mvc;
using RSPeer.Compiler.Middleware;

namespace RSPeer.Compiler.Controllers.Base
{
	[ApiController]
	[Route("api/[controller]/[action]")]
	[Authorize]
	public abstract class BaseController : Controller
	{
	}
}