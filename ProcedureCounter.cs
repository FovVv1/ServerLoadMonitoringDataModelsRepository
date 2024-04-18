using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class ProcedureCounter : INotifyPropertyChanged
    {
        private int chartAngle;

        public int ChartAngle
        {
            get { return chartAngle; }
            set
            {
                if (chartAngle != value)
                {
                    chartAngle = value;
                    OnPropertyChanged(nameof(ChartAngle));
                }
            }
        }

        public string MicroserviceName { get; set; }
        public int ProcedureCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
