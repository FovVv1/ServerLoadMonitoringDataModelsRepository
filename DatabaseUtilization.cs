﻿using Dapper;
using ElMessage;
using Newtonsoft.Json;
using NLog;
using ServerLoadMonitoringDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServerLoadMonitoringDataModels
{
    public class DatabaseUtilization : IMetric, ICloneable
    {
        // Временно
        public float CpuUsage { get; set; }

        public float UsedMemoryPercents { get; set; }
        //
        public DatabaseUtilization()
        {
            this.UsedMemoryPercents = 0;
            this.CpuUsage = new float();
            this.RefreshingData = DateTime.Now;
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
            this.StoredProcedures = new List<string>();
            this.Placement = 0;
            this.CollectionNumber = 0;
            this.UsersCount = 0;
        }

        public DatabaseUtilization(string ip, MetricType type, long checkInterval)
        {
            this.UsedMemoryPercents = 0;
            this.CpuUsage = new float();
            this.RefreshingData = DateTime.Now;
            this.Ip = ip;
            this.Type = type;
            this.MicroservicesProceduresCount = new List<ProcedureCounter>();
            // для преобразования секунд в тики
            this.CheckInterval = checkInterval * TimeSpan.TicksPerSecond;
            this.Placement = 0;
            this.CollectionNumber = 0;
            this.UsersCount = 0;
        }
        public int UsersCount { get; set; }
        public int CollectionNumber {  get; set; }
        public int Placement { get; set; }
        public DateTime RefreshingData { get; set; }
        public string Ip { get; set; }
        public MetricType Type { get; set; }
        public long CheckInterval { get; set; }
        public long LastCheckTime { get; set; }
        public int TotalDatabaseSizeBytes { get; set; }
        public int UsedSpaceBytes { get; set; }
        public int FreeSpaceBytes { get; set; }
        public int TransactionLogSizeBytes { get; set; }
        public int UsedTransactionLogSpaceBytes { get; set; }
        public int FreeTransactionLogSpaceBytes { get; set; }
        public int NumberOfStoredProcedures { get; set; }
        public List<string> StoredProcedures { get; set; }
        
        public List<ProcedureCounter> MicroservicesProceduresCount { get; set; }

        public DateTime LastBackupTime { get; set; }

        public void GetMMetric(ElConnectionClient elConnectionClient)
        {
            try
            {
                
                using (SqlConnection db = new SqlConnection(elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString))
                {
                    db.Open();

                    // Получение списка хранимых процедур
                    var storedProcedures = db.Query<string>("ServerLoadMonitoring_GetAllStoredProcedures", commandType: CommandType.StoredProcedure).ToList();
                    this.StoredProcedures = storedProcedures;
                    // Создание нового списка ProcedureCounter
                    var procedureCounters = new List<ProcedureCounter>();

                    // Получение данных
                    var data = db.QueryFirstOrDefault("ServerLoadMonitoring_GetDatabaseUtilization", commandType: CommandType.StoredProcedure);

                    if (data != null)
                    {
                        // ... (оставьте остальной код без изменений)

                        // Анализ названий хранимых процедур и добавление объектов ProcedureCounter в список
                        int totalMicroservices = storedProcedures.Select(p => p.Split('_')[0]).Distinct().Count();
                        int angleStep = 360 / totalMicroservices;
                        int currentAngle = 0;

                        foreach (var procedureName in storedProcedures)
                        {
                            // Разделение названия хранимой процедуры по символу '_'
                            var parts = procedureName.Split('_');

                            if (parts.Length >= 2)
                            {
                                var microserviceName = parts[0];

                                // Проверка наличия микросервиса в списке и увеличение счетчика
                                var existingCounter = procedureCounters.FirstOrDefault(pc => pc.MicroserviceName == microserviceName);
                                if (existingCounter != null)
                                {
                                    existingCounter.ProcedureCount++;
                                }
                                else
                                {
                                    // Если микросервиса нет в списке, добавляем новый объект ProcedureCounter
                                    procedureCounters.Add(new ProcedureCounter
                                    {
                                        MicroserviceName = microserviceName,
                                        ProcedureCount = 1,
                                        ChartAngle = currentAngle
                                    });

                                    currentAngle += angleStep;
                                }
                            }
                        }

                        // Присваиваем полученный список объектов ProcedureCounter свойству MicroservicesProcedureCounters
                        MicroservicesProceduresCount.Clear();
                        foreach (var counter in procedureCounters)
                        {
                            MicroservicesProceduresCount.Add(counter);
                        }

                        if (data.TotalDatabaseSizeBytes != null)
                        {
                            this.TotalDatabaseSizeBytes = int.Parse(data.TotalDatabaseSizeBytes.ToString());
                        }
                        if (data.FreeSpaceBytes != null)
                        {
                            this.FreeSpaceBytes = int.Parse(data.FreeSpaceBytes.ToString());
                        }
                        if (data.UsedSpaceBytes != null)
                        {
                            this.UsedSpaceBytes = int.Parse(data.UsedSpaceBytes.ToString());
                        }
                        if (data.TransactionLogSizeBytes != null)
                        {
                            this.TransactionLogSizeBytes = int.Parse(data.TransactionLogSizeBytes.ToString());
                        }
                        if (data.UsedTransactionLogSpaceBytes != null)
                        {
                            this.UsedTransactionLogSpaceBytes = int.Parse(data.UsedTransactionLogSpaceBytes.ToString());
                        }
                        if (data.FreeTransactionLogSpaceBytes != null)
                        {
                            this.FreeTransactionLogSpaceBytes = int.Parse(data.FreeTransactionLogSpaceBytes.ToString());
                        }
                        if (data.NumberOfStoredProcedures != null)
                        {
                            this.NumberOfStoredProcedures = int.Parse(data.NumberOfStoredProcedures.ToString());
                        }
                        if (data.LastBackupTime != null)
                        {
                            this.LastBackupTime = DateTime.Parse(data.LastBackupTime.ToString());
                        }
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
                using (SqlConnection db = new SqlConnection(elConnectionClient.ServerControlManager.DataBaseControl.DbConnectionString))
                {
                    db.Open();

                    // Получение списка хранимых процедур
                    var storedProcedures = db.Query<string>("GetAllStoredProcedures", commandType: CommandType.StoredProcedure).ToList();
                    this.StoredProcedures = storedProcedures;
                    // Создание нового списка ProcedureCounter
                    var procedureCounters = new List<ProcedureCounter>();

                    // Получение данных
                    var data = db.QueryFirstOrDefault("ServerLoadMonitoring_GetDatabaseUtilization", commandType: CommandType.StoredProcedure);

                    if (data != null)
                    {
                        // ... (оставьте остальной код без изменений)

                        // Анализ названий хранимых процедур и добавление объектов ProcedureCounter в список
                        int totalMicroservices = storedProcedures.Select(p => p.Split('_')[0]).Distinct().Count();
                        int angleStep = 360 / totalMicroservices;
                        int currentAngle = 0;

                        foreach (var procedureName in storedProcedures)
                        {
                            // Разделение названия хранимой процедуры по символу '_'
                            var parts = procedureName.Split('_');

                            if (parts.Length >= 2)
                            {
                                var microserviceName = parts[0];

                                // Проверка наличия микросервиса в списке и увеличение счетчика
                                var existingCounter = procedureCounters.FirstOrDefault(pc => pc.MicroserviceName == microserviceName);
                                if (existingCounter != null)
                                {
                                    existingCounter.ProcedureCount++;
                                }
                                else
                                {
                                    // Если микросервиса нет в списке, добавляем новый объект ProcedureCounter
                                    procedureCounters.Add(new ProcedureCounter
                                    {
                                        MicroserviceName = microserviceName,
                                        ProcedureCount = 1,
                                        ChartAngle = currentAngle
                                    });

                                    currentAngle += angleStep;
                                }
                            }
                        }

                        // Присваиваем полученный список объектов ProcedureCounter свойству MicroservicesProcedureCounters
                        MicroservicesProceduresCount.Clear();
                        foreach (var counter in procedureCounters)
                        {
                            MicroservicesProceduresCount.Add(counter);
                        }

                        if (data.TotalDatabaseSizeBytes != null)
                        {
                            this.TotalDatabaseSizeBytes = int.Parse(data.TotalDatabaseSizeBytes.ToString());
                        }
                        if (data.FreeSpaceBytes != null)
                        {
                            this.FreeSpaceBytes = int.Parse(data.FreeSpaceBytes.ToString());
                        }
                        if (data.UsedSpaceBytes != null)
                        {
                            this.UsedSpaceBytes = int.Parse(data.UsedSpaceBytes.ToString());
                        }
                        if (data.TransactionLogSizeBytes != null)
                        {
                            this.TransactionLogSizeBytes = int.Parse(data.TransactionLogSizeBytes.ToString());
                        }
                        if (data.UsedTransactionLogSpaceBytes != null)
                        {
                            this.UsedTransactionLogSpaceBytes = int.Parse(data.UsedTransactionLogSpaceBytes.ToString());
                        }
                        if (data.FreeTransactionLogSpaceBytes != null)
                        {
                            this.FreeTransactionLogSpaceBytes = int.Parse(data.FreeTransactionLogSpaceBytes.ToString());
                        }
                        if (data.NumberOfStoredProcedures != null)
                        {
                            this.NumberOfStoredProcedures = int.Parse(data.NumberOfStoredProcedures.ToString());
                        }
                        if (data.LastBackupTime != null)
                        {
                            this.LastBackupTime = DateTime.Parse(data.LastBackupTime.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error(e.ToString().Replace("\r\n", ""));
            }
        }

        private bool ProcedureExists(SqlConnection connection, string procedureName)
        {
            using (SqlCommand command = new SqlCommand($"SELECT 1 FROM sys.objects WHERE type = 'P' AND name = '{procedureName}'", connection))
            {
                return command.ExecuteScalar() != null;
            }
        }
        public object Clone()
        {
            return new DatabaseUtilization
            {
                RefreshingData = this.RefreshingData,
                Ip = this.Ip,
                Type = this.Type,
                CheckInterval = this.CheckInterval,
                LastCheckTime = this.LastCheckTime,
                TotalDatabaseSizeBytes = this.TotalDatabaseSizeBytes,
                UsedSpaceBytes = this.UsedSpaceBytes,
                FreeSpaceBytes = this.FreeSpaceBytes,
                TransactionLogSizeBytes = this.TransactionLogSizeBytes,
                UsedTransactionLogSpaceBytes = this.UsedTransactionLogSpaceBytes,
                FreeTransactionLogSpaceBytes = this.FreeTransactionLogSpaceBytes,
                NumberOfStoredProcedures = this.NumberOfStoredProcedures,
                StoredProcedures = new List<string>(this.StoredProcedures),
                MicroservicesProceduresCount = new List<ProcedureCounter>(this.MicroservicesProceduresCount),
                LastBackupTime = this.LastBackupTime,
                Placement = this.Placement,
                CollectionNumber = this.CollectionNumber,
                UsersCount = this.UsersCount
            };
        }
    }
}

