using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace IoTHubDashboard.Service
{
	public class DeviceManagement
	{
		private const string DevicePrefix = "device-";
		private static readonly Regex DeviceIDRegex = new Regex("^" + DevicePrefix + "[a-f0-9]{32}$", RegexOptions.Singleline);
		private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

		public interface ISettings
		{
			string IoTHubConnectionString { get; }
			string IoTHubHostName { get; }
		}

		private readonly RegistryManager _manager;
		private readonly string _hostname;

		public DeviceManagement(ISettings settings)
		{
			_manager = RegistryManager.CreateFromConnectionString(settings.IoTHubConnectionString);
			_hostname = settings.IoTHubHostName;
		}

		public async Task<DeviceConnectionInfo> ConnectDeviceAsync(string deviceID)
		{
			// Only work in lowercase, cos stuff.
			deviceID = deviceID?.ToLower();

			if (string.IsNullOrWhiteSpace(deviceID) || !DeviceIDRegex.IsMatch(deviceID))
			{
				deviceID = GenerateNewDeviceID();
			}

			Device device;
			try
			{
				device = await _manager.AddDeviceAsync(new Device(deviceID));
			}
			catch (DeviceAlreadyExistsException)
			{
				device = await _manager.GetDeviceAsync(deviceID);
			}

			return GetConnectionInfo(device);
		}

		private DeviceConnectionInfo GetConnectionInfo(Device device)
		{
			string resourceUri = _hostname + "/devices/" + device.Id;

			string password = GenerateSasToken(resourceUri, device.Authentication.SymmetricKey.PrimaryKey);

			return new DeviceConnectionInfo
			{
				DeviceID = device.Id,
				HostName = _hostname,
				Password = password
			};
		}

		private string GenerateNewDeviceID()
		{
			return DevicePrefix + Guid.NewGuid().ToString("n");
		}

		private string GenerateSasToken(string resourceUri, string key)
		{
			int expiryInSeconds = 3600;

			TimeSpan fromEpochStart = DateTime.UtcNow - Epoch;

			string expiry = Convert.ToString((int)fromEpochStart.TotalSeconds + expiryInSeconds);

			string encodedResourceUri = WebUtility.UrlEncode(resourceUri).ToLower();

			string stringToSign = encodedResourceUri + "\n" + expiry;

			string signature = GenerateSignature(key, stringToSign);

			string encodedSignature = WebUtility.UrlEncode(signature);

			return $"SharedAccessSignature sr={encodedResourceUri}&sig={encodedSignature}&se={expiry}";
		}

		private string GenerateSignature(string key, string stringToSign)
		{
			using (HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key)))
			{
				return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
			}
		}
	}
}