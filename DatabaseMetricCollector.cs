using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class DatabaseMetricCollector : INotifyPropertyChanged
    {
        public DatabaseMetricCollector()
        {
            this.Ip = "";
            this.MetricCollection = new ObservableCollection<DatabaseUtilization>();
        }
        public DatabaseMetricCollector(string ip, ObservableCollection<DatabaseUtilization> metricCollection)
        {
            this.Ip = ip;
            this.MetricCollection = metricCollection;
        }


        public string Ip { get; set; }




        private ObservableCollection<DatabaseUtilization> _metricCollection;
        public ObservableCollection<DatabaseUtilization> MetricCollection
        {
            get => _metricCollection;
            set
            {
                if (_metricCollection != value)
                {
                    _metricCollection = value;
                    OnPropertyChanged(nameof(MetricCollection));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public object Clone()
        {
            DatabaseMetricCollector clone = new DatabaseMetricCollector
            {
                Ip = this.Ip,
                MetricCollection = this.MetricCollection,
            };
            return clone;
        }
    }
}
