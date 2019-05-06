using Microsoft.AspNetCore.Mvc;

namespace RSPeer.Compiler.Middleware
{
	public class AuthorizeAttribute : TypeFilterAttribute
	{
		public AuthorizeAttribute() : base(typeof(AuthorizationFilter))
		{
		}
	}
}