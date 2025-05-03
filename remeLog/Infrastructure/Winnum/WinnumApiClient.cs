using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Features;
using System.Web;
using System.Windows.Forms;
using remeLog.Models;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Net.Http.Headers;

namespace remeLog.Infrastructure.Winnum
{
    public class WinnumApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly Machine _machine;
        private readonly string _usr;
        private readonly string _pwd;

        public WinnumApiClient(string baseUrl, Machine machine, string usr, string pwd)
        {
            _baseUrl = baseUrl;
            _machine = machine;
            _httpClient = new HttpClient();
            _usr = usr;
            _pwd = pwd;
            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_usr}:{_pwd}")));
        }

        private string FormatWnId() => Uri.EscapeDataString($"winnum.org.product.WNProduct:{_machine.WnId}");
        private string FormatWnUuid() => Uri.EscapeDataString(_machine.WnUuid.ToString());

        private async Task<string> ExecuteRequestAsync(Dictionary<string, string> parameters)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["usr"] = _usr;
            query["pwd"] = _pwd;
            foreach (var param in parameters)
            {
                query[param.Key] = param.Value;
            }

            var url = $"{_baseUrl}?{query}&mode=yes";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // Сохранение значения сигнала
        public async Task SaveSignalAsync(string signal, string value, string pid = null, DateTime? eventTime = null)
        {
            var parameters = new Dictionary<string, string>
        {
            { "rpc", "winnum.views.url.WNConnectorHelper" },
            { "men", "saveSignal" },
            { "uuid", FormatWnUuid() },
            { "signal", signal },
            { "value", value }
        };

            if (!string.IsNullOrEmpty(pid))
                parameters.Add("pid", pid);

            if (eventTime.HasValue)
                parameters.Add("event", eventTime.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            await ExecuteRequestAsync(parameters);
        }

        // Получение значений сигнала
        public async Task<string> GetSignalAsync(
            string signal,
            string stype,
            string order,
            DateTime? start = null,
            DateTime? end = null,
            int? count = null)
        {
            var parameters = new Dictionary<string, string>
        {
            { "rpc", "winnum.views.url.WNConnectorHelper" },
            { "men", "getSignal" },
            { "uuid", FormatWnUuid() },
            { "signal", signal },
            { "stype", stype },
            { "order", order }
        };

            if (start.HasValue)
                parameters.Add("start", start.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            if (end.HasValue)
                parameters.Add("end", end.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            if (count.HasValue)
                parameters.Add("count", count.Value.ToString());

            return await ExecuteRequestAsync(parameters);
        }

        // Получение уникальных значений сигнала
        public async Task<string> GetUniqSignalsAsync(
            string signal,
            string order,
            DateTime start,
            DateTime end)
        {
            var parameters = new Dictionary<string, string>
        {
            { "rpc", "winnum.views.url.WNConnectorHelper" },
            { "men", "getUniqSignals" },
            { "uuid", FormatWnUuid() },
            { "signal", signal },
            { "order", order },
            { "start", start.ToString("yyyy-MM-dd HH:mm:ss.fff") },
            { "end", end.ToString("yyyy-MM-dd HH:mm:ss.fff") }
        };

            return await ExecuteRequestAsync(parameters);
        }

        // Отправка значения сигнала на контроллер
        public async Task SendSignalAsync(string signalId, string equipmentId, string signalValue, string forwardSerial = null)
        {
            var parameters = new Dictionary<string, string>
        {
            { "rpc", "winnum.views.url.WNConnectorHelper" },
            { "men", "sendSignal" },
            { "oid", signalId },
            { "pid", equipmentId },
            { "signalValue", signalValue }
        };

            if (!string.IsNullOrEmpty(forwardSerial))
                parameters.Add("forwardSerial", forwardSerial);

            await ExecuteRequestAsync(parameters);
        }

        // Получение количества выполненных операций
        public async Task<string> GetCompletedOperationsAsync(
            string appId,
            DateTime fromDate,
            DateTime tillDate)
        {
            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNCNCApplicationCompletedQtyHelper" },
                { "men", "getCompletedQty" },
                { "appid", Uri.EscapeDataString(appId) },
                { "pid", FormatWnId() },
                { "from", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                { "till", tillDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
            };

            return await ExecuteRequestAsync(parameters);
        }
    }
}
