using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RSPeer.Services.Base;

namespace RSPeer.Services
{
	public class RsPeerApiService : IRspeerApiService
	{
		private readonly HttpClient _client;

		public RsPeerApiService(IConfiguration configuration, IHttpClientFactory factory)
		{
			_client = factory.CreateClient("RsPeerApi");
			_client.BaseAddress = new Uri(configuration.GetValue<string>("RspeerApi:Path"));
		}

		public async Task<byte[]> GetCurrentJar()
		{
			return await _client.GetByteArrayAsync("/api/bot/currentJar");
		}

		public async Task<decimal> GetBotVersion()
		{
			var response = await _client.GetStringAsync("/api/bot/currentVersionRaw");
			return decimal.Parse(response);
		}
	}
}