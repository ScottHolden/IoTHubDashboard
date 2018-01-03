using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace IoTHubDashboard
{
	public class StreamHttpFunctions
	{
		[FunctionName(nameof(StreamTrigger))]
		public static async Task StreamTrigger(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			string data = await req.Content.ReadAsStringAsync();

			log.Info(data);
		}
	}
}