using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{

    public interface IMetricCollector : ICloneable
    {
        IMetric LastMetric { get; set; }
        string Ip { get; set; }
        IMetric StaticMetric { get; set; }
        ObservableCollection<IMetric> MetricCollection { get; set; }

    }
}
