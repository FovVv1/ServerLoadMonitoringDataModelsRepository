using CefSharp.DevTools.Performance;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class ServerMetricCollector : INotifyPropertyChanged
    {
        public ServerMetricCollector() {
            this.Ip = "";
            this.MetricCollection = new ObservableCollection<ServerUtilization>();
        }
        public ServerMetricCollector(string ip, ObservableCollection<ServerUtilization> metricCollection)
        {
            this.Ip = ip;
            this.MetricCollection = metricCollection;
        }

        

        public string Ip { get; set; }



        //private ServerUtilization _staticMetric;
        //public ServerUtilization StaticMetric
        //{
        //    get => _staticMetric;
        //    set
        //    {
        //        if (_staticMetric != value)
        //        {
        //            _staticMetric = value;
        //            OnPropertyChanged(nameof(StaticMetric));
        //        }
        //    }
        //}

        private ObservableCollection<ServerUtilization> _metricCollection;
        public ObservableCollection<ServerUtilization> MetricCollection
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
        public object Clone()
        {
            ServerMetricCollector clone = new ServerMetricCollector
            {
                Ip = this.Ip,
                MetricCollection = this.MetricCollection,
                
            };
            return clone;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
