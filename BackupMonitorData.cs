using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels {
	public class BackupMonitorData {
		/// <summary>
		/// Дата формирования
		/// </summary>
		public DateTime RefreshingData { get; set; }
		/// <summary>
		/// Список агентов с состоянием выполнения работ
		/// </summary>
		public List<AgentConfig> AgentsConfigs { get; set; }
	}
}
