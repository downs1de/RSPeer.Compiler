using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace RSPeer.Compiler.Middleware
{
	public class AuthorizationFilter : IAuthorizationFilter
	{
		private readonly IConfiguration _configuration;

		public AuthorizationFilter(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var auth = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
			if (auth != _configuration.GetValue<string>("Compiler:Token"))
			{
				context.Result = new UnauthorizedResult();
			}
		}
	}
}