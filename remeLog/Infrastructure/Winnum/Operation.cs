using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using remeLog.Models;
using static remeLog.Infrastructure.Winnum.Types;

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

        private static string FormatAppId(AppId id) =>
            Uri.EscapeDataString($"winnum.org.app.WNApplicationInstance:{(int)id}");
        private static string FormatTagId(TagId id) =>
                    Uri.EscapeDataString($"winnum.org.tag.WNTag:{(int)id}");
        private string FormatWnId() =>
            Uri.EscapeDataString($"winnum.org.product.WNProduct:{_machine.WnId}");

        public async Task<string> GetCompletedOperationsAsync(AppId appId, DateTime fromDate, DateTime tillDate)
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

        public async Task<string> GetPriorityTagDurationAsync(AppId appId, DateTime fromDate, DateTime tillDate)
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

        public async Task<string> GetTagCalculationValueAsync(AppId appId, TagId tagId, DateTime fromDate, DateTime tillDate)
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

        public async Task<string> GetSimpleTagCalculationAsync(AppId appId, TagId tagId, DateTime fromDate, DateTime tillDate, int timeout = 10000)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNApplicationTagHelper" },
                { "men", "getSimpleTagCalculation" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "tid", FormatTagId(tagId) },
                { "from", fromDate.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture) },
                { "timeout", timeout.ToString() }
            };

            return await _client.ExecuteRequestAsync(parameters);
        }
    }
}