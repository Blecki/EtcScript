using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EtcScriptEmu
{
	public class HostSettings
	{
		public String Path;
		public String DisplayName;
	}

	public class Settings
	{
		public String DefaultHost;
		public List<String> OpenFiles = new List<String>();
		public List<HostSettings> Hosts = new List<HostSettings>();
	}
}
