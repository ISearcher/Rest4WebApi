using System.Collections.Generic;

namespace Common.DTO
{
	public class ClientVersionDto
	{
		public decimal Version { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public IList<string> StableDeviceList { get; set; }

		public IList<string> CurrentDeviceList { get; set; }
	}
}
