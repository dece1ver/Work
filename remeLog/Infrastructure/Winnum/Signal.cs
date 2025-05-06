using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using remeLog.Models;

namespace remeLog.Infrastructure.Winnum
{
    public class Signal
    {
        private readonly ApiClient _client;
        private readonly Machine _machine;

        public Signal(ApiClient client, Machine machine)
        {
            _client = client;
            _machine = machine;
        }

        private string FormatWnUuid() =>
            Uri.EscapeDataString(_machine.WnUuid.ToString());

        public async Task SaveSignalAsync(string signal, string value, string? pid = null, DateTime? eventTime = null)
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

            await _client.ExecuteRequestAsync(parameters);
        }

        public async Task<string> GetSignalAsync(string signal, string stype, string order,
            DateTime? start = null, DateTime? end = null, int? count = null)
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

            return await _client.ExecuteRequestAsync(parameters);
        }

        public async Task<string> GetUniqSignalsAsync(string signal, string order, DateTime start, DateTime end)
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

            return await _client.ExecuteRequestAsync(parameters);
        }

        public async Task SendSignalAsync(string signalId, string equipmentId, string signalValue, string? forwardSerial = null)
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

            await _client.ExecuteRequestAsync(parameters);
        }
    }
}
