using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprend_Tissage.AppClasses;
using System.IO;
using System.Text.Json;

namespace Apprend_Tissage.Services
{
    public class ServiceLogs
    {
        private readonly List<AppLog> Logs = [];

        private string FileLogs = "logs.json";

        public ServiceLogs()
        {
            if(File.Exists(FileLogs))
            {
                string datas = File.ReadAllText(FileLogs);
                Logs = JsonSerializer.Deserialize<List<AppLog>>(datas);
            }
        }

        public void Logger(string act, string txt)
        {
            AppLog log = new()
            {
                DateLog = DateTime.Now,
                Action = act,
                Log = txt
            };

            Logs.Add(log);

            string datas = JsonSerializer.Serialize<List<AppLog>>(Logs);
            File.WriteAllText(FileLogs, datas);
        }

        internal void Definir(string fileLogs)
        {
            FileLogs = fileLogs;
        }
    }
}
