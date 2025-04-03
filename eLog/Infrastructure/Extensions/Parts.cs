using DocumentFormat.OpenXml.Presentation;
using eLog.Models;
using libeLog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    public static class Parts
    {
        public async static Task<string> GetPositionInTasksList(this Part part, IProgress<(int, string)> progress)
        {
            var gs = new GoogleSheet(AppSettings.Instance.GoogleCredentialsPath, AppSettings.Instance.GsId);
            var partPosition = await gs.FindRowByValue(part.Order, AppSettings.Instance.Machine?.Name ?? "", AppSettings.Instance.Machines.Select(m => m.Name), progress);
            if (string.IsNullOrEmpty(partPosition) && part.Order.ToLowerInvariant() == "без м/л") partPosition = 
                    await gs.FindRowByValue(part.FullName, AppSettings.Instance.Machine?.Name ?? "", AppSettings.Instance.Machines.Select(m => m.Name), progress, 1);
            return partPosition;
        }
    }
}
