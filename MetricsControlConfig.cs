using ElMessage;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class MetricsControlConfig
    {
        public MetricsControlConfig(string Ip, MetricType Type, int Interval)
        {
            this.metricIp = Ip;
            this.metricType = Type;
            this.metricCheckInterval = Interval;
        }
        public string metricIp { get; set; }
        public MetricType metricType { get; set; }
        public int metricCheckInterval { get; set; }

    }
}
