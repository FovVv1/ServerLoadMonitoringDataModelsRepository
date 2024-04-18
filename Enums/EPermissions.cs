using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels.Enums {
	[Flags]
	public enum EPermissions:long {
		[Description("Обновление данных")] IsRefreshDataEnabled = 1,
		


	}
}
