using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AppleAppStoreConnect;

partial class AppStoreConnectClient
{
	public const string DEVICES_TYPE = Device.TYPE;

	public async Task<DeviceResponse> ListDevicesAsync(
		string[]? filterId = null,
		string[]? filterIdentifier = null,
		string[]? filterName = null,
		Platform[]? filterPlatform = null,
		DeviceStatus[]? filterStatus = null,
		string[]? filterUdid = null,
		string? include = null,
		string? sort = null,
		int? limit = null,
		int? limitProfiles = null,
		int? limitBundleIdCapabilities = null,
		string[]? fieldsDevices = null,
		CancellationToken cancellationToken = default)
	{
		var qs = new QueryStringBuilder();

		qs.Filter("id", filterId);
		qs.Filter("name", filterName);
		qs.Filter("platform", filterPlatform);
		qs.Filter("status", filterStatus);
		qs.Filter("udid", filterUdid);

		qs.Include(include, "app", "bundleIdCapabilities", PROFILES_TYPE);
		qs.Sort(sort, "id", "-id", "name", "-name", "platform", "-platform", "status", "-status", "udid", "-udid");

		qs.Limit(nameof(limit), null, 200, limit);
		
		qs.Fields(DEVICES_TYPE, fieldsDevices);

		return await RequestAsync<DeviceResponse>(DEVICES_TYPE, qs, cancellationToken).ConfigureAwait(false)
			?? new DeviceResponse();
	}

	public async Task<ItemResponse<Device, DeviceAttributes>> RegisterDeviceAsync(
		DeviceAttributes deviceAttributes,
		CancellationToken cancellationToken = default)
	{
		// Prepare only allowed attributes for create
		var createAttrs = new DeviceCreateRequestAttributes
		{
			Name = deviceAttributes.Name,
			Platform = string.IsNullOrEmpty(deviceAttributes.PlatformValue) && deviceAttributes.Platform != Platform.Unknown ? deviceAttributes.Platform.ToString() : deviceAttributes.PlatformValue,
			Udid = deviceAttributes.Udid
		};

		var token = Configuration.AccessToken;
		http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

		if (Environment.GetEnvironmentVariable("APPLEDEV_DEBUG") == "1")
		{
			var json = System.Text.Json.JsonSerializer.Serialize(new Request<DeviceCreateRequestAttributes>(new RequestData<DeviceCreateRequestAttributes>(createAttrs){ Type = Device.TYPE }), JsonSerializerOptions);
			Console.Error.WriteLine($"[DEBUG] RegisterDevice Request: {json}");
		}

		// Use generic create overload to wrap request
		var response = await PostAsync<Device, DeviceAttributes, DeviceCreateRequestAttributes>(DEVICES_TYPE,
			createAttrs,
			cancellationToken).ConfigureAwait(false);

		if (response == null || string.IsNullOrEmpty(response.Data?.Id))
		{
			throw new Exception("Device registration failed or returned no ID.");
		}

		return response;
	}


	public async Task<ItemResponse<Device, DeviceAttributes>> ModifyDeviceAsync(
		string id,
		DeviceAttributes deviceAttributes,
		CancellationToken cancellationToken = default)
	{

		return await PatchAsync<Device, DeviceAttributes>($"{DEVICES_TYPE}/{id}", deviceAttributes, cancellationToken).ConfigureAwait(false)
			?? new ItemResponse<Device, DeviceAttributes>();
	}
}
