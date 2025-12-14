using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private TimeSpan intervalUpdate = TimeSpan.FromMilliseconds(1000);
        private Timer timerUsage;
        private ComputerInfo ciInfo;
        private PerformanceCounter pcCpu;
        private List<PerformanceCounter> lstPcGpu;
        private DriveInfo diHddC;

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
            if (SystemCounterInfo.GpuUsage <= SystemCounterInfo.GpuUsageMin)
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
            if (lstPcGpu != null)
            {
                foreach (var c in lstPcGpu)
                {
                    c.Close();
                    c.Dispose();
                }

                lstPcGpu.Clear();
                lstPcGpu = null;
            }
            if (diHddC != null)
            {
                diHddC = null;
            }
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            int intervalSec = 1;
            
            if ((string.IsNullOrEmpty(txtIntervalUpdateSec.Text)) || (!int.TryParse(txtIntervalUpdateSec.Text, out intervalSec)))
            {
                intervalSec = 1;
                txtIntervalUpdateSec.Text = intervalSec.ToString();
            }
            else
            {
                intervalSec = int.Parse(txtIntervalUpdateSec.Text);
            }

            intervalUpdate = TimeSpan.FromSeconds(intervalSec);
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
            lstPcGpu = new PerformanceCounterCategory("GPU Engine")
                        .GetInstanceNames()
                        .Where(name => name.EndsWith("engtype_3D"))
                        .Select(name => new PerformanceCounter("GPU Engine", "Utilization Percentage", name))
                        .ToList();
            ciInfo = new ComputerInfo();
            diHddC = new DriveInfo("C");

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

            if ((lstPcGpu != null) && (lstPcGpu.Count > 0))
            {
                try
                {
                    lstPcGpu.ForEach(c => c.NextValue());
                }
                catch (InvalidOperationException) { }

                Thread.Sleep(500);

                try
                {
                    gpuUsage = lstPcGpu.Sum(c => c.NextValue());
                }
                catch (InvalidOperationException) { }
            }

            return gpuUsage;
        }

        private float GetHddCUsage()
        {
            if (diHddC == null)
                return 0;
            else
                return ((float)(diHddC.TotalSize - diHddC.TotalFreeSpace) / diHddC.TotalSize) * 100;
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
