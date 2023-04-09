using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleDev.Test
{
	public class SimCtlTests
	{
		[Fact]
		public async Task GetAnySimulators()
		{
			var simCtl = new AppleDev.SimCtl();
			var sims = await simCtl.GetSimulatorsAsync().ConfigureAwait(false);

			Assert.NotNull(sims);
			Assert.NotEmpty(sims);
		}

		[Fact]
		public async Task GetAnySimulatorGroups()
		{
			var simCtl = new AppleDev.SimCtl();
			var simGrps = await simCtl.GetSimulatorGroupsAsync().ConfigureAwait(false);

			Assert.NotNull(simGrps);
			Assert.NotEmpty(simGrps);

			foreach (var sg in simGrps)
				Assert.NotEmpty(sg.Devices);
		}
	}
}
