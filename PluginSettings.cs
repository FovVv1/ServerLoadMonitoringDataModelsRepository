using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels {
	public class PluginSettings {
		public string PluginName { get; set; }
		public string SettingName { get; set; }
		public long Value { get; set; }
		public string StringValue { get; set; }
	}
}
