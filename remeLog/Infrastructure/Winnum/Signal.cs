using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using remeLog.Models;
using static remeLog.Infrastructure.Winnum.Types;

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

        private static string FormatSignalType(SignalType type) =>
        type switch
        {
            SignalType.ByCount => "bycount",
            SignalType.ByTime => "bytime",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Неизвестное значение перечисления: {type}")
        };

        private static string FormatOrder(Order order) =>
        order switch
        {
            Order.Asc => "asc",
            Order.Desc => "desc",
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, $"Неизвестное значение перечисления: {order}")
        };

        private string FormatWnUuid() =>
            _machine.WnUuid.ToString().ToUpperInvariant();

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

        public async Task<string> GetSignalAsync(string signal, SignalType stype, Order order,
            DateTime? start = null, DateTime? end = null, int? count = null)
        {
            switch (stype)
            {
                case SignalType.ByCount:
                    if (!count.HasValue)
                        throw new ArgumentException("Для сигнала с типом 'ByCount' необходимо указать параметр 'count'.", nameof(count));
                    break;

                case SignalType.ByTime:
                    if (!start.HasValue || !end.HasValue)
                        throw new ArgumentException("Для сигнала с типом 'ByTime' необходимо указать параметры 'start' и 'end'.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(stype), stype, $"Неизвестное значение перечисления: {stype}");
            }

            var parameters = new Dictionary<string, string>
            {
                { "rpc", "winnum.views.url.WNConnectorHelper" },
                { "men", "getSignal" },
                { "uuid", FormatWnUuid() },
                { "signal", signal },
                { "stype", FormatSignalType(stype) },
                { "order", FormatOrder(order) }
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
