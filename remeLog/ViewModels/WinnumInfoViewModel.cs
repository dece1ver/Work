using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libeLog.Base;
using remeLog.Infrastructure.Winnum.Data;

namespace remeLog.ViewModels
{
    public class WinnumInfoViewModel : ViewModel
    {
        public WinnumInfoViewModel(string generalInfo, List<Dictionary<string, string>> dictList, List<PriorityTagDuration> priorityTagDurations) 
        {
            GeneralInfo = generalInfo;

            PriorityTagDurations = priorityTagDurations;

            var allKeys = dictList.SelectMany(d => d.Keys).Distinct().ToList();
            var table = new DataTable();
            foreach (var key in allKeys)
                table.Columns.Add(key);
            foreach (var dict in dictList)
            {
                var row = table.NewRow();
                foreach (var key in allKeys)
                    row[key] = dict.TryGetValue(key, out var value) ? value : "";
                table.Rows.Add(row);
            }
            Dicts = table.DefaultView;
        }

        public string GeneralInfo { get; set; }

        public List<PriorityTagDuration> PriorityTagDurations { get; set; }

        public DataView Dicts { get; set; }
    }
}
