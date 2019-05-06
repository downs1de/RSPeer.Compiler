using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace RSPeer.Compiler
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var builder = WebHost.CreateDefaultBuilder(args)
				.ConfigureKestrel(((context, options) =>
				{
					options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
				}))
				.UseApplicationInsights()
				.UseStartup<Startup>();
			if (env == "Development")
			{
				builder.UseUrls("http://*:5000");
			}
			return builder;
		}
	}
}