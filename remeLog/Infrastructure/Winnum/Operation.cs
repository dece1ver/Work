using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using remeLog.Models;

namespace remeLog.Infrastructure.Winnum
{
    public class Operation
    {
        private readonly ApiClient _client;
        private readonly Machine _machine;

        public Operation(ApiClient client, Machine machine)
        {
            _client = client;
            _machine = machine;
        }

        private static string FormatAppId(int id) =>
            Uri.EscapeDataString($"winnum.org.app.WNApplicationInstance:{id}");
        private static string FormatTagId(int id) =>
                    Uri.EscapeDataString($"winnum.org.tag.WNTag:{id}");
        private string FormatWnId() =>
            Uri.EscapeDataString($"winnum.org.product.WNProduct:{_machine.WnId}");

        public async Task<string> GetCompletedOperationsAsync(int appId, DateTime fromDate, DateTime tillDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNCNCApplicationCompletedQtyHelper" },
                { "men", "getCompletedQty" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return await _client.ExecuteRequestAsync(parameters);
        }

        public async Task<string> GetPriorityTagDurationAsync(int appId, DateTime fromDate, DateTime tillDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNApplicationTagHelper" },
                { "men", "getPriorityTagDuration" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return await _client.ExecuteRequestAsync(parameters);
        }

        public async Task<string> GetTagCalculationValueAsync(int appId, int tagId, DateTime fromDate, DateTime tillDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNApplicationTagHelper" },
                { "men", "getTagCalculationValue" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "tid", FormatTagId(tagId) },
                { "from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return await _client.ExecuteRequestAsync(parameters);
        }
    }
}
