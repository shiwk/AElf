﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Configuration;
using InfluxDB.Net;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Models;

namespace AElf.Management.Helper
{
    public class InfluxDBHelper
    {

        private static readonly InfluxDb _client;

        static InfluxDBHelper()
        {
            _client = new InfluxDb(MonitorDatabaseConfig.Instance.Url, MonitorDatabaseConfig.Instance.Username, MonitorDatabaseConfig.Instance.Password);
        }

        public static void Set(string database, string measurement, Dictionary<string, object> fields, Dictionary<string, object> tags, DateTime timestamp)
        {
            var point = new Point
            {
                Measurement = measurement,
                Fields = fields,
                Timestamp = timestamp
            };
            if (tags != null)
            {
                point.Tags = tags;
            }
            _client.WriteAsync(database, point);
        }

        public static List<Serie> Get(string database, string query)
        {
            var series = _client.QueryAsync(database, query).Result;

            return series;
        }
        
        public static string Version()
        {
            var version = _client.GetClientVersion();
            return version.ToString();
        }

        public static void CreateDatabase(string database)
        {
            _client.CreateDatabaseAsync(database);
        }

        public static void DropDatabase(string database)
        {
            _client.DropDatabaseAsync(database);
        }
    }
}