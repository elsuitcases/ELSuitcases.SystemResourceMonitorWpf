using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic.Devices;

namespace ELSuitcases.SystemResourceMonitorWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CONFIG_KEY_NAME_UPDATE_INTERVAL = "UPDATE_INTERVAL_SEC";
        private const int DEFAULT_UPDATE_INTERVAL_SEC = 1;

        private TimeSpan intervalUpdate = TimeSpan.FromSeconds(DEFAULT_UPDATE_INTERVAL_SEC);
        private Timer timerUsage;
        private ComputerInfo ciInfo;
        private PerformanceCounter pcCpu;        

        public CounterInfoRecord SystemCounterInfo
        {
            get; set;
        }



        public MainWindow()
        {
            InitializeComponent();

            SystemCounterInfo = new CounterInfoRecord();

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        

        private void TimerUsageCallback(object state)
        {
            SystemCounterInfo.CpuUsage = GetCpuUsage();
            SystemCounterInfo.CpuUsageTime = DateTime.Now;
            if (SystemCounterInfo.CpuUsageMin == 0)
            {
                SystemCounterInfo.CpuUsageMin = SystemCounterInfo.CpuUsage;
                SystemCounterInfo.CpuUsageMinTime = SystemCounterInfo.CpuUsageTime;
            }
            if ((SystemCounterInfo.CpuUsage > 0) && 
                (SystemCounterInfo.CpuUsage <= SystemCounterInfo.CpuUsageMin))
            {
                SystemCounterInfo.CpuUsageMin = SystemCounterInfo.CpuUsage;
                SystemCounterInfo.CpuUsageMinTime = SystemCounterInfo.CpuUsageTime;
            }
            if (SystemCounterInfo.CpuUsage >= SystemCounterInfo.CpuUsageMax)
            {
                SystemCounterInfo.CpuUsageMax = SystemCounterInfo.CpuUsage;
                SystemCounterInfo.CpuUsageMaxTime = SystemCounterInfo.CpuUsageTime;            
            }

            SystemCounterInfo.RamUsage = GetRamUsage();
            SystemCounterInfo.RamUsageTime = DateTime.Now;
            if (SystemCounterInfo.RamUsage <= SystemCounterInfo.RamUsageMin)
            {
                SystemCounterInfo.RamUsageMin = SystemCounterInfo.RamUsage;
                SystemCounterInfo.RamUsageMinTime = SystemCounterInfo.RamUsageTime;
            }
            if (SystemCounterInfo.RamUsage >= SystemCounterInfo.RamUsageMax)
            {
                SystemCounterInfo.RamUsageMax = SystemCounterInfo.RamUsage;
                SystemCounterInfo.RamUsageMaxTime = SystemCounterInfo.RamUsageTime;
            }

            SystemCounterInfo.GpuUsage = GetGpuUsage();
            SystemCounterInfo.GpuUsageTime = DateTime.Now;
            if (SystemCounterInfo.GpuUsageMin == 0)
            {
                SystemCounterInfo.GpuUsageMin = SystemCounterInfo.GpuUsage;
                SystemCounterInfo.GpuUsageMinTime = SystemCounterInfo.GpuUsageTime;
            }
            if ((SystemCounterInfo.GpuUsage > 0) &&
                (SystemCounterInfo.GpuUsage <= SystemCounterInfo.GpuUsageMin))
            {
                SystemCounterInfo.GpuUsageMin = SystemCounterInfo.GpuUsage;
                SystemCounterInfo.GpuUsageMinTime = SystemCounterInfo.GpuUsageTime;
            }
            if (SystemCounterInfo.GpuUsage >= SystemCounterInfo.GpuUsageMax)
            {
                SystemCounterInfo.GpuUsageMax = SystemCounterInfo.GpuUsage;
                SystemCounterInfo.GpuUsageMaxTime = SystemCounterInfo.GpuUsageTime;
            }

            SystemCounterInfo.HddCUsage = GetHddCUsage();
            SystemCounterInfo.HddCUsageTime = DateTime.Now;
            if (SystemCounterInfo.HddCUsage < SystemCounterInfo.HddCUsageMin)
            {
                SystemCounterInfo.HddCUsageMin = SystemCounterInfo.HddCUsage;
                SystemCounterInfo.HddCUsageMinTime = SystemCounterInfo.HddCUsageTime;
            }
            if (SystemCounterInfo.HddCUsage > SystemCounterInfo.HddCUsageMax)
            {
                SystemCounterInfo.HddCUsageMax = SystemCounterInfo.HddCUsage;
                SystemCounterInfo.HddCUsageMaxTime = SystemCounterInfo.HddCUsageTime;
            }

            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                tbCpuUsage.Text = string.Format("{0:F2} %", SystemCounterInfo.CpuUsage);
                tbRamUsage.Text = string.Format("{0:F2} %", SystemCounterInfo.RamUsage);
                tbGpuUsage.Text = string.Format("{0:F2} %", SystemCounterInfo.GpuUsage);
                tbHddCUsage.Text = string.Format("{0:F2} %", SystemCounterInfo.HddCUsage);

                tbCpuMin.Text = string.Format("{0:F2} %", SystemCounterInfo.CpuUsageMin);
                tbCpuMinTime.Text = string.Format("{0}", SystemCounterInfo.CpuUsageMinTime);
                tbCpuMax.Text = string.Format("{0:F2} %", SystemCounterInfo.CpuUsageMax);
                tbCpuMaxTime.Text = string.Format("{0}", SystemCounterInfo.CpuUsageMaxTime);
                tbRamMin.Text = string.Format("{0:F2} %", SystemCounterInfo.RamUsageMin);
                tbRamMinTime.Text = string.Format("{0}", SystemCounterInfo.RamUsageMinTime);
                tbRamMax.Text = string.Format("{0:F2} %", SystemCounterInfo.RamUsageMax);
                tbRamMaxTime.Text = string.Format("{0}", SystemCounterInfo.RamUsageMaxTime);
                tbGpuMin.Text = string.Format("{0:F2} %", SystemCounterInfo.GpuUsageMin);
                tbGpuMinTime.Text = string.Format("{0}", SystemCounterInfo.GpuUsageMinTime);
                tbGpuMax.Text = string.Format("{0:F2} %", SystemCounterInfo.GpuUsageMax);
                tbGpuMaxTime.Text = string.Format("{0}", SystemCounterInfo.GpuUsageMaxTime);
                tbHddCMin.Text = string.Format("{0:F2} %", SystemCounterInfo.HddCUsageMin);
                tbHddCMinTime.Text = string.Format("{0}", SystemCounterInfo.HddCUsageMinTime);
                tbHddCMax.Text = string.Format("{0:F2} %", SystemCounterInfo.HddCUsageMax);
                tbHddCMaxTime.Text = string.Format("{0}", SystemCounterInfo.HddCUsageMaxTime);
            }));
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAppConfig();
            Initialize(intervalUpdate);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (timerUsage != null)
            {
                timerUsage.Dispose();
                timerUsage = null;
            }
            if (pcCpu != null)
            {
                pcCpu.Close();
                pcCpu.Dispose();
                pcCpu = null;
            }
            if (ciInfo != null)
            {
                ciInfo = null;
            }
        }

        private void txtIntervalUpdateSec_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox ctl = (TextBox)e.Source;

            int interval = (int.TryParse(ctl.Text, out interval)) && (interval > 0) ?
                                interval :
                                DEFAULT_UPDATE_INTERVAL_SEC;

            ctl.Text = interval.ToString();
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            intervalUpdate = TimeSpan.FromSeconds(int.Parse(txtIntervalUpdateSec.Text));

            Initialize(intervalUpdate);

            SaveAppConfig();
        }



        private void Initialize(TimeSpan interval)
        {
            tbSystemHostName.Text = GetSystemHostName();
            tbSystemIP.Dispatcher.BeginInvoke(new Action(() =>
            {
                tbSystemIP.Text = GetSystemIPAddress();
            }));

            pcCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            pcCpu.NextValue();            
            ciInfo = new ComputerInfo();

            Thread.Sleep(500);

            SystemCounterInfo.CpuUsage = GetCpuUsage();
            SystemCounterInfo.CpuUsageTime = DateTime.Now;            
            SystemCounterInfo.CpuUsageMin = SystemCounterInfo.CpuUsage;
            SystemCounterInfo.CpuUsageMinTime = SystemCounterInfo.CpuUsageTime;

            SystemCounterInfo.RamUsage = GetRamUsage();
            SystemCounterInfo.RamUsageTime = DateTime.Now;
            SystemCounterInfo.RamUsageMin = SystemCounterInfo.RamUsage;
            SystemCounterInfo.RamUsageMinTime = SystemCounterInfo.RamUsageTime;

            SystemCounterInfo.GpuUsage = GetGpuUsage();
            SystemCounterInfo.GpuUsageTime = DateTime.Now;
            SystemCounterInfo.GpuUsageMin = SystemCounterInfo.GpuUsage;
            SystemCounterInfo.GpuUsageMinTime = SystemCounterInfo.GpuUsageTime;

            SystemCounterInfo.HddCUsage = GetHddCUsage();
            SystemCounterInfo.HddCUsageTime = DateTime.Now;
            SystemCounterInfo.HddCUsageMin = SystemCounterInfo.HddCUsage;
            SystemCounterInfo.HddCUsageMinTime = SystemCounterInfo.HddCUsageTime;

            if (timerUsage != null)
            {
                timerUsage.Dispose();
                timerUsage = null;
            }
            timerUsage = new Timer(TimerUsageCallback, null, 0, (int)interval.TotalMilliseconds);
        }

        private float GetCpuUsage()
        {
            if (pcCpu == null) 
                return 0;
            else 
                return pcCpu.NextValue();
        }

        private float GetRamUsage()
        {
            if (ciInfo == null)
                return 0;
            else
                return (((float)ciInfo.TotalPhysicalMemory - (float)ciInfo.AvailablePhysicalMemory) / (float)ciInfo.TotalPhysicalMemory) * 100;
        }

        private float GetGpuUsage()
        {
            float gpuUsage = 0;

            List<PerformanceCounter> lstPcGpu = new PerformanceCounterCategory("GPU Engine")
                                                    .GetInstanceNames()
                                                    .Where(name => name.EndsWith("engtype_3D"))
                                                    .Select(name => new PerformanceCounter("GPU Engine", "Utilization Percentage", name))
                                                    .ToList();

            if ((lstPcGpu != null) && (lstPcGpu.Count > 0))
            {
                try
                {
                    lstPcGpu.ForEach(c => c.NextValue());
                }
                catch (InvalidOperationException) { }
                catch (ArgumentNullException) { }
                catch (Exception) { }

                Thread.Sleep(500);

                try
                {
                    gpuUsage = lstPcGpu.Sum(c => c.NextValue());
                }
                catch (InvalidOperationException) { }
                catch (ArgumentNullException) { }
                catch (Exception) { }

                foreach (var pc in lstPcGpu)
                {
                    pc.Close();
                    pc.Dispose();
                }
            }

            return gpuUsage;
        }

        private float GetHddCUsage()
        {
            return GetDriveUsage("C");
        }

        private float GetDriveUsage(string driveLetter)
        {
            float driveUsage = 0;

            DriveInfo di = DriveInfo.GetDrives()
                                .Where(d => d.Name == string.Format("{0}:\\", driveLetter.ToUpper()))
                                .SingleOrDefault();
            if (di != null)
                driveUsage = ((float)(di.TotalSize - di.TotalFreeSpace) / di.TotalSize) * 100;

            return driveUsage;
        }

        private float GetAllGpuMemoryUsage()
        {
            return ((float)GetAllGpuMemoryUsedSize() / (float)GetAllGpuMemoryTotalSize()) * 100;
        }

        private ulong GetAllGpuMemoryTotalSize()
        {
            ulong gpuRamTotal = 0;

            var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");

            foreach (ManagementObject obj in searcher.Get())
            {
                ulong ramBytes = ulong.Parse(obj["AdapterRAM"].ToString());
                gpuRamTotal += ramBytes;

                obj.Dispose();
            }

            return gpuRamTotal;
        }

        private ulong GetAllGpuMemoryUsedSize()
        {
            ulong gpuRamUsed = 0;

            PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Adapter Memory");
            List<PerformanceCounter> lstPcGpu = category.GetInstanceNames()
                                                        .SelectMany(instance => category.GetCounters(instance))
                                                        .Where(counter => counter.CounterName == "Dedicated Usage")
                                                        .ToList();

            if ((lstPcGpu != null) && (lstPcGpu.Count > 0))
            {
                try
                {
                    gpuRamUsed = (ulong)lstPcGpu.Sum(counter => counter.NextValue());
                }
                catch (InvalidOperationException) { }
                catch (ArgumentNullException) { }
                catch (Exception) { }

                foreach (var pc in lstPcGpu)
                {
                    pc.Close();
                    pc.Dispose();
                }
            }

            return gpuRamUsed;
        }

        private string GetSystemHostName()
        {
            return Dns.GetHostName();
        }

        private string GetSystemIPAddress()
        {
            string ip = "";
            
            using (var client = new WebClient())
                ip = client.DownloadString("https://api.ipify.org");

            return ip;
        }

        private void LoadAppConfig()
        {
            if (ConfigurationManager.AppSettings[CONFIG_KEY_NAME_UPDATE_INTERVAL] == null)
            {
                intervalUpdate = TimeSpan.FromSeconds(1);
            }
            else
            {
                intervalUpdate = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings[CONFIG_KEY_NAME_UPDATE_INTERVAL]));
            }

            txtIntervalUpdateSec.Text = intervalUpdate.Seconds.ToString();
        }

        private void SaveAppConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 업데이트 주기
            if (config.AppSettings.Settings[CONFIG_KEY_NAME_UPDATE_INTERVAL] != null)
                config.AppSettings.Settings[CONFIG_KEY_NAME_UPDATE_INTERVAL].Value = intervalUpdate.Seconds.ToString();
            else
                config.AppSettings.Settings.Add(CONFIG_KEY_NAME_UPDATE_INTERVAL, intervalUpdate.Seconds.ToString());

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
