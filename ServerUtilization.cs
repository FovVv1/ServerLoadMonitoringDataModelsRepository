using ElMessage;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using NLog;
using Renci.SshNet;
using Renci.SshNet.Common;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SnmpSharpNet;
using System.Activities;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace ServerLoadMonitoringDataModels 
{
    public class ServerUtilization : IMetric, INotifyPropertyChanged, ICloneable
    {
        //Это временная мера и ненужное говно
        public List<ProcedureCounter> MicroservicesProceduresCount { get; set; }

        //
        public ServerUtilization()
        {
           this.RefreshingData = DateTime.Now;
           this.CpuUsage = 0;
           this.InstalledMemory = 0;
           this.UsedMemory = 0;
           this.DiskUsage = 0;
           this.DiskReads = 0;
           this.DiskWrites = 0;
           this.Placement = 0;
           this.CollectionNumber = 0;
           this.MediaType = "";
           this.Model = "";
           this.Caption = "";
           this.processorName = "";
           this.numberOfCores = "";
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
            this.UsersCount = 0;
        }
        public ServerUtilization(string ip, MetricType type, long checkInterval)
        {
            this.RefreshingData = DateTime.Now;
            Ip = ip;
            Type = type;
            this.UsersCount = 0;
            // для преобразования секунд в тики
            CheckInterval = checkInterval * TimeSpan.TicksPerSecond;
            this.CpuUsage = 0;
            this.InstalledMemory = 0;
            this.UsedMemory = 0;
            this.DiskUsage = 0;
            this.DiskReads = 0;
            this.DiskWrites = 0;
            this.Placement = 0;
            this.CollectionNumber = 0;
            this.MediaType = "";
            this.Model = "";
            this.Caption = "";
            this.processorName = "";
            this.numberOfCores = "";
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
        }
        //Номер коллекции с метриками того же IP и типа
        public int CollectionNumber { get; set; }
        //Пордяок расположения при отображении на графике
        public int Placement { get; set; }
        //Время обновления метрика
        public DateTime RefreshingData { get; set; }
        //IP метрика
        public string Ip { get; set; }

        

        //Получаемые с сервера метрики
        public int UsersCount { get; set; }
        public float GpuUsage { get; set; }
        public float CpuUsage { get; set; }
        public int Processes { get; set; }
        public float currentClockSpeed { get; set; }
        public float maxClockSpeed { get; set; }
        //
        public int Threads { get; set; }
        public TimeSpan UpTime { get; set; }
        public string processorName { get; set; }
        public string numberOfCores { get; set; }
        
        public float InstalledMemory { get; set; }

        public float UsedMemory { get; set; }
        public float UsedMemoryPercents { get; set; }
        public float DiskUsage { get; set; }
        public float DiskReads { get; set; }
        public float DiskWrites { get; set; }
        public string Caption { get; set; }
        public string Model { get; set; }
        public string MediaType { get; set; }
        
        public MetricType Type { get; set; }
        public long CheckInterval { get; set; }
        public long LastCheckTime { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        static WSManConnectionInfo CreateConnectionInfo(string serverIP, string username, string password)
        {
            PSCredential credential = new PSCredential(username, ConvertToSecureString(password));

            return new WSManConnectionInfo(new Uri($"http://{serverIP}:5985/wsman"))
            {
                Credential = credential,
                AuthenticationMechanism = AuthenticationMechanism.Basic
            };
        }

        static SecureString ConvertToSecureString(string password)
        {
            SecureString secureString = new SecureString();
            foreach (char c in password)
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }
        public void GetMMetric()
        {
            try
            {

                // Создание PerformanceCounter для счетчика процессора
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                // Чтение процента загрузки процессора
                float сcpuUsage = cpuCounter.NextValue();

                // Ожидание некоторое время для получения актуальных данных
                System.Threading.Thread.Sleep(1000);

                // Чтение процента загрузки процессора повторно
                сcpuUsage = cpuCounter.NextValue();

                string computerName = "DESKTOP-03F2G22"; // Замените на имя или IP адрес компьютера А

                ConnectionOptions connectionOptions = new ConnectionOptions();
                connectionOptions.Username = "lenovo";
                connectionOptions.Password = "Artem_10112017";

                //ManagementScope scope = new ManagementScope("\\\\" + computerName + "\\root\\cimv2", connectionOptions);
                //scope.Connect();

                using (var searcher = new ManagementObjectSearcher("SELECT LoadPercentage, Name, NumberOfCores, maxClockSpeed, CurrentClockSpeed FROM Win32_Processor"))
                //using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT LoadPercentage, Name, NumberOfCores, MaxClockSpeed, CurrentClockSpeed FROM Win32_Processor")))
                {
                    foreach (var obj in searcher.Get())
                    {
                        try
                        {
                            this.CpuUsage = obj["LoadPercentage"] != null ? float.Parse(obj["LoadPercentage"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.CpuUsage = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении CpuUsage: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.maxClockSpeed = obj["maxClockSpeed"] != null ? float.Parse(obj["maxClockSpeed"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.maxClockSpeed = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении CpuUsage: " + e.ToString().Replace("\r\n", ""));
                        }
                        try
                        {
                            this.processorName = obj["Name"] != null ? obj["Name"].ToString() : "";
                        }
                        catch (Exception e)
                        {
                            this.processorName = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении processorName: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.numberOfCores = obj["NumberOfCores"] != null ? obj["NumberOfCores"].ToString() : "";
                        }
                        catch (Exception e)
                        {
                            this.numberOfCores = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении numberOfCores: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.currentClockSpeed = obj["CurrentClockSpeed"] != null ? float.Parse(obj["CurrentClockSpeed"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.currentClockSpeed = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении currentClockSpeed: " + e.ToString().Replace("\r\n", ""));
                        }

                        break;
                    }
                }

                // Запрос для получения информации о системе
                using (var osSearcher = new ManagementObjectSearcher("SELECT LastBootUpTime, NumberOfProcesses, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (var osObj in osSearcher.Get())
                    {
                        try
                        {
                            var lastBootUpTime = ManagementDateTimeConverter.ToDateTime(osObj["LastBootUpTime"].ToString());
                            this.UpTime = DateTime.Now - lastBootUpTime;
                        }
                        catch (Exception e)
                        {
                            this.UpTime = TimeSpan.Zero;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UpTime: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Processes = osObj["NumberOfProcesses"] != null ? Convert.ToInt32(osObj["NumberOfProcesses"]) : 0;
                        }
                        catch (Exception e)
                        {
                            this.Processes = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Processes: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Threads = osObj["NumberOfThreads"] != null ? Convert.ToInt32(osObj["NumberOfThreads"]) : 0;
                        }
                        catch (Exception e)
                        {
                            this.Threads = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Threads: " + e.ToString().Replace("\r\n", ""));
                        }


                        try
                        {
                            this.InstalledMemory = osObj["TotalVisibleMemorySize"] != null ? float.Parse(osObj["TotalVisibleMemorySize"].ToString()) / (1024 * 1024) : 0;
                        }
                        catch (Exception e)
                        {
                            this.InstalledMemory = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении InstalledMemory: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.UsedMemory = (osObj["TotalVisibleMemorySize"] != null && osObj["FreePhysicalMemory"] != null) ?
                                (float.Parse(osObj["TotalVisibleMemorySize"].ToString()) - float.Parse(osObj["FreePhysicalMemory"].ToString())) / (1024 * 1024) : 0;
                        }
                        catch (Exception e)
                        {
                            this.UsedMemory = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UsedMemory: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.UsedMemoryPercents = (float)(Math.Round((UsedMemory * 100) / (InstalledMemory > 0 ? InstalledMemory : 1)));
                        }
                        catch (Exception e)
                        {
                            this.UsedMemoryPercents = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UsedMemoryPercents: " + e.ToString().Replace("\r\n", ""));
                        }

                        break; // Assuming there is only one result for the operating system
                    }
                }
                // Запрос для получения информации о диске
                using (var diskSearcher = new ManagementObjectSearcher(
                           "SELECT DiskReadsPerSec, DiskWritesPerSec, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk WHERE Name='_Total'"))
                {
                    foreach (var diskObj in diskSearcher.Get())
                    {
                        try
                        {
                            this.DiskReads = diskObj["DiskReadsPerSec"] != null
                                ? float.Parse(diskObj["DiskReadsPerSec"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskReads = 0;
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении DiskReads: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.DiskWrites = diskObj["DiskWritesPerSec"] != null
                                ? float.Parse(diskObj["DiskWritesPerSec"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskWrites = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении DiskWrites: " +
                                                                     e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.DiskUsage = diskObj["PercentDiskTime"] != null
                                ? float.Parse(diskObj["PercentDiskTime"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskUsage = 0;
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении DiskUsage: " + e.ToString().Replace("\r\n", ""));
                        }

                        break; // Assuming there is only one result for _Total
                    }
                }
                using (var diskSearcher = new ManagementObjectSearcher("SELECT Caption, Model, MediaType FROM Win32_DiskDrive"))
                {
                    foreach (var diskObj in diskSearcher.Get())
                    {
                        try
                        {
                            this.Caption = diskObj["Caption"] != null
                                ? diskObj["Caption"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.Caption = "";
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении Caption: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Model = diskObj["Model"] != null
                                ? diskObj["Model"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.Model = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Model: " +
                                                                     e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.MediaType = diskObj["MediaType"] != null
                                ? diskObj["MediaType"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.MediaType = "";
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении MediaType: " + e.ToString().Replace("\r\n", ""));
                        }

                        // Можете убрать break, если хотите обработать все результаты, а не только первый.
                        break;
                    }
                }
                using (var gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine WHERE Name='_Total'"))
                {
                    foreach (ManagementObject queryObj in gpuSearcher.Get())
                    {
                        string gpu = queryObj["PercentProcessorTime"].ToString();
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));

            }
        }
        public void GetMetric(ElConnectionClient elConnectionClient, ElMessageServer elMessageServer)
        {
            try
            {

                UsersCount = elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections.Count;

                // Создание PerformanceCounter для счетчика процессора
                PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                // Чтение процента загрузки процессора
                float сcpuUsage = cpuCounter.NextValue();

                // Ожидание некоторое время для получения актуальных данных
                System.Threading.Thread.Sleep(1000);

                // Чтение процента загрузки процессора повторно
                сcpuUsage = cpuCounter.NextValue();

                string computerName = "DESKTOP-03F2G22"; // Замените на имя или IP адрес компьютера А

                ConnectionOptions connectionOptions = new ConnectionOptions();
                connectionOptions.Username = "lenovo";
                connectionOptions.Password = "Artem_10112017";

                //ManagementScope scope = new ManagementScope("\\\\" + computerName + "\\root\\cimv2", connectionOptions);
                //scope.Connect();

                using (var searcher = new ManagementObjectSearcher("SELECT LoadPercentage, Name, NumberOfCores, maxClockSpeed, CurrentClockSpeed FROM Win32_Processor"))
                //using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT LoadPercentage, Name, NumberOfCores, MaxClockSpeed, CurrentClockSpeed FROM Win32_Processor")))
                {
                    foreach (var obj in searcher.Get())
                    {
                        try
                        {
                            this.CpuUsage = obj["LoadPercentage"] != null ? float.Parse(obj["LoadPercentage"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.CpuUsage = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении CpuUsage: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.maxClockSpeed = obj["maxClockSpeed"] != null ? float.Parse(obj["maxClockSpeed"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.maxClockSpeed = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении CpuUsage: " + e.ToString().Replace("\r\n", ""));
                        }
                        try
                        {
                            this.processorName = obj["Name"] != null ? obj["Name"].ToString() : "";
                        }
                        catch (Exception e)
                        {
                            this.processorName = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении processorName: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.numberOfCores = obj["NumberOfCores"] != null ? obj["NumberOfCores"].ToString() : "";
                        }
                        catch (Exception e)
                        {
                            this.numberOfCores = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении numberOfCores: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.currentClockSpeed = obj["CurrentClockSpeed"] != null ? float.Parse(obj["CurrentClockSpeed"].ToString()) : 0;
                        }
                        catch (Exception e)
                        {
                            this.currentClockSpeed = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении currentClockSpeed: " + e.ToString().Replace("\r\n", ""));
                        }

                        break;
                    }
                }

                // Запрос для получения информации о системе
                using (var osSearcher = new ManagementObjectSearcher("SELECT LastBootUpTime, NumberOfProcesses, TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (var osObj in osSearcher.Get())
                    {
                        try
                        {
                            var lastBootUpTime = ManagementDateTimeConverter.ToDateTime(osObj["LastBootUpTime"].ToString());
                            this.UpTime = DateTime.Now - lastBootUpTime;
                        }
                        catch (Exception e)
                        {
                            this.UpTime = TimeSpan.Zero;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UpTime: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Processes = osObj["NumberOfProcesses"] != null ? Convert.ToInt32(osObj["NumberOfProcesses"]) : 0;
                        }
                        catch (Exception e)
                        {
                            this.Processes = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Processes: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Threads = osObj["NumberOfThreads"] != null ? Convert.ToInt32(osObj["NumberOfThreads"]) : 0;
                        }
                        catch (Exception e)
                        {
                            this.Threads = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Threads: " + e.ToString().Replace("\r\n", ""));
                        }


                        try
                        {
                            this.InstalledMemory = osObj["TotalVisibleMemorySize"] != null ? float.Parse(osObj["TotalVisibleMemorySize"].ToString()) / (1024 * 1024) : 0;
                        }
                        catch (Exception e)
                        {
                            this.InstalledMemory = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении InstalledMemory: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.UsedMemory = (osObj["TotalVisibleMemorySize"] != null && osObj["FreePhysicalMemory"] != null) ?
                                (float.Parse(osObj["TotalVisibleMemorySize"].ToString()) - float.Parse(osObj["FreePhysicalMemory"].ToString())) / (1024 * 1024) : 0;
                        }
                        catch (Exception e)
                        {
                            this.UsedMemory = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UsedMemory: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.UsedMemoryPercents = (float)(Math.Round((UsedMemory * 100) / (InstalledMemory > 0 ? InstalledMemory : 1)));
                        }
                        catch (Exception e)
                        {
                            this.UsedMemoryPercents = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении UsedMemoryPercents: " + e.ToString().Replace("\r\n", ""));
                        }

                        break; // Assuming there is only one result for the operating system
                    }
                }
                // Запрос для получения информации о диске
                using (var diskSearcher = new ManagementObjectSearcher(
                           "SELECT DiskReadsPerSec, DiskWritesPerSec, PercentDiskTime FROM Win32_PerfFormattedData_PerfDisk_PhysicalDisk WHERE Name='_Total'"))
                {
                    foreach (var diskObj in diskSearcher.Get())
                    {
                        try
                        {
                            this.DiskReads = diskObj["DiskReadsPerSec"] != null
                                ? float.Parse(diskObj["DiskReadsPerSec"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskReads = 0;
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении DiskReads: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.DiskWrites = diskObj["DiskWritesPerSec"] != null
                                ? float.Parse(diskObj["DiskWritesPerSec"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskWrites = 0;
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении DiskWrites: " +
                                                                     e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.DiskUsage = diskObj["PercentDiskTime"] != null
                                ? float.Parse(diskObj["PercentDiskTime"].ToString())
                                : 0;
                        }
                        catch (Exception e)
                        {
                            this.DiskUsage = 0;
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении DiskUsage: " + e.ToString().Replace("\r\n", ""));
                        }

                        break; // Assuming there is only one result for _Total
                    }
                }
                using (var diskSearcher = new ManagementObjectSearcher("SELECT Caption, Model, MediaType FROM Win32_DiskDrive"))
                {
                    foreach (var diskObj in diskSearcher.Get())
                    {
                        try
                        {
                            this.Caption = diskObj["Caption"] != null
                                ? diskObj["Caption"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.Caption = "";
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении Caption: " + e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.Model = diskObj["Model"] != null
                                ? diskObj["Model"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.Model = "";
                            LogManager.GetCurrentClassLogger().Error("Ошибка при получении Model: " +
                                                                     e.ToString().Replace("\r\n", ""));
                        }

                        try
                        {
                            this.MediaType = diskObj["MediaType"] != null
                                ? diskObj["MediaType"].ToString()
                                : "";
                        }
                        catch (Exception e)
                        {
                            this.MediaType = "";
                            LogManager.GetCurrentClassLogger()
                                .Error("Ошибка при получении MediaType: " + e.ToString().Replace("\r\n", ""));
                        }

                        // Можете убрать break, если хотите обработать все результаты, а не только первый.
                        break;
                    }
                }
                using (var gpuSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_GPUPerformanceCounters_GPUEngine WHERE Name='_Total'"))
                {
                    foreach (ManagementObject queryObj in gpuSearcher.Get())
                    {
                        string gpu = queryObj["PercentProcessorTime"].ToString();
                    }
                }
                }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));

            }
        }
        public void GetMetric3(ElConnectionClient elConnectionClient, ElMessageServer elMessageServer)
        {
            try
            {

                UsersCount = elConnectionClient.ServerControlManager.ConnectionsControl.ActiveConnections.Count;
                // Получение использования процессора
                using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpuCounter.NextValue(); // Начальное значение, требуется для корректного чтения
                    System.Threading.Thread.Sleep(1000); // Ожидание для получения актуальных данных
                    this.CpuUsage = cpuCounter.NextValue();
                }

                // Получение метрик диска
                using (PerformanceCounter diskReadCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total"))
                {
                    this.DiskReads = diskReadCounter.NextValue();
                }
                using (PerformanceCounter diskWriteCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total"))
                {
                    this.DiskWrites = diskWriteCounter.NextValue();
                }
               
                using (PerformanceCounter diskUsageCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"))
                {
                    this.DiskUsage = diskUsageCounter.NextValue();
                }
                

                // Получение информации о системе
                this.UpTime = TimeSpan.FromSeconds(Environment.TickCount / 1000);
              
                this.Processes = Process.GetProcesses().Length;
                
                int threadCount = 0;
                foreach (Process process in Process.GetProcesses())
                {
                    threadCount += process.Threads.Count;
                }
                this.Threads = threadCount;

                
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));

            }
        }
        public object Clone()
        {
            ServerUtilization clone = new ServerUtilization
            {
                RefreshingData = this.RefreshingData,
                Ip = this.Ip,
                GpuUsage = this.GpuUsage,
                CpuUsage = this.CpuUsage,
                Processes = this.Processes,
                Threads = this.Threads,
                UpTime = this.UpTime,
                processorName = this.processorName,
                numberOfCores = this.numberOfCores,
                maxClockSpeed = this.maxClockSpeed,
                currentClockSpeed = this.currentClockSpeed,
                InstalledMemory = this.InstalledMemory,
                UsedMemory = this.UsedMemory,
                UsedMemoryPercents = this.UsedMemoryPercents,
                DiskUsage = this.DiskUsage,
                DiskReads = this.DiskReads,
                DiskWrites = this.DiskWrites,
                Caption = this.Caption,
                Model = this.Model,
                MediaType = this.MediaType,
                Type = this.Type,
                CheckInterval = this.CheckInterval,
                LastCheckTime = this.LastCheckTime,
                Placement = this.Placement,
                CollectionNumber = this.CollectionNumber,
                UsersCount = this.UsersCount

            };

            return clone;
        }
    }
}
