using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AppleDev.Test
{
	public class SimCtlTests
	{
		private readonly ITestOutputHelper _testOutputHelper;

		public SimCtlTests(ITestOutputHelper testOutputHelper)
		{
			_testOutputHelper = testOutputHelper;
		}

		[Fact]
		public async Task GetAnySimulators()
		{
			var simCtl = new AppleDev.SimCtl();
			var sims = await simCtl.GetSimulatorsAsync().ConfigureAwait(false);
			
			Assert.NotNull(sims);
			Assert.NotEmpty(sims);

			foreach (var sim in sims)
			{
				_testOutputHelper.WriteLine($"{sim.Name} - {sim.Runtime} - {sim.Udid}");
			}
		}

		[Fact]
		public async Task GetAnySimulatorGroups()
		{
			var simCtl = new AppleDev.SimCtl();
			var simGrps = await simCtl.GetSimulatorGroupsAsync().ConfigureAwait(false);

			Assert.NotNull(simGrps);
			Assert.NotEmpty(simGrps);

			foreach (var sg in simGrps)
			{
				_testOutputHelper.WriteLine($"{sg.Name} - {sg.ProductFamily} - {sg.Identifier} - {sg.BundlePath}");
				
				Assert.NotEmpty(sg.Devices);
				
				foreach (var d in sg.Devices)
					_testOutputHelper.WriteLine($"{d.Name} - {d.Runtime} - {d.Udid}");
			}
		}

		[Fact]
		public async Task ControlSimulator()
		{
			var simctl = new SimCtl();
			var sims = await simctl.GetSimulatorsAsync();

			var s = sims.FirstOrDefault(s => s.DeviceType?.ProductFamily?.Contains("iPhone") ?? false);

			var udid = s?.Udid;
			
			Assert.NotEmpty(udid);
			
			// Erase the sim first
			await simctl.EraseAsync(udid);

			// Open simulator.app
			//Assert.True(await simctl.OpenSimulatorAppAsync(udid));

			// Boot
			Assert.True(await simctl.BootAsync(udid));
			
			// Wait for boot complete
			Assert.True(await simctl.WaitForBootedAsync(udid, TimeSpan.FromSeconds(120)));

			// Verify we found a sim
			var booted = (await simctl.GetSimulatorsAsync())
				.FirstOrDefault(i => !string.IsNullOrEmpty(i.Udid) && i.Udid == udid && i.IsBooted);
			
			Assert.NotNull(booted);
		}
		
		
	}
}
