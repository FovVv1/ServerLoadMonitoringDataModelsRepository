using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ServerLoadMonitoringDataModels.Enums;

namespace ServerLoadMonitoringDataModels {
	
	public class Job {
		/// <summary>Статус выполнения работы</summary>
		public EJobStatus Status { get; set; }
		/// <summary>Последнее сообщение</summary>
		public string LastMessage { get; set; }
		
		/// <summary>Имя работы</summary>
		public string Name { get; set; }
		
		/// <summary>Путь сборки</summary>
		public string DLL { get; set; }
		
		/// <summary>Описание</summary>
		public string Description { get; set; }
		
		/// <summary>Время от начала недели, через которое нужно запускать</summary>
		public int ScheduleTime { get; set; }
		
		/// <summary>Словарь свойств</summary>
		public Dictionary<string, string> Properties { get; set; }

		/// <summary>Коллекция путей обрабатываемых файлов</summary>
		public List<string> AllFilesPath { get; set; }

		/// <summary>Коллекция путей обрабатываемых директорий</summary>
		public List<string> AllDirectoryPath { get; set; }
	}
}
