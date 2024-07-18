using DocumentFormat.OpenXml.Presentation;
using eLog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Infrastructure.Extensions
{
    public static class Parts
    {
        public async static Task<string> GetPositionInTasksList(this Part part, IProgress<int> progress)
        {
            var partPosition = await GoogleSheets.FindRowByValue(part.Order, progress);
            if (string.IsNullOrEmpty(partPosition) && part.Order.ToLowerInvariant() == "без м/л") partPosition = await GoogleSheets.FindRowByValue(part.FullName, progress, 1);
            return partPosition;
        }
    }
}
