using eLog.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using libeLog.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task<IReadOnlyList<ProductionTaskData>> GetProductionTasksData(IProgress<string> progress, CancellationToken cancellationToken)
        {
            List<ProductionTaskData> data = new List<ProductionTaskData>();
            Credential = GetCredentialsFromFile();
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
            });
            SpreadsheetsResource.ValuesResource.GetRequest request = SheetsService.Spreadsheets.Values.Get(AppSettings.Instance.GsId, "Загрузка станков!A1:J200");
            progress.Report("Подключение к списку...");
            ValueRange response = await request.ExecuteAsync(cancellationToken);
            progress.Report("Формирование списка работы...");
            IList<IList<object>> values = response.Values;
            bool machineFound = false;
            int skipCnt = 0;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    cancellationToken.ThrowIfCancellationRequested();
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
                        && (otherMachine.ToLower() == "маркировка" || otherMachine.Contains('|') 
                        && AppSettings.Machines.Select(m => m.Name).Any(name => name.Contains(otherMachine.Split('|')[0].Trim()))))
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

                        if (!string.IsNullOrEmpty(partName))
                        {
                            data.Add(new ProductionTaskData(
                                partName,
                                order.IfEmpty("Без М/Л", o => o.ToUpper()),
                                count.IfEmpty("?"),
                                date.IfEmpty("-"),
                                plantComment.IfEmpty("-"),
                                priority,
                                engineerComment.IfEmpty("-"),
                                laborInput.IfEmpty("-"),
                                pdComment.IfEmpty("-")));
                        }
                    }
                }
                progress.Report("Список работы сформирован.");

            }

            var sortedData = data
                .Select((item, index) => new { Item = item, Index = index })
                .OrderBy(x => ParsePriority(x.Item.Priority), Comparer<int?>.Default)
                .ThenBy(x => x.Index)
                .Select(x => x.Item)
                .ToList()
                .AsReadOnly();
            return sortedData;
        }

        private static int? ParsePriority(string priority)
        {
            if (int.TryParse(priority, out int result))
            {
                return result;
            }
            return null; // Вернет null, если не удалось распарсить как число
        }

        public static string SafeGet(IList<object> list, int index)
        {
            return (list.Count > index && list[index] != null) ? list[index].ToString()! : "";
        }
    }
}
