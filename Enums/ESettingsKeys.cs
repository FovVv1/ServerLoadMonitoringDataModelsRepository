using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels.Enums {
	public enum ESettingsKeys {
		[Description("Время автообновления данных")] Settings = 1,
		[Description("Состояние сервера (выполнения работ бэкапера)")] CurrentJobsStatus = 2,
		[Description("Пусть к корневому каталогу бэкапера")] BackupMasterPath = 4,
	}
}
