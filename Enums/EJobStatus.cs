using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels.Enums {
	public enum EJobStatus {
		[Description("Ожидает запуска")] Waiting = 1,
		[Description("Выполняется")] Started = 2,
		[Description("Остановлена")] Stopped = 4,
		[Description("Успешно завершена")] OK = 8,
		[Description("Завершена с ошибкой")] Error = 16,
	}
}
