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
		return await PostAsync<Device, DeviceAttributes>(DEVICES_TYPE, deviceAttributes, cancellationToken).ConfigureAwait(false)
			?? new ItemResponse<Device, DeviceAttributes>();
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
