using System;
using System.Configuration;
using IoTHubDashboard.Service;

namespace IoTHubDashboard
{
	public class AppSettings : DeviceManagement.ISettings
	{
		private static readonly Lazy<AppSettings> LazyInstance = new Lazy<AppSettings>();
		public static AppSettings Instance => LazyInstance.Value;

		public string IoTHubConnectionString => GetSetting(nameof(IoTHubConnectionString));
		public string IoTHubHostName => GetSetting(nameof(IoTHubHostName));

		private string GetSetting(string name)
		{
			string value = ConfigurationManager.AppSettings[name];

			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ConfigurationErrorsException($"AppSetting '{name}' was missing or empty.");
			}

			return value;
		}
	}
}