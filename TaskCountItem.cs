using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLoadMonitoringDataModels
{
    public class TaskCountItem
    {
        public TaskCountItem()
        {
            this.Count = 0;
            this.LAN = "Default";
            this.LANAndCount= "Default";
        }
        public string LAN { get; set; }
        public string LANAndCount { get; set; }
        public int Count { get; set; }
    }
}
