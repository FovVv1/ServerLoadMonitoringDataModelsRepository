using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class StorageMetricCollector : INotifyPropertyChanged
    {

        public StorageMetricCollector()
        {
            this.Ip = "";
            this.MetricCollection = new ObservableCollection<StorageUtilization>();
        }
        public StorageMetricCollector(string ip, ObservableCollection<StorageUtilization> metricCollection)
        {
            this.Ip = ip;
            this.MetricCollection = metricCollection;
        }

      
        public string Ip { get; set; }




        private ObservableCollection<StorageUtilization> _metricCollection;
        public ObservableCollection<StorageUtilization> MetricCollection
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
            StorageMetricCollector clone = new StorageMetricCollector
            {
                Ip = this.Ip,
                MetricCollection = this.MetricCollection,   
            };
            return clone;
        }
    }
}
