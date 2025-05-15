using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            $"winnum.org.app.WNApplicationInstance:{(int)id}";
        private static string FormatTagId(TagId id) =>
            $"winnum.org.tag.WNTag:{(int)id}";
        private string FormatWnId() =>
            $"winnum.org.product.WNProduct:{_machine.WnId}";

        public async Task<DateTime> GetPlatformDateTimeAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNCalendarHelper" },
                { "men", "getPlatformDateTime" }
            };

            return Parser.FromWinnumDateTime(Parser.ParseXmlItems(await _client.ExecuteRequestAsync(parameters)).First()["datetime"]);
        }

        public async Task<DateTime> GetCloudDateTimeAsync()
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNCalendarHelper" },
                { "men", "getCloudDateTime" }
            };

            return Parser.FromWinnumDateTime(Parser.ParseXmlItems(await _client.ExecuteRequestAsync(parameters)).First()["datetime"]);
        }

        /// <summary>
        /// Получение количества выполненных операций
        /// </summary>
        /// <param name="appId">Идентификатор приложения</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="tillDate">Конечная дата</param>
        /// <returns>Результат запроса в виде строки</returns>
        public async Task<string> GetOperationSummaryAsync(AppId appId, DateTime fromDate, DateTime tillDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNCNCApplicationCompletedQtyHelper" },
                { "men", "getOperationSummary" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return await _client.ExecuteRequestAsync(parameters);
        }

        /// <summary>
        /// Получение количества завершенных и не завершенных операций
        /// </summary>
        /// <param name="appId">Идентификатор приложения</param>
        /// <param name="fromDate">Начальная дата</param>
        /// <param name="tillDate">Конечная дата</param>
        /// <returns>Результат запроса в виде строки</returns>
        public async Task<string> GetCompletedQtyAsync(AppId appId, DateTime fromDate, DateTime tillDate)
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

        public async Task<string> GetSimpleTagIntervalCalculationAsync(AppId appId, TagId tagId, DateTime fromDate, DateTime tillDate, int? timeout = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNApplicationTagHelper" },
                { "men", "getSimpleTagIntervalCalculation" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "tid", FormatTagId(tagId) },
                { "from", fromDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) }
            };
            if (timeout.HasValue) parameters.Add("timeout", timeout.Value.ToString());

            return await _client.ExecuteRequestAsync(parameters);
        }

        public async Task<string> GetTagIntervalCalculationAsync(AppId appId, TagId tagId, DateTime fromDate, DateTime tillDate, int? timeout = null, bool? check_success = null, bool? base_shift = null)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNApplicationTagHelper" },
                { "men", "getTagIntervalCalculation" },
                { "appid", FormatAppId(appId) },
                { "pid", FormatWnId() },
                { "tid", FormatTagId(tagId) },
                { "from", fromDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) }
            };
            if (timeout.HasValue) parameters.Add("timeout", timeout.Value.ToString());
            if (check_success.HasValue) parameters.Add("check_success", check_success.Value.ToString());
            if (base_shift.HasValue) parameters.Add("base_shift", base_shift.Value.ToString());

            return await _client.ExecuteRequestAsync(parameters);
        }
    }
}