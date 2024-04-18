using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class ErrorCountItem
    {
        public ErrorCountItem() 
        {
            this.ErrorCount = 0;
            this.MicroserviceName = "Default";
            this.MicroserviceNameAndCount = "Default";
        }
        public string MicroserviceName { get; set; }
        public string MicroserviceNameAndCount { get; set; }
        public int ErrorCount { get; set; }
    }

}
