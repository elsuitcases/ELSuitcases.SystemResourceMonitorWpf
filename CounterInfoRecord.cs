using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.SystemResourceMonitorWpf
{
    public class CounterInfoRecord : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public float CpuUsage = 0;
        public DateTime CpuUsageTime = DateTime.MinValue;

        public float CpuUsageMin = 0;
        public DateTime CpuUsageMinTime = DateTime.MinValue;

        public float CpuUsageMax = 0;
        public DateTime CpuUsageMaxTime = DateTime.MinValue;

        public float RamUsage = 0;
        public DateTime RamUsageTime = DateTime.MinValue;

        public float RamUsageMin = 0;
        public DateTime RamUsageMinTime = DateTime.MinValue;

        public float RamUsageMax = 0;
        public DateTime RamUsageMaxTime = DateTime.MinValue;

        public float GpuUsage = 0;
        public DateTime GpuUsageTime = DateTime.MinValue;

        public float GpuUsageMin = 0;
        public DateTime GpuUsageMinTime = DateTime.MinValue;

        public float GpuUsageMax = 0;
        public DateTime GpuUsageMaxTime = DateTime.MinValue;

        public float HddCUsage = 0;
        public DateTime HddCUsageTime = DateTime.MinValue;

        public float HddCUsageMin = 0;
        public DateTime HddCUsageMinTime = DateTime.MinValue;

        public float HddCUsageMax = 0;
        public DateTime HddCUsageMaxTime = DateTime.MinValue;
    }
}
