using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IoTHubDashboard.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace IoTHubDashboard
{
	public class DeviceHttpFunctions
	{
		private const string JsonMediaType = "application/json";
		private static readonly Lazy<DeviceManagement> _lazyDeviceManagement = new Lazy<DeviceManagement>(() => new DeviceManagement(AppSettings.Instance));
		private static DeviceManagement _deviceManagement => _lazyDeviceManagement.Value;

		[FunctionName(nameof(GetDeviceCode))]
		public static async Task<HttpResponseMessage> GetDeviceCode(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "device/connect/{deviceID?}")] HttpRequestMessage req,
			string deviceID,
			TraceWriter log)
		{
			DeviceConnectionInfo info = await _deviceManagement.ConnectDeviceAsync(deviceID);

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(info), Encoding.UTF8, JsonMediaType)
			};
		}
	}
}