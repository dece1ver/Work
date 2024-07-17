using DocumentFormat.OpenXml.Spreadsheet;
using eLog.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace eLog.Infrastructure
{
    public class GoogleSheets
    {
        
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private static GoogleCredential Credential { get; set; } = GetCredentialsFromFile();
        public static SheetsService SheetsService { get; set; } = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = Credential,
        });

        private static GoogleCredential GetCredentialsFromFile()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(AppSettings.Instance.GoogleCredentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }
            return credential;
        }

        public static async Task<List<ProductionTaskData>> GetProductionTasksData(IProgress<string> progress)
        {
            List<ProductionTaskData> data = new List<ProductionTaskData>();
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
            });
            SpreadsheetsResource.ValuesResource.GetRequest request = SheetsService.Spreadsheets.Values.Get(AppSettings.Instance.GsId, "Загрузка станков!A1:J200");
            progress.Report("Подключение к списку...");
            ValueRange response = await request.ExecuteAsync();
            progress.Report("Формирование списка работы...");
            IList<IList<object>> values = response.Values;
            bool machineFound = false;
            int skipCnt = 0;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (row == null || row.Count == 0) continue;
                    if (skipCnt > 0)
                    {
                        skipCnt--;
                        continue;
                    }
                    if (row[0] is string currentMachine 
                        && currentMachine.Contains('|') 
                        && AppSettings.Instance.Machine.Name.Contains(currentMachine.Split('|')[0].Trim()))
                    {
                        machineFound = true;
                        skipCnt = 2;
                    }
                    else if (machineFound && row[0] is string otherMachine 
                        && otherMachine.Contains('|') 
                        && AppSettings.Machines.Select(m => m.Name).Any(name => name.Contains(otherMachine.Split('|')[0].Trim())))
                    {
                        break;
                    }
                    else if (machineFound && row.Count > 1)
                    {
                        var partName = SafeGet(row, 1);
                        var order = SafeGet(row, 2);
                        var count = SafeGet(row, 3);
                        var date = SafeGet(row, 4);
                        var plantComment = SafeGet(row, 5);
                        var priority = SafeGet(row, 6);
                        var engineerComment = SafeGet(row, 7);
                        var laborInput = SafeGet(row, 8);
                        var pdComment = SafeGet(row, 9);

                        data.Add(new ProductionTaskData(partName, order, count, date, plantComment, priority, engineerComment, laborInput, pdComment));
                    }
                }
            }
            progress.Report("Список работы сформирован.");
            return data;
        }

        public static string SafeGet(IList<object> list, int index)
        {
            return (list.Count > index && list[index] != null) ? list[index].ToString() : "";
        }
    }
}
