using Dapper;
using ElMessage;
using Newtonsoft.Json;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using Exception = System.Exception;
using ServerLoadMonitoringServer.Helper;

namespace ServerLoadMonitoringDataModels
{
    public class StorageUtilization : IMetric, ICloneable
    {
        //Временная мера  и ненужное 

        public List<ProcedureCounter> MicroservicesProceduresCount { get; set; }

        //

        public DateTime RefreshingData { get; set; }
        /// <summary>
        /// Список агентов с состоянием выполнения работ
        /// </summary>
        public StorageUtilization()
        {
            this.Placement = 0;
            this.CollectionNumber = 0;
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
            this.UsersCount = 0;
        }
        public StorageUtilization(string ip, MetricType type, long checkInterval)
        {
            this.Ip = ip;
            this.Type = type;
            // для преобразования секунд в тики
            this.CheckInterval = checkInterval * TimeSpan.TicksPerSecond;
            this.Placement = 0;
            this.CollectionNumber = 0;
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
            this.UsersCount = 0;
        }

        public int UsersCount { get; set; }
        public int CollectionNumber { get; set; }
        public int Placement { get; set; }
        public string Ip { get; set; }
        public MetricType Type { get; set; }

        public long CheckInterval { get; set; }

        public long LastCheckTime { get; set; }
        
        public BackupMonitorData Data { get; set; }
        public  void GetMetric(ElConnectionClient elConnectionClient, ElMessageServer elMessageServer)
        {
            try
            { //получить путь
                //var description = new { ReportKey = ESettingsKeys.Settings };
                //var parameters = JsonConvert.DeserializeAnonymousType(elMessageServer.Data, description);


                using (var db =
                    new SqlConnection($"Data Source={this.Ip};{elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString}"))
                {
                    db.Open();
                    //create key for loading current
                    var reportKey = $"{ESettingsKeys.Settings}";

                    //get report from table or return clean object
                    var settings = db.Query<PluginSettings>("ServerLoadMonitoring_GetPluginSettings",
                        new { PluginName = elMessageServer.PluginName },
                        commandType: CommandType.StoredProcedure).ToList();

                    //var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(settings);

                    if (settings != null && settings.Count(i => i.SettingName == "BackupMasterPath") > 0)
                    {
                        var backupPath = settings.FirstOrDefault(i => i.SettingName == "BackupMasterPath")?.StringValue;

                        ////Доступ под указанным пользователем
                        using (NetworkShareAccesser.Access(backupPath, AuthCredentials.Login, AuthCredentials.Password))
                        {
                            var configsPath = backupPath + "\\el_backup_master\\config\\";


                            var configsDir = new DirectoryInfo(configsPath);
                            var configFiles = configsDir.GetFiles("*.config");
                            var configs = new List<AgentConfig>();

                            //get all configs
                            foreach (var configFile in configFiles)
                            {
                                var agentConfig = new AgentConfig();
                                var res = agentConfig.LoadToXml(configFile.FullName);
                                if (res)
                                {
                                    //agent's name taking from filename
                                    agentConfig.Name = configFile.Name.Split('.')[0];
                                    configs.Add(agentConfig);
                                }
                                else
                                {
                                    LogManager.GetCurrentClassLogger()
                                        .Warn("Невозможнос считать конфигурацию агента: " + configFile.FullName);
                                }
                            }

                            //get all jobs for all agents
                            var agentsDataPath = backupPath + "\\el_backup_master\\agent_data\\";

                            foreach (var agentsConfig in configs)
                            {
                                if (agentsConfig.Jobs != null)
                                {
                                    foreach (var job in agentsConfig.Jobs)
                                    {
                                        //path forming with name of day of week
                                        var jobPath = agentsDataPath + agentsConfig.Name + "\\" + job.Name + "\\" +
                                                      EnumConverter.GetDescription((EWeek)DateTime.Now.DayOfWeek) +
                                                      "\\";


                                        var jobsDirToday = new DirectoryInfo(jobPath);

                                        if (jobsDirToday.Exists)
                                        {
                                            var logPath = jobPath + "log.txt";
                                            if (File.Exists(logPath) && File.GetLastWriteTime(logPath).Date == DateTime.Today)
                                            {
                                                var logText = "";
                                                using (var fs = File.Open(logPath, FileMode.Open))
                                                {
                                                    byte[] buffer = new byte[fs.Length];
                                                    fs.Read(buffer, 0, (int)fs.Length);
                                                    logText = Encoding.UTF8.GetString(buffer);
                                                }

                                                var reg = new Regex("\\n");
                                                var logStrings = reg.Split(logText);
                                                for (int i = logStrings.Length; i > 0; i--)
                                                {
                                                    //get all key message from file + time mark (new collect messages, if keyword - add)

                                                    //get last time mark - ok - status ok
                                                    //error - error
                                                    //logic for stopped will be later
                                                    //stop - stopped
                                                    if (logStrings[i - 1].Contains(JobStatusMessages.ErrorMessage))
                                                    {
                                                        job.Status = EJobStatus.Error;
                                                        break;
                                                    }

                                                    if (logStrings[i - 1].Contains(JobStatusMessages.OKMessage))
                                                    {
                                                        job.Status = EJobStatus.OK;
                                                        break;
                                                    }

                                                    if (logStrings[i - 1].Contains(JobStatusMessages.StopMessage))
                                                    {
                                                        job.Status = EJobStatus.Stopped;
                                                        break;
                                                    }

                                                    if (logStrings[i - 1].Contains(JobStatusMessages.StartMessage))
                                                    {
                                                        job.Status = EJobStatus.Started;
                                                        break;
                                                    }
                                                }

                                                if (logStrings.Length > 0)
                                                {
                                                    job.LastMessage = logStrings.Last();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //waiting - today's folder isn't exists at all
                                            job.Status = EJobStatus.Waiting;
                                        }

                                        //
                                        var jobTmpPath = agentsDataPath + agentsConfig.Name + "\\" + job.Name + "\\~" +
                                                         EnumConverter.GetDescription((EWeek)DateTime.Now.DayOfWeek) +
                                                         "\\";

                                        var jobTmpDirToday = new DirectoryInfo(jobTmpPath);
                                        if (jobTmpDirToday.Exists && File.GetLastWriteTime(jobTmpPath).Date == DateTime.Today)
                                        {
                                            var logPath = jobPath + "log.txt";
                                            if (File.Exists(logPath))
                                            {
                                                var logText = "";
                                                using (var fs = File.Open(logPath, FileMode.Open))
                                                {
                                                    byte[] buffer = new byte[fs.Length];
                                                    fs.Read(buffer, 0, (int)fs.Length);
                                                    logText = Encoding.UTF8.GetString(buffer);
                                                }

                                                var reg = new Regex("\\n");
                                                var logStrings = reg.Split(logText);
                                                for (int i = logStrings.Length; i > 0; i--)
                                                {
                                                    //get all key message from file + time mark (new collect messages, if keyword - add)

                                                    //get last time mark - ok - status ok
                                                    //error - error
                                                    //logic for stopped will be later
                                                    //stop - stopped
                                                    if (logStrings[i - 1].Contains(JobStatusMessages.ErrorMessage))
                                                    {
                                                        job.Status = EJobStatus.Error;
                                                        break;
                                                    }

                                                    if (logStrings[i - 1].Contains(JobStatusMessages.StopMessage))
                                                    {
                                                        job.Status = EJobStatus.Stopped;
                                                        break;
                                                    }

                                                    if (logStrings[i - 1].Contains(JobStatusMessages.StartMessage))
                                                    {
                                                        job.Status = EJobStatus.Started;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //формируем и сохраняем в настройки коллекцию агентов с текущими статусами работ


                            var fullData = new BackupMonitorData();
                            fullData.RefreshingData = DateTime.Now;
                            fullData.AgentsConfigs = configs;

                            var settingsKey = $"CurrentJobsStatus";

                            db.Query("ServerLoadMonitoring_SaveReportData",
                                new
                                {
                                    Plugin = elMessageServer.PluginName,
                                    ReportKey = settingsKey,
                                    Data = JsonConvert.SerializeObject(fullData)
                                }, commandType: CommandType.StoredProcedure);

                            this.Data = fullData;
                        }
                    }
                    //return JsonConvert.SerializeObject(new { result = false });
                }
            }
            catch (Exception e)
            {
                //При возникновении ошибки возвращаем сериализованый объект Exception
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
                //return JsonConvert.SerializeObject(e);
            }
        }
        public object Clone()
        {
            StorageUtilization clone = new StorageUtilization
            {
                RefreshingData = this.RefreshingData,
                Ip = this.Ip,
                Type = this.Type,
                CheckInterval = this.CheckInterval,
                LastCheckTime = this.LastCheckTime,
                Data = (this.Data != null) ? JsonConvert.DeserializeObject<BackupMonitorData>(JsonConvert.SerializeObject(this.Data)) : null,
                Placement = this.Placement,
                CollectionNumber = this.CollectionNumber,
                UsersCount = this.UsersCount
            };

            return clone;
        }

    }

}

