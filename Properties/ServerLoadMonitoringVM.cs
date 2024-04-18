using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoring {
	public class ServerLoadMonitoringDataModelVm:ServerLoadMonitoringDataModels.ServerLoadMonitoring, INotifyPropertyChanged {
		#region OnPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "") {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
		#endregion

		public  new string Name {
			get => base.Name;
			set {
				base.Name = value;
				OnPropertyChanged("Name");
			}
		}
	}
}
