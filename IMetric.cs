using ElMessage;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public interface IMetric : ICloneable
    {
        string Ip { get; set; }

        int UsersCount { get; set; }
        int CollectionNumber { get; set; }
        int Placement { get; set; }
        DateTime RefreshingData { get; set; }
        MetricType Type { get; set; }

        long LastCheckTime { get; set; }

        long CheckInterval { get; set; }
        void GetMetric(ElConnectionClient elConnectionClient, ElMessageServer elMessageServer);

        object Clone();
        //Временная мера и ненужное
        List<ProcedureCounter> MicroservicesProceduresCount
        {
            get; set;
        }
    }
}
