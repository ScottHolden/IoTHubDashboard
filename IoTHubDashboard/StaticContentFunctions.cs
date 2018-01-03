using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace IoTHubDashboard
{
	public class StaticContentFunctions
	{
		[FunctionName(nameof(StaticDevicePage))]
		public static HttpResponseMessage StaticDevicePage(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/device")] HttpRequestMessage req) => GetStaticFile("device.html");

		[FunctionName(nameof(StaticDashboardPage))]
		public static HttpResponseMessage StaticDashboardPage(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/dashboard")] HttpRequestMessage req) => GetStaticFile("dashboard.html");

		private static HttpResponseMessage GetStaticFile(string name) => new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(GetCachedFile(name), Encoding.UTF8, "text/html")
		};

		private static readonly ConcurrentDictionary<string, string> MiniCache = new ConcurrentDictionary<string, string>();

		private static string GetCachedFile(string name) => MiniCache.GetOrAdd(name, LoadFile);

		private static string LoadFile(string name) => File.ReadAllText(GetAppDataPath() + @"\" + name, Encoding.UTF8);

		private static string GetAppDataPath() => Path.Combine(GetEnvironmentVariable("HOME"), @"site\wwwroot\App_Data");

		private static string GetEnvironmentVariable(string name) => System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
	}
}