 using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels.Enums
{
    public enum MetricType
    {
        [Description("Хранилище")] StorageUtilization = 1,
        [Description("Сервер")] ServerUtilization = 2,
        [Description("База данных")] DatabaseUtilization = 4,
    }

}
